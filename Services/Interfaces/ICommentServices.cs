using CRM.Models;
using System.Xml.Linq;
using CRM.Models;

namespace CRM.Services.Interfaces
{
    public interface ICommentServices
    {
        public JsonResponse AddComment(Comments comments);
        public JsonResponse GetComments();
    }
}
