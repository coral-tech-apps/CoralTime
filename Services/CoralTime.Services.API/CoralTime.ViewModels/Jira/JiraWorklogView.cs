namespace CoralTime.ViewModels.Jira
{
    public class JiraWorklogView
    {
        public string WorklogId { get; set; }

        public string Description { get; set; }

        public int TimeActual { get; set; }

        public string Date { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public string Key { get; set; }

        public JiraWorklogType Type { get; set; }

        public int TaskId { get; set; }

        public int? JiraProjectId { get; set; }

        public string OldDate { get; set; }

        public int? OldTimeActual { get; set; }

        public string OldDescription { get; set; }
    }

    public enum JiraWorklogType 
    {
        New,
        Edited,
        Deleted
    }
}
