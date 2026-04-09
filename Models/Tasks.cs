namespace CRM.Models
{
    public class Tasks
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string LeadName { get; set; }
        public string Deal { get; set; }
        public int? DealID { get; set; } 
        public int ContactID { get; set; }
        public int AssignedTo { get; set; }
        public DateTime DueDate { get; set; }
        public int PriorityID { get; set; }
        public string Priority{ get; set; }
        public int StatusID { get; set; }
       public string Status { get; set; }
       public string UserName { get; set; }
    }
}
