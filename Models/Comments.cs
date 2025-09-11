namespace CRM.Models
{
    public class Comments
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public byte[] Photo { get; set; }
        public string Time { get; set; }
    }
}
