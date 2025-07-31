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
    public static class MockEgnyteAPI
    {
        [FunctionName("MockGetEgnyteFiles")]
        public static async Task<IActionResult> MockGetEgnyteFiles(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/egnyte/files")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting mock Egnyte files");

            await Task.Delay(400); // Simulate API delay

            var path = req.Query["path"].ToString();
            if (string.IsNullOrEmpty(path))
                path = "/";
            
            log.LogInformation($"Mock browsing path: {path}");

            var mockData = GetMockFolderData(path);

            return new OkObjectResult(new { success = true, data = mockData });
        }

        [FunctionName("MockEgnyteAuth")]
        public static async Task<IActionResult> MockEgnyteAuth(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "mock/egnyte/auth-url")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting mock Egnyte auth URL");

            await Task.Delay(100);

            return new OkObjectResult(new 
            { 
                success = true, 
                data = new 
                { 
                    authUrl = "http://localhost:3000?mock=egnyte&code=mock_egnyte_code_12345&state=egnyte" 
                } 
            });
        }

        [FunctionName("MockEgnyteExchange")]
        public static async Task<IActionResult> MockEgnyteExchange(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "mock/egnyte/exchange-code")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Mock exchanging Egnyte code for token");

            await Task.Delay(200);

            return new OkObjectResult(new 
            { 
                success = true, 
                data = new 
                { 
                    token = "mock_egnyte_token_" + Guid.NewGuid().ToString()[..8] 
                } 
            });
        }

        private static object GetMockFolderData(string path)
        {
            switch (path)
            {
                case "/":
                case "":
                    return new
                    {
                        name = "Root",
                        path = "/",
                        folders = new[]
                        {
                            new { name = "Documents", path = "/Shared/Documents" },
                            new { name = "Projects", path = "/Shared/Projects" },
                            new { name = "Marketing", path = "/Shared/Marketing" },
                            new { name = "HR", path = "/Shared/HR" }
                        },
                        files = new[]
                        {
                            new 
                            { 
                                name = "Company Overview.pdf", 
                                path = "/Shared/Company Overview.pdf",
                                is_folder = false,
                                size = 2048576,
                                lastModified = DateTime.UtcNow.AddDays(-5).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "abc123def456"
                            },
                            new 
                            { 
                                name = "Contact List.xlsx", 
                                path = "/Shared/Contact List.xlsx",
                                is_folder = false,
                                size = 45632,
                                lastModified = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "def789ghi012"
                            }
                        }
                    };

                case "/Shared/Documents":
                    return new
                    {
                        name = "Documents",
                        path = "/Shared/Documents",
                        folders = new[]
                        {
                            new { name = "Policies", path = "/Shared/Documents/Policies" },
                            new { name = "Templates", path = "/Shared/Documents/Templates" }
                        },
                        files = new[]
                        {
                            new 
                            { 
                                name = "Employee Handbook.pdf", 
                                path = "/Shared/Documents/Employee Handbook.pdf",
                                is_folder = false,
                                size = 5242880,
                                lastModified = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "handbook123"
                            },
                            new 
                            { 
                                name = "Code of Conduct.docx", 
                                path = "/Shared/Documents/Code of Conduct.docx",
                                is_folder = false,
                                size = 98304,
                                lastModified = DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "conduct456"
                            },
                            new 
                            { 
                                name = "Safety Guidelines.pdf", 
                                path = "/Shared/Documents/Safety Guidelines.pdf",
                                is_folder = false,
                                size = 1048576,
                                lastModified = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "safety789"
                            }
                        }
                    };

                case "/Shared/Projects":
                    return new
                    {
                        name = "Projects",
                        path = "/Shared/Projects",
                        folders = new[]
                        {
                            new { name = "Alpha Project", path = "/Shared/Projects/Alpha Project" },
                            new { name = "Beta Launch", path = "/Shared/Projects/Beta Launch" },
                            new { name = "Archive", path = "/Shared/Projects/Archive" }
                        },
                        files = new[]
                        {
                            new 
                            { 
                                name = "Project Status.xlsx", 
                                path = "/Shared/Projects/Project Status.xlsx",
                                is_folder = false,
                                size = 87654,
                                lastModified = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "status123"
                            }
                        }
                    };

                case "/Shared/Projects/Alpha Project":
                    return new
                    {
                        name = "Alpha Project",
                        path = "/Shared/Projects/Alpha Project",
                        folders = new object[0],
                        files = new[]
                        {
                            new 
                            { 
                                name = "Requirements.docx", 
                                path = "/Shared/Projects/Alpha Project/Requirements.docx",
                                is_folder = false,
                                size = 156789,
                                lastModified = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "req123"
                            },
                            new 
                            { 
                                name = "Design Mockups.pptx", 
                                path = "/Shared/Projects/Alpha Project/Design Mockups.pptx",
                                is_folder = false,
                                size = 12582912,
                                lastModified = DateTime.UtcNow.AddHours(-6).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "design456"
                            },
                            new 
                            { 
                                name = "Budget Analysis.xlsx", 
                                path = "/Shared/Projects/Alpha Project/Budget Analysis.xlsx",
                                is_folder = false,
                                size = 67890,
                                lastModified = DateTime.UtcNow.AddDays(-4).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "budget789"
                            }
                        }
                    };

                case "/Shared/Marketing":
                    return new
                    {
                        name = "Marketing",
                        path = "/Shared/Marketing",
                        folders = new[]
                        {
                            new { name = "Campaigns", path = "/Shared/Marketing/Campaigns" },
                            new { name = "Brand Assets", path = "/Shared/Marketing/Brand Assets" }
                        },
                        files = new[]
                        {
                            new 
                            { 
                                name = "Marketing Strategy 2024.pptx", 
                                path = "/Shared/Marketing/Marketing Strategy 2024.pptx",
                                is_folder = false,
                                size = 8388608,
                                lastModified = DateTime.UtcNow.AddDays(-20).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "strategy2024"
                            },
                            new 
                            { 
                                name = "Customer Demographics.xlsx", 
                                path = "/Shared/Marketing/Customer Demographics.xlsx",
                                is_folder = false,
                                size = 234567,
                                lastModified = DateTime.UtcNow.AddDays(-8).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                checksum = "demographics123"
                            }
                        }
                    };

                default:
                    return new
                    {
                        name = "Unknown Folder",
                        path = path,
                        folders = new object[0],
                        files = new object[0]
                    };
            }
        }
    }
}
