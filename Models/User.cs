namespace CRM.Models
{
    public class User
    {
        public int ID { get; set; }
        public string MemberName { get; set; }
        public string EmailID { get; set; }

        public int BooksCount { get; set; }
    }
}
