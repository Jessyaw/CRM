namespace CRM.Models
{
    public class Borrow
    {
        public int ID { get; set; }
        public string MemberName { get; set; }
        public string Title { get; set; }
        public int BookQuantity { get; set; }
        public string BorrowedDate { get; set; }
        public string DueDate { get; set; }
        public string BookStatus { get; set; }
    }
}
