using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WsRaisedHandsModern.Api.ViewModels
{
    public class DashboardViewModel
    {
        public List<ApiEndpoint> LocalEndpoints { get; set; } = new List<ApiEndpoint>();
        public List<ApiEndpoint> ExternalEndpoints { get; set; } = new List<ApiEndpoint>();
        public List<DashboardSection> Sections { get; set; } = new List<DashboardSection>();
        public SystemStatus SystemStatus { get; set; } = new SystemStatus();
    }

    public class ApiEndpoint
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public string Icon { get; set; } = "bi-gear";
        public string Category { get; set; } = "General";
        public bool RequiresAuth { get; set; } = false;
        public bool IsExternal { get; set; } = false;
        public string ButtonColor { get; set; } = "primary";
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class DashboardSection
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "primary";
        public List<ApiEndpoint> Endpoints { get; set; } = new List<ApiEndpoint>();
        public bool IsCollapsed { get; set; } = false;
    }

    public class SystemStatus
    {
        public string Status { get; set; } = "Online";
        public int ActiveConnections { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<ServiceStatus> Services { get; set; } = new List<ServiceStatus>();
    }

    public class ServiceStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public string Url { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
    }
}