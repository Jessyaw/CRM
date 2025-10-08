using Microsoft.VisualBasic;
using System.Net;

namespace CRM.Models
{
    public class BorrowOrReturn
    {

        public int ID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public int BookQuantity { get; set; }
        public string BorrowedDate { get; set; }
        public string DueDate { get; set; }
        public string ReturnDate { get; set; }
        public int BookStatusID { get; set; }

    }
}
