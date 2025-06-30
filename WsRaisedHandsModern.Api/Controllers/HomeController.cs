using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.ViewModels;

namespace WsRaisedHandsModern.Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        // GET: HomeController
        [Route("")]
        [Route("home")]
        [Route("index")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("about")]
        public ActionResult About()
        {
            return View(); // Returns Views/Home/About.cshtml
        }

        /*[Route("dashboard")]
        // [Authorize] // Require authentication for dashboard
        public ActionResult Dashboard()
        {
            return View(); // Returns Views/Home/Dashboard.cshtml
        }*/
        
        [Route("dashboard")]
        public IActionResult Dashboard()
        {
            var model = new DashboardViewModel
            {
                SystemStatus = new SystemStatus
                {
                    Status = "Online",
                    ActiveConnections = 42,
                    LastUpdated = DateTime.Now,
                    Services = new List<ServiceStatus>
                    {
                        new ServiceStatus { Name = "Database", Status = "Online", ResponseTime = TimeSpan.FromMilliseconds(45) },
                        new ServiceStatus { Name = "API Gateway", Status = "Online", ResponseTime = TimeSpan.FromMilliseconds(120) },
                        new ServiceStatus { Name = "Cache Server", Status = "Warning", ResponseTime = TimeSpan.FromMilliseconds(300) },
                        new ServiceStatus { Name = "File Storage", Status = "Online", ResponseTime = TimeSpan.FromMilliseconds(80) }
                    }
                },
                Sections = new List<DashboardSection>
                {
                    new DashboardSection
                    {
                        Id = "user-management",
                        Title = "User Management",
                        Icon = "bi-people",
                        Color = "primary",
                        Endpoints = new List<ApiEndpoint>
                        {
                            new ApiEndpoint { Id = "list-users", Title = "List Users", Description = "View all users", Url = "/Admin/Index", Icon = "bi-person-lines-fill", ButtonColor = "info" },
                            new ApiEndpoint { Id = "create-user", Title = "Create User", Description = "Add new user", Url = "/Admin/Create", Icon = "bi-person-plus", ButtonColor = "success" },
                            new ApiEndpoint { Id = "user-roles", Title = "Manage Roles", Description = "User role management", Url = "/Admin/Roles", Icon = "bi-shield-check", ButtonColor = "warning" }
                        }
                    },
                    new DashboardSection
                    {
                        Id = "system-monitoring",
                        Title = "System Monitoring",
                        Icon = "bi-graph-up",
                        Color = "success",
                        Endpoints = new List<ApiEndpoint>
                        {
                            new ApiEndpoint { Id = "system-health", Title = "System Health", Description = "Check system status", Url = "/api/health", Icon = "bi-heart-pulse", ButtonColor = "success", Method = "GET" },
                            new ApiEndpoint { Id = "performance", Title = "Performance Metrics", Description = "View performance data", Url = "/api/metrics", Icon = "bi-speedometer2", ButtonColor = "info", Method = "GET" },
                            new ApiEndpoint { Id = "logs", Title = "System Logs", Description = "View application logs", Url = "/api/logs", Icon = "bi-journal-text", ButtonColor = "secondary", Method = "GET" }
                        }
                    },
                    new DashboardSection
                    {
                        Id = "automation-tools",
                        Title = "IT Automation Tools",
                        Icon = "bi-robot",
                        Color = "warning",
                        Endpoints = new List<ApiEndpoint>
                        {
                            new ApiEndpoint { Id = "backup-systems", Title = "Backup Systems", Description = "Trigger system backups", Url = "/api/automation/backup", Icon = "bi-cloud-arrow-up", ButtonColor = "primary", Method = "POST" },
                            new ApiEndpoint { Id = "deploy-updates", Title = "Deploy Updates", Description = "Deploy system updates", Url = "/api/automation/deploy", Icon = "bi-arrow-repeat", ButtonColor = "warning", Method = "POST" },
                            new ApiEndpoint { Id = "restart-services", Title = "Restart Services", Description = "Restart system services", Url = "/api/automation/restart", Icon = "bi-bootstrap-reboot", ButtonColor = "danger", Method = "POST" }
                        }
                    },
                    new DashboardSection
                    {
                        Id = "external-integrations",
                        Title = "External Integrations",
                        Icon = "bi-diagram-3",
                        Color = "info",
                        Endpoints = new List<ApiEndpoint>
                        {
                            new ApiEndpoint { Id = "azure-status", Title = "Azure Services", Description = "Check Azure service status", Url = "https://status.azure.com/api/status", Icon = "bi-cloud", ButtonColor = "info", IsExternal = true, Method = "GET" },
                            new ApiEndpoint { Id = "github-repos", Title = "GitHub Repositories", Description = "Manage repositories", Url = "https://api.github.com/user/repos", Icon = "bi-github", ButtonColor = "dark", IsExternal = true, RequiresAuth = true, Method = "GET" },
                            new ApiEndpoint { Id = "slack-notifications", Title = "Slack Notifications", Description = "Send notifications", Url = "https://hooks.slack.com/services/YOUR/SLACK/WEBHOOK", Icon = "bi-slack", ButtonColor = "success", IsExternal = true, Method = "POST" }
                        }
                    }
                }
            };
    
            return View(model);
        }

    }
}
