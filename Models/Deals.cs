namespace CRM.Models
{
    public class Deals
    {
        public int ID { get;set; }
        public int? TeamID { get;set; }
        public string Title { get;set; }
        public string Contact { get;set; }
        public int ContactID { get;set; }
        public decimal Amount { get;set; }
        public int StageID { get;set; }
        public string Stage{ get;set; }
        public DateTime CloseDate { get;set; }
    }
}
