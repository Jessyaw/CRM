using CRM.Models;
using CRM.Services;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CRMController : ControllerBase
    {
        private readonly ICRMServices _crmServices;
        public CRMController(ICRMServices crmServices)
        {
            _crmServices = crmServices;
        }

        [HttpPost("LoginUser")]
        public JsonResponse LoginUser([FromBody] Login login)
        {
            return _crmServices.LoginUser(login);
        }

        [HttpPost("CreateUser")]
        public Task<JsonResponse> CreateUser([FromBody] Login login)
        {
            return _crmServices.CreateUser(login);
        }
        [HttpPost("sendMailToLoginUser")]
        public Task<JsonResponse> sendMailToLoginUser([FromBody] Login login)
        {
            return _crmServices.sendMailToLoginUser(login);
        }
        [HttpPost("CheckEmailVerified")]
        public JsonResponse CheckEmailVerified([FromBody] Login login)
        {
            return _crmServices.CheckEmailVerified(login);
        }

        [HttpPost("VerifyToken")]
        public JsonResponse VerifyToken([FromBody] Login login)
        {
            return _crmServices.VerifyToken(login);
        }
        [HttpPost("ActiveDeactiveUser")]
       
        public JsonResponse ActiveDeactiveUser([FromBody] Login login)
        {
            return _crmServices.ActiveDeactiveUser(login);
        }

        [HttpGet("FetchLeadUser")]
        public JsonResponse FetchLeadUser()
        {
            return _crmServices.FetchLeadUser();
        }
        [HttpGet("FetchRoles")]
        public JsonResponse FetchRoles()
        {
            return _crmServices.FetchRoles();
        }
        [HttpGet("FetchTeams")]
        public JsonResponse FetchTeams()
        {
            return _crmServices.FetchTeams();
        }
        [HttpPost("FilterDealsOrTasks")]
        public JsonResponse FilterDealsOrTasks(FilterDealsOrTasks filterDealsOrTasks)
        {
            return _crmServices.FilterDealsOrTasks(filterDealsOrTasks);
        }
        [HttpPost("FilterDeals")]
        public JsonResponse FilterDeals(Deals deals)
        {
            return _crmServices.FilterDeals(deals);
        }
        [HttpGet("FetchLead")]
        public JsonResponse FetchLead()
        {
            return _crmServices.FetchLead();
        }
        [HttpGet("FetchDealsStat")]
        public JsonResponse FetchDealsStat()
        {
            return _crmServices.FetchDealsStat();
        }
        [HttpGet("FetchDeals")]
        public JsonResponse FetchDeals()
        {
            return _crmServices.FetchDeals();
        }
        [HttpGet("FetchContacts")]
        public JsonResponse FetchContacts()
        {
            return _crmServices.FetchContacts();
        }
        [HttpGet("FetchLeadSource")]
        public JsonResponse FetchLeadSource()
        {
            return _crmServices.FetchLeadSource();
        }
        [HttpGet("FetchLeadStatus")]
        public JsonResponse FetchLeadStatus()
        {
            return _crmServices.FetchLeadStatus();
        }
        [HttpGet("FetchDealsStages")]
        public JsonResponse FetchDealsStages()
        {
            return _crmServices.FetchDealsStages();
        }
        [HttpGet("FetchTasksPriority")]
        public JsonResponse FetchTasksPriority()
        {
            return _crmServices.FetchTasksPriority();
        }
        [HttpGet("FetchTasks")]
        public JsonResponse FetchTasks()
        {
            return _crmServices.FetchTasks();
        }
        [HttpGet("FetchCurrMonthTasks")]
        public JsonResponse FetchCurrMonthTasks()
        {
            return _crmServices.FetchCurrMonthTasks();
        }
        [HttpGet("FetchCRMStatData")]
        public JsonResponse FetchCRMStatData()
        {
            return _crmServices.FetchCRMStatData();
        }
        [HttpGet("FetchRecentTasks")]
        public JsonResponse FetchRecentTasks()
        {
            return _crmServices.FetchRecentTasks();
        }
        [HttpGet("FetchRecentDeals")]
        public JsonResponse FetchRecentDeals()
        {
            return _crmServices.FetchRecentDeals();
        }
        [HttpGet("FetchTasksStatus")]
        public JsonResponse FetchTasksStatus()
        {
            return _crmServices.FetchTasksStatus();
        }
        [HttpPost("AddUpdateLead")]
        public JsonResponse AddUpdateLead([FromBody] Leads lead)
        {
            return _crmServices.AddUpdateLead(lead);
        }
        [HttpPost("AddUpdateLeadSources")]
        public JsonResponse AddUpdateLeadSources([FromBody] LeadSources lead)
        {
            return _crmServices.AddUpdateLeadSources(lead);
        }

        [HttpPost("AddUpdateContact")]
        public JsonResponse AddUpdateContact([FromBody] Contacts contacts)
        {
            return _crmServices.AddUpdateContact(contacts);
        }
        [HttpPost("AddUpdateDeals")]
        public JsonResponse AddUpdateDeals([FromBody] Deals deals)
        {
            return _crmServices.AddUpdateDeals(deals);
        }
        [HttpPost("AddUpdateDealStages")]
        public JsonResponse AddUpdateDealStages([FromBody] DealStages deals)
        {
            return _crmServices.AddUpdateDealStages(deals);
        }
        [HttpPost("AddUpdateTasks")]
        public JsonResponse AddUpdateTasks([FromBody] Tasks tasks)
        {
            return _crmServices.AddUpdateTasks(tasks);
        }
        [HttpPost("DeleteLead")]
        public JsonResponse DeleteLead([FromBody] Leads lead)
        {
            return _crmServices.DeleteLead(lead);
        }
        [HttpPost("DeleteLeadSources")]
        public JsonResponse DeleteLeadSources([FromBody] LeadSources lead)
        {
            return _crmServices.DeleteLeadSources(lead);
        }

        [HttpPost("DeleteContact")]
        public JsonResponse DeleteContact([FromBody] Contacts contacts)
        {
            return _crmServices.DeleteContact(contacts);
        }
        [HttpPost("DeleteDeal")]
        public JsonResponse DeleteDeal([FromBody] Deals deals)
        {
            return _crmServices.DeleteDeal(deals);
        }
        [HttpPost("DeleteDealStages")]
        public JsonResponse DeleteDealStages([FromBody] DealStages deals)
        {
            return _crmServices.DeleteDealStages(deals);
        }
        [HttpPost("DeleteTask")]
        public JsonResponse DeleteTask([FromBody] Tasks tasks)
        {
            return _crmServices.DeleteTask(tasks);
        }

    }
}
