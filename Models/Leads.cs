using System.Reflection;
using static Azure.Core.HttpHeader;

namespace CRM.Models
{
    public class Leads
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Leadname { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; } = string.Empty;
        public int SourceID { get; set; }
        public string Source{ get; set; }
        public string Status{ get; set; }
        public int StatusID { get; set; }
        public string? Notes { get; set; }
        public bool IsConverted { get; set; }

    }
}
