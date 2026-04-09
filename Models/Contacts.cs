namespace CRM.Models
{
    public class Contacts
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Leadname { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public int SourceID { get; set; }
        public string? Notes { get; set; }
        public string Source { get; set; }
        public int? DealCount { get; set; }
    }
}
