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

        [HttpPost("FetchUserData")]
        public JsonResponse FetchUserData([FromBody] Login login)
        {
            return _crmServices.FetchUserData(login);
        }

        [HttpPost("FetchLeadUser")]
        public JsonResponse FetchLeadUser([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchLeadUser(crmFilters);
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
        public JsonResponse FilterDealsOrTasks(FilterRequest request)
        {
            return _crmServices.FilterDealsOrTasks(request);
        }

        [HttpPost("FilterDeals")]
        public JsonResponse FilterDeals(Deals deals)
        {
            return _crmServices.FilterDeals(deals);
        }

        [HttpPost("FetchLead")]
        public JsonResponse FetchLead([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchLead(crmFilters);
        }

        [HttpGet("FetchDealsStat")]
        public JsonResponse FetchDealsStat()
        {
            return _crmServices.FetchDealsStat();
        }

        [HttpPost("FetchDeals")]
        public JsonResponse FetchDeals([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchDeals(crmFilters);
        }

        [HttpPost("FetchContacts")]
        public JsonResponse FetchContacts([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchContacts(crmFilters);
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

        [HttpPost("FetchTasks")]
        public JsonResponse FetchTasks([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchTasks(crmFilters);
        }

        [HttpPost("FetchCurrMonthTasks")]
        public JsonResponse FetchCurrMonthTasks([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchCurrMonthTasks(crmFilters);
        }

        [HttpPost("FetchCRMStatData")]
        public JsonResponse FetchCRMStatData([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchCRMStatData(crmFilters);
        }

        [HttpPost("FetchRecentTasks")]
        public JsonResponse FetchRecentTasks([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchRecentTasks(crmFilters);
        }

        [HttpPost("FetchRecentDeals")]
        public JsonResponse FetchRecentDeals([FromBody] CRMFilters crmFilters)
        {
            return _crmServices.FetchRecentDeals(crmFilters);
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
