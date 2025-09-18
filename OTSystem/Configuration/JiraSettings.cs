namespace OTSystem.Configuration
{
    public class JiraSettings
    {
        public string CloudUrl { get; set; }
        public string Email { get; set; }
        public string ApiToken { get; set; }
        public string ProjectKey { get; set; }
        public string IssueType { get; set; } = "Task";
    }
}