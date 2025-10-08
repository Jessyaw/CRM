namespace CRM.Models
{
    public class Library
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int CategoryID { get; set; }
        public int CopiesAvailable { get; set; }
        public bool isAvailable { get; set; }


      


    }
}
