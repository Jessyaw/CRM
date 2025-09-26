namespace CRM.Models
{
    public class Book
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public int CopiesAvailable { get; set; }
        public bool IsAvailable { get; set; }
    }
}
