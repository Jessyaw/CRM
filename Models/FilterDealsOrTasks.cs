namespace CRM.Models
{
    public class FilterDealsOrTasks
    {
        public int? ID { get; set; }
        public bool IsDeals { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int? StageID { get; set; }
        public int? StatusID { get; set; }
        public string? Title { get; set; }
        public string? LeadName { get; set; }
        public string? FullName { get; set; }
        public decimal? Amount { get; set; }
        public string? Stage { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? TaskStatus { get; set; }
        public string? TaskPriority { get; set; }
        public int? TotalDeals { get; set; }
        public int? TotalTasks { get; set; }
        public int? TotalRevenue { get; set; }
        public int? Completed { get; set; }
        public int? Pending { get; set; }
        public int? OverDue { get; set; }
        public int? TotalWon { get; set; }
        public int? TotalLost { get; set; }
        public int? OpenDeals { get; set; }

    }
}
