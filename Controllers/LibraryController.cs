using CRM.Models;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    public class LibraryController : ControllerBase
    {

        private readonly ILibraryServices _libraryServices;
        public LibraryController(ILibraryServices libraryServices)
        {
            _libraryServices=libraryServices;
        }

        [HttpPost("AddBook")]
        public JsonResponse AddBook([FromBody] Library library)
        {
            return _libraryServices.AddBook(library);
        }
        [HttpPost("AddMember")]
        public JsonResponse AddMember([FromBody] User user)
        {
            return _libraryServices.AddMember(user);
        }

        [HttpPost("AddUpdateBorrowReturnDetails")]
        public JsonResponse AddUpdateBorrowReturnDetails([FromBody] BorrowOrReturn borrowOrReturn)
        {
            return _libraryServices.AddUpdateBorrowReturnDetails(borrowOrReturn);
        }

        [HttpGet("GetStatData")]
        public JsonResponse GetStatData()
        {
            return _libraryServices.GetStatData();
        }

        [HttpGet("GetRecentlyBorrowedData")]
        public JsonResponse GetRecentlyBorrowedData()
        {
            return _libraryServices.GetRecentlyBorrowedData();
        }

        [HttpGet("GetRecentlyReturnData")]
        public JsonResponse GetRecentlyReturnData()
        {
            return _libraryServices.GetRecentlyReturnData();
        }

        [HttpGet("GetOverdueData")]
        public JsonResponse GetOverdueData()
        {
            return _libraryServices.GetOverdueData();
        }

        [HttpGet("GetBookData")]
        public JsonResponse GetBookData()
        {
            return _libraryServices.GetBookData();
        }


        [HttpGet("GetMemberData")]
        public JsonResponse GetMemberData()
        {
            return _libraryServices.GetMemberData();
        }

        [HttpGet("GetBorrowedData")]
        public JsonResponse GetBorrowedData()
        {
            return _libraryServices.GetBorrowedData();
        }

        [HttpGet("GetReturnData")]
        public JsonResponse GetReturnData()
        {
            return _libraryServices.GetReturnData();
        }


    }
}
