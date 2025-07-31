using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace EgnyteSPOSync
{
    public static class MockSharePointAPI
    {
        [FunctionName("MockGetSharePointSites")]
        public static async Task<IActionResult> MockGetSharePointSites(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/sharepoint/sites")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting mock SharePoint sites");

            await Task.Delay(500); // Simulate API delay

            var mockSites = new List<object>
            {
                new
                {
                    id = "contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321",
                    displayName = "Company Portal",
                    webUrl = "https://contoso.sharepoint.com/sites/company-portal",
                    isDefault = false
                },
                new
                {
                    id = "contoso.sharepoint.com,11111111-2222-3333-4444-555555555555,66666666-7777-8888-9999-000000000000",
                    displayName = "Project Alpha",
                    webUrl = "https://contoso.sharepoint.com/sites/project-alpha",
                    isDefault = false
                },
                new
                {
                    id = "contoso.sharepoint.com,aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee,ffffffff-gggg-hhhh-iiii-jjjjjjjjjjjj",
                    displayName = "HR Documents",
                    webUrl = "https://contoso.sharepoint.com/sites/hr-documents",
                    isDefault = false
                },
                new
                {
                    id = "contoso.sharepoint.com,99999999-8888-7777-6666-555555555555,44444444-3333-2222-1111-000000000000",
                    displayName = "Team Collaboration",
                    webUrl = "https://contoso.sharepoint.com/sites/team-collab",
                    isDefault = false
                }
            };

            return new OkObjectResult(new { success = true, data = mockSites });
        }

        [FunctionName("MockGetSharePointLibraries")]
        public static async Task<IActionResult> MockGetSharePointLibraries(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/sharepoint/libraries")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting mock SharePoint libraries");

            await Task.Delay(300); // Simulate API delay

            var siteId = req.Query["siteId"];
            var mockLibraries = new List<object>();

            // Return different libraries based on site ID
            switch (siteId.ToString())
            {
                case "contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321":
                    mockLibraries.AddRange(new[]
                    {
                        new
                        {
                            id = "lib-12345678-1234-1234-1234-123456789012",
                            name = "Documents",
                            displayName = "Shared Documents",
                            webUrl = "https://contoso.sharepoint.com/sites/company-portal/Shared%20Documents",
                            isDefault = false
                        },
                        new
                        {
                            id = "lib-87654321-4321-4321-4321-210987654321",
                            name = "Policies",
                            displayName = "Company Policies",
                            webUrl = "https://contoso.sharepoint.com/sites/company-portal/Policies",
                            isDefault = false
                        }
                    });
                    break;

                case "contoso.sharepoint.com,11111111-2222-3333-4444-555555555555,66666666-7777-8888-9999-000000000000":
                    mockLibraries.AddRange(new[]
                    {
                        new
                        {
                            id = "lib-11111111-2222-3333-4444-555555555555",
                            name = "Documents",
                            displayName = "Project Documents",
                            webUrl = "https://contoso.sharepoint.com/sites/project-alpha/Documents",
                            isDefault = false
                        },
                        new
                        {
                            id = "lib-66666666-7777-8888-9999-000000000000",
                            name = "Specifications",
                            displayName = "Technical Specifications",
                            webUrl = "https://contoso.sharepoint.com/sites/project-alpha/Specifications",
                            isDefault = false
                        },
                        new
                        {
                            id = "lib-projectalpha-meetings",
                            name = "Meetings",
                            displayName = "Meeting Notes",
                            webUrl = "https://contoso.sharepoint.com/sites/project-alpha/Meetings",
                            isDefault = false
                        }
                    });
                    break;

                case "contoso.sharepoint.com,aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee,ffffffff-gggg-hhhh-iiii-jjjjjjjjjjjj":
                    mockLibraries.AddRange(new[]
                    {
                        new
                        {
                            id = "lib-hr-documents",
                            name = "Documents",
                            displayName = "HR Documents",
                            webUrl = "https://contoso.sharepoint.com/sites/hr-documents/Documents",
                            isDefault = false
                        },
                        new
                        {
                            id = "lib-hr-forms",
                            name = "Forms",
                            displayName = "Employee Forms",
                            webUrl = "https://contoso.sharepoint.com/sites/hr-documents/Forms",
                            isDefault = false
                        }
                    });
                    break;

                default:
                    mockLibraries.AddRange(new[]
                    {
                        new
                        {
                            id = "lib-default-documents",
                            name = "Documents",
                            displayName = "Shared Documents",
                            webUrl = "https://contoso.sharepoint.com/sites/default/Documents",
                            isDefault = false
                        }
                    });
                    break;
            }

            return new OkObjectResult(new { success = true, data = mockLibraries });
        }

        [FunctionName("MockSyncFiles")]
        public static async Task<IActionResult> MockSyncFiles(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mock/sharepoint/sync")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Mock syncing files to SharePoint");

            // Simulate processing time
            await Task.Delay(2000);

            var mockSyncResult = new
            {
                success = true,
                data = new
                {
                    id = Guid.NewGuid().ToString(),
                    status = "completed",
                    progress = 100,
                    startTime = DateTime.UtcNow.AddSeconds(-2),
                    endTime = DateTime.UtcNow,
                    results = new[]
                    {
                        new
                        {
                            fileName = "document1.pdf",
                            status = "completed",
                            message = "File synced successfully to SharePoint",
                            targetSite = "contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321",
                            targetLibrary = "lib-12345678-1234-1234-1234-123456789012"
                        },
                        new
                        {
                            fileName = "spreadsheet.xlsx",
                            status = "completed",
                            message = "File synced successfully to SharePoint",
                            targetSite = "contoso.sharepoint.com,12345678-1234-1234-1234-123456789012,87654321-4321-4321-4321-210987654321",
                            targetLibrary = "lib-12345678-1234-1234-1234-123456789012"
                        }
                    }
                }
            };

            return new OkObjectResult(mockSyncResult);
        }

        [FunctionName("MockSharePointConfig")]
        public static async Task<IActionResult> MockSharePointConfig(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/sharepoint/config")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting mock SharePoint configuration");

            await Task.Delay(100);

            var mockConfig = new
            {
                success = true,
                data = new
                {
                    hasDefaultSite = false, // No default site for full dynamic selection
                    defaultSite = new
                    {
                        url = "",
                        id = "",
                        libraryName = "",
                        libraryId = ""
                    },
                    allowSiteSelection = true, // Full dynamic selection enabled
                    mockMode = true // Indicate this is mock data
                }
            };

            return new OkObjectResult(mockConfig);
        }
    }
}
