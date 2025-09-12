using CRM.Models;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommentController : Controller
    {
        private readonly ICommentServices _commentServices;
        public CommentController(ICommentServices commentServices) {
            _commentServices = commentServices;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("AddComment")]
        public JsonResponse AddComment([FromBody]Comments comments)
        {
            return _commentServices.AddComment(comments);
        }


        [HttpGet("GetComment")]
        public JsonResponse GetComments()
        {
            return _commentServices.GetComments();
        }
    }
}
