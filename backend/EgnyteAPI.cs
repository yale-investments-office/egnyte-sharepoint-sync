using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace EgnyteSharePointSync
{
    public static class EgnyteAPI
    {
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("GetEgnyteAuth")]
        public static async Task<IActionResult> GetEgnyteAuth(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "egnyte/auth")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting Egnyte authentication URL");

            try
            {
                var config = new ConfigurationService(log);
                var domain = config.GetEgnyteDomain();
                var clientId = await config.GetEgnyteClientIdAsync();
                var redirectUri = config.GetEgnyteRedirectUri();

                var authUrl = $"https://{domain}.egnyte.com/puboauth/token" +
                             $"?client_id={clientId}" +
                             $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                             "&response_type=code" +
                             "&scope=Egnyte.filesystem" +
                             "&state=egnyte";

                return new OkObjectResult(new { 
                    success = true, 
                    data = new { authUrl } 
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error getting Egnyte auth URL: {ex.Message}");
                return new OkObjectResult(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [FunctionName("ExchangeEgnyteCode")]
        public static async Task<IActionResult> ExchangeEgnyteCode(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "egnyte/token")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Exchanging Egnyte code for token");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                
                var code = data?.code?.ToString();

                if (string.IsNullOrEmpty(code))
                {
                    return new OkObjectResult(new { 
                        success = false, 
                        error = "Authorization code is required" 
                    });
                }

                var config = new ConfigurationService(log);
                var domain = config.GetEgnyteDomain();
                var clientId = await config.GetEgnyteClientIdAsync();
                var clientSecret = await config.GetEgnyteClientSecretAsync();
                var redirectUri = config.GetEgnyteRedirectUri();

                var tokenRequest = new Dictionary<string, string>
                {
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"redirect_uri", redirectUri},
                    {"grant_type", "authorization_code"},
                    {"code", code}
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await httpClient.PostAsync($"https://{domain}.egnyte.com/puboauth/token", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var tokenData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    return new OkObjectResult(new { 
                        success = true, 
                        data = new { 
                            token = tokenData?.access_token?.ToString(),
                            tokenType = tokenData?.token_type?.ToString(),
                            expiresIn = tokenData?.expires_in
                        }
                    });
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                log.LogError($"Token exchange failed: {response.StatusCode} - {errorContent}");
                
                return new OkObjectResult(new { 
                    success = false, 
                    error = "Failed to exchange authorization code for token" 
                });
            }
            catch (Exception ex)
            {
                log.LogError($"Error exchanging Egnyte code: {ex.Message}");
                return new OkObjectResult(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [FunctionName("GetEgnyteFiles")]
        public static async Task<IActionResult> GetEgnyteFiles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "egnyte/files")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting Egnyte files and folders");

            try
            {
                var accessToken = req.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var domain = req.Query["domain"];
                var path = req.Query["path"].ToString() ?? "/Shared";

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(domain))
                {
                    return new BadRequestObjectResult("Access token and domain are required");
                }

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await httpClient.GetAsync($"https://{domain}.egnyte.com/pubapi/v1/fs{path}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return new OkObjectResult(JsonConvert.DeserializeObject(responseContent));
                }

                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                log.LogError($"Error getting Egnyte files: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }

        [FunctionName("DownloadEgnyteFile")]
        public static async Task<IActionResult> DownloadEgnyteFile(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "egnyte/download")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Downloading file from Egnyte");

            try
            {
                var accessToken = req.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var domain = req.Query["domain"];
                var filePath = req.Query["path"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(filePath))
                {
                    return new BadRequestObjectResult("Access token, domain, and file path are required");
                }

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await httpClient.GetAsync($"https://{domain}.egnyte.com/pubapi/v1/fs-content{filePath}");
                
                if (response.IsSuccessStatusCode)
                {
                    var fileContent = await response.Content.ReadAsByteArrayAsync();
                    var fileName = Path.GetFileName(filePath);
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    
                    return new FileContentResult(fileContent, contentType)
                    {
                        FileDownloadName = fileName
                    };
                }

                return new StatusCodeResult((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                log.LogError($"Error downloading Egnyte file: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
