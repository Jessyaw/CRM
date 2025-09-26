using CRM.Models;

namespace CRM.Services.Interfaces
{
    public interface ILibraryServices
    {
        public JsonResponse AddBook(Library library);
        public JsonResponse AddMember(User user);
        public JsonResponse AddUpdateBorrowReturnDetails(BorrowOrReturn borrowOrReturn);
        public JsonResponse GetStatData();
        public JsonResponse GetRecentlyBorrowedData();
        public JsonResponse GetRecentlyReturnData();
        public JsonResponse GetOverdueData();
        public JsonResponse GetBookData();
        public JsonResponse GetMemberData();
        public JsonResponse GetBorrowedData();
        public JsonResponse GetReturnData();
    
    }
}
