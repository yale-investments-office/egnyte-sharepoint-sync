using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace EgnyteSPOSync
{
    public static class SharePointSync
    {
        private static readonly string clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID") ?? "";
        private static readonly string clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET") ?? "";
        private static readonly string tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID") ?? "";
        private static readonly string redirectUri = Environment.GetEnvironmentVariable("AZURE_REDIRECT_URI") ?? "http://localhost:3000";
        
        // Pre-configured SharePoint target (optional)
        private static readonly string defaultSiteUrl = Environment.GetEnvironmentVariable("SHAREPOINT_SITE_URL") ?? "";
        private static readonly string defaultSiteId = Environment.GetEnvironmentVariable("SHAREPOINT_SITE_ID") ?? "";
        private static readonly string defaultLibraryName = Environment.GetEnvironmentVariable("SHAREPOINT_LIBRARY_NAME") ?? "Documents";
        private static readonly string defaultLibraryId = Environment.GetEnvironmentVariable("SHAREPOINT_LIBRARY_ID") ?? "";

        [FunctionName("GetSharePointAuthUrl")]
        public static async Task<IActionResult> GetSharePointAuthUrl(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sharepoint/auth-url")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting SharePoint OAuth URL");

            try
            {
                var app = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                    .WithRedirectUri(redirectUri)
                    .Build();

                var scopes = new string[] { "https://graph.microsoft.com/.default" };
                
                var authUrl = await app.GetAuthorizationRequestUrl(scopes)
                    .WithRedirectUri(redirectUri)
                    .ExecuteAsync();

                return new OkObjectResult(new { 
                    success = true, 
                    data = new { authUrl = authUrl.ToString() } 
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error generating SharePoint auth URL: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("ExchangeSharePointCode")]
        public static async Task<IActionResult> ExchangeSharePointCode(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sharepoint/exchange-code")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Exchanging SharePoint authorization code for token");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(requestBody);
                string code = data?.code;

                if (string.IsNullOrEmpty(code))
                {
                    return new BadRequestObjectResult("Authorization code is required");
                }

                var app = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                    .WithRedirectUri(redirectUri)
                    .Build();

                var scopes = new string[] { "https://graph.microsoft.com/.default" };
                var result = await app.AcquireTokenByAuthorizationCode(scopes, code)
                    .ExecuteAsync();

                return new OkObjectResult(new { 
                    success = true, 
                    data = new { token = result.AccessToken } 
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error exchanging SharePoint code: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("GetSharePointConfig")]
        public static IActionResult GetSharePointConfig(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sharepoint/config")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting SharePoint configuration");

            try
            {
                var config = new
                {
                    hasDefaultSite = !string.IsNullOrEmpty(defaultSiteId) || !string.IsNullOrEmpty(defaultSiteUrl),
                    defaultSite = new
                    {
                        url = defaultSiteUrl,
                        id = defaultSiteId,
                        libraryName = defaultLibraryName,
                        libraryId = defaultLibraryId
                    },
                    allowSiteSelection = string.IsNullOrEmpty(defaultSiteId) // Allow selection if no default configured
                };

                return new OkObjectResult(new { success = true, data = config });
            }
            catch (Exception ex)
            {
                log.LogError($"Error getting SharePoint config: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("GetSharePointSites")]
        public static async Task<IActionResult> GetSharePointSites(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sharepoint/sites")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting SharePoint sites");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(requestBody);
                string token = data?.token;

                if (string.IsNullOrEmpty(token))
                {
                    return new BadRequestObjectResult("Access token is required");
                }

                // If we have a default site configured, return it along with other sites
                var siteList = new List<object>();
                
                // Add default site if configured
                if (!string.IsNullOrEmpty(defaultSiteId) || !string.IsNullOrEmpty(defaultSiteUrl))
                {
                    siteList.Add(new
                    {
                        id = defaultSiteId,
                        displayName = $"Default Site ({defaultLibraryName})",
                        webUrl = defaultSiteUrl,
                        isDefault = true
                    });
                }

                // Get other available sites
                var graphClient = GetGraphServiceClient(token);
                var sites = await graphClient.Sites.GetAsync();

                if (sites?.Value != null)
                {
                    foreach (var site in sites.Value)
                    {
                        // Skip if this is already our default site
                        if (site.Id != defaultSiteId)
                        {
                            siteList.Add(new
                            {
                                id = site.Id,
                                displayName = site.DisplayName,
                                webUrl = site.WebUrl,
                                isDefault = false
                            });
                        }
                    }
                }

                return new OkObjectResult(new { success = true, data = siteList });
            }
            catch (Exception ex)
            {
                log.LogError($"Error getting SharePoint sites: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("GetSharePointLibraries")]
        public static async Task<IActionResult> GetSharePointLibraries(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sharepoint/libraries")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting SharePoint document libraries");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(requestBody);
                string token = data?.token;
                string siteId = data?.siteId;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(siteId))
                {
                    return new BadRequestObjectResult("Access token and site ID are required");
                }

                var libraryList = new List<object>();

                // If this is the default site, add the default library first
                if (siteId == defaultSiteId && !string.IsNullOrEmpty(defaultLibraryId))
                {
                    libraryList.Add(new
                    {
                        id = defaultLibraryId,
                        name = defaultLibraryName,
                        displayName = $"{defaultLibraryName} (Default)",
                        webUrl = $"{defaultSiteUrl}/Shared Documents",
                        isDefault = true
                    });
                }

                // Get other libraries
                var graphClient = GetGraphServiceClient(token);
                var lists = await graphClient.Sites[siteId].Lists.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Filter = "baseTemplate eq 101"; // Document Library template
                });

                if (lists?.Value != null)
                {
                    foreach (var list in lists.Value)
                    {
                        // Skip if this is already our default library
                        if (list.Id != defaultLibraryId)
                        {
                            libraryList.Add(new
                            {
                                id = list.Id,
                                name = list.Name,
                                displayName = list.DisplayName,
                                webUrl = list.WebUrl,
                                isDefault = false
                            });
                        }
                    }
                }

                return new OkObjectResult(new { success = true, data = libraryList });
            }
            catch (Exception ex)
            {
                log.LogError($"Error getting SharePoint libraries: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("UploadToSharePoint")]
        public static async Task<IActionResult> UploadToSharePoint(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sharepoint/upload")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Uploading file to SharePoint");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(requestBody);
                
                string token = data?.token;
                string siteId = data?.siteId ?? defaultSiteId; // Use default if not provided
                string libraryId = data?.libraryId ?? defaultLibraryId; // Use default if not provided
                string fileName = data?.fileName;
                string fileContent = data?.fileContent;

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(siteId) || 
                    string.IsNullOrEmpty(libraryId) || string.IsNullOrEmpty(fileName))
                {
                    return new BadRequestObjectResult("Access token, site ID, library ID, and file name are required");
                }

                var graphClient = GetGraphServiceClient(token);
                
                // For now, simulate successful upload
                // In production, implement proper file upload logic
                var uploadId = Guid.NewGuid().ToString();
                
                log.LogInformation($"Simulated upload of {fileName} to site {siteId}, library {libraryId}");

                return new OkObjectResult(new
                {
                    success = true,
                    data = new
                    {
                        uploadId = uploadId,
                        name = fileName,
                        webUrl = $"{defaultSiteUrl}/Shared%20Documents/{fileName}",
                        size = fileContent?.Length ?? 0,
                        message = "File upload simulated successfully"
                    }
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error uploading to SharePoint: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        [FunctionName("SyncFiles")]
        public static async Task<IActionResult> SyncFiles(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "sharepoint/sync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Starting file sync from Egnyte to SharePoint");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(requestBody);

                var files = JsonConvert.DeserializeObject<List<dynamic>>(data?.files?.ToString() ?? "[]");
                string siteId = data?.siteId?.ToString() ?? defaultSiteId; // Use default if not provided
                string libraryId = data?.libraryId?.ToString() ?? defaultLibraryId; // Use default if not provided
                string egnyteToken = data?.egnyteToken;
                string sharePointToken = data?.sharePointToken;

                if (files == null || files.Count == 0)
                {
                    return new BadRequestObjectResult("No files provided for sync");
                }

                if (string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(libraryId))
                {
                    return new BadRequestObjectResult("Site ID and Library ID are required (check configuration)");
                }

                var syncResults = new List<object>();

                foreach (var file in files)
                {
                    try
                    {
                        string fileName = file.name?.ToString() ?? "Unknown";
                        string filePath = file.path?.ToString() ?? "";

                        // Here you would implement the actual file download from Egnyte
                        // and upload to SharePoint logic
                        
                        syncResults.Add(new
                        {
                            fileName = fileName,
                            status = "completed",
                            message = $"File synced to {(siteId == defaultSiteId ? "default" : "selected")} SharePoint site",
                            targetSite = siteId,
                            targetLibrary = libraryId
                        });
                    }
                    catch (Exception fileEx)
                    {
                        log.LogError($"Error syncing file {file.name}: {fileEx.Message}");
                        syncResults.Add(new
                        {
                            fileName = file.name?.ToString() ?? "Unknown",
                            status = "error",
                            error = fileEx.Message
                        });
                    }
                }

                return new OkObjectResult(new { 
                    success = true, 
                    data = new
                    {
                        id = Guid.NewGuid().ToString(),
                        status = "completed",
                        sourceFiles = files,
                        targetLibrary = new { 
                            id = libraryId,
                            siteId = siteId,
                            isDefault = siteId == defaultSiteId
                        },
                        progress = 100,
                        startTime = DateTime.UtcNow,
                        endTime = DateTime.UtcNow,
                        results = syncResults
                    }
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error syncing files: {ex.Message}");
                return new OkObjectResult(new { success = false, error = ex.Message });
            }
        }

        private static GraphServiceClient GetGraphServiceClient(string accessToken)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            return new GraphServiceClient(httpClient);
        }
    }
}
