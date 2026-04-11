using CRM.Models;

namespace CRM.Services.Interfaces
{
    public interface ICRMServices
    {
        public JsonResponse LoginUser(Login login);
        public Task<JsonResponse> CreateUser(Login login);
        public Task<JsonResponse> sendMailToLoginUser(Login login);
        public JsonResponse VerifyToken(Login login);
        public JsonResponse CheckEmailVerified(Login login);
        public JsonResponse ActiveDeactiveUser(Login login);
        public JsonResponse FetchLeadUser();
        public JsonResponse FilterDealsOrTasks(FilterDealsOrTasks filterDealsOrTasks);
        public JsonResponse FilterDeals(Deals deals);
        public JsonResponse FetchLead();
        public JsonResponse FetchRoles();
        public JsonResponse FetchTeams();
        public JsonResponse FetchDeals();
        public JsonResponse FetchDealsStat();
        public JsonResponse FetchRecentDeals();
        public JsonResponse FetchRecentTasks();
        public JsonResponse FetchCRMStatData();
        public JsonResponse FetchCurrMonthTasks();
        public JsonResponse FetchTasks();
        public JsonResponse FetchContacts();
        public JsonResponse FetchDealsStages();
        public JsonResponse FetchTasksPriority();
        public JsonResponse FetchTasksStatus();
        public JsonResponse FetchLeadSource();
        public JsonResponse FetchLeadStatus();
        public JsonResponse AddUpdateLeadSources(LeadSources lead);
        public JsonResponse AddUpdateLead(Leads lead);
        public JsonResponse AddUpdateContact(Contacts conatcts);
        public JsonResponse AddUpdateDeals(Deals deals);
        public JsonResponse AddUpdateDealStages(DealStages deals);
        public JsonResponse AddUpdateTasks(Tasks tasks);
        public JsonResponse DeleteLeadSources(LeadSources lead);
        public JsonResponse DeleteLead(Leads lead);
        public JsonResponse DeleteContact(Contacts conatcts);
        public JsonResponse DeleteDeal(Deals deals);
        public JsonResponse DeleteDealStages(DealStages deals);
        public JsonResponse DeleteTask(Tasks tasks);
    }
}
