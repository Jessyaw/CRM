using CRM.Models;
using CRM.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Data.SqlClient;
using MimeKit;
using System.Collections.Generic;
using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;

namespace CRM.Services
{
    public class CRMServices : ICRMServices
    {
        private readonly string _connectionString;
        private readonly string _apiKey;

        public CRMServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _apiKey = configuration["SendGrid:ApiKey"];
        }

        public String GenerateSecureToken()
        {
            var randomByte = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomByte);
            }
            return Convert.ToBase64String(randomByte)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

        }

        public async Task SendVerificationEmail(string toEmail, string token)
        {
            try
            {
                var apikey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? _apiKey;
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("SendGrid API Key is missing!");
                }
                var client = new SendGrid.SendGridClient(apikey);
                var from = new SendGrid.Helpers.Mail.EmailAddress("jcorecrm@gmail.com", "JCore CRM");
                var to = new SendGrid.Helpers.Mail.EmailAddress(toEmail);

               // var message = new MimeMessage();
               // message.From.Add(new MailboxAddress("Mini core CRM", "minicorecrm@gmail.com"));
               // message.To.Add(new MailboxAddress("", toEmail));
               // message.Subject = "Verify Your Email";

                string verifyLink = $"http://jessyaw.github.io/Portfolio/#/verify?token={token}";
                var htmlContent = $@"
                        <div style='font-family: Arial, sans-serif; line-height: 1.6;'>
                            <h2 style='color: #2e7d32;'>Mini CRM</h2>
                        
                            <p>Hello,</p>
                        
                            <p>Thank you for signing up. Please confirm your email address:</p>
                        
                            <p style='margin: 20px 0;'>
                                <a href='{verifyLink}' 
                                   style='background-color: #2e7d32; color: white; padding: 10px 20px; 
                                   text-decoration: none; border-radius: 5px; display: inline-block;'>
                                   Verify Email
                                </a>
                            </p>
                        
                            <p><a href='{verifyLink}'>{verifyLink}</a></p>
                        
                            <p>Regards,<br/>Mini CRM Team</p>
                        </div>";
                var message = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(
                    from,
                    to,
                    "Verify Your Email",
                    "Please, Verify Your Email",
                    htmlContent
                   );
                var response = await client.SendEmailAsync(message);
                Console.WriteLine("SendGrid ERROR: " + ex.Message);
                //using (var client = new MailKit.Net.Smtp.SmtpClient())
                //{
                //    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                //    client.Authenticate("minicorecrm@gmail.com", "vpzk bqgh gltq jthu");
                //    client.Send(message);
                //    client.Disconnect(true);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("send drid ERROR : " + ex.Message);
            }
        }

        public async Task<JsonResponse> CreateUser(Login login)
        {
            login.EmailVerificationToken = GenerateSecureToken();
            DateTime expiry = DateTime.UtcNow.AddHours(24);
            JsonResponse json = new JsonResponse();
            string proc = "SP_CreateUser";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", login.ID);
                sqlCommand.Parameters.AddWithValue("@FullName", login.FullName);
                sqlCommand.Parameters.AddWithValue("@UserPassword", login.Password);
                sqlCommand.Parameters.AddWithValue("@ConfirmPassword", login.ConfirmPassword);
                sqlCommand.Parameters.AddWithValue("@RoleID", login.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", login.TeamID);
                sqlCommand.Parameters.AddWithValue("@Email", login.Email);
                sqlCommand.Parameters.AddWithValue("@IsEmailVerified", 0);
                sqlCommand.Parameters.AddWithValue("@EmailVerificationToken", login.EmailVerificationToken);
                sqlCommand.Parameters.AddWithValue("@EmailVerificationTokenExpiry", expiry);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();


                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
                if (json.Status == "S")
                {
                    await SendVerificationEmail(login.Email, login.EmailVerificationToken);
                }
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public async Task<JsonResponse> sendMailToLoginUser(Login login)
        {
            login.EmailVerificationToken = GenerateSecureToken();
            JsonResponse json = new JsonResponse();

            try
            {
                DateTime expiry = DateTime.UtcNow.AddHours(24);

                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("SP_UpdateVerificationToken", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.AddWithValue("@Email", login.Email);
                        sqlCommand.Parameters.AddWithValue("@EmailVerificationToken", login.EmailVerificationToken);
                        sqlCommand.Parameters.AddWithValue("@EmailVerificationTokenExpiry", expiry);

                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                }

                await SendVerificationEmail(login.Email, login.EmailVerificationToken);

                json.Status = "S";
                json.Message = "Verification email sent!";
            }
            catch (Exception)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse VerifyToken(Login login)
        {

            JsonResponse json = new JsonResponse();
            string proc = "SP_VerifyToken";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@EmailVerificationToken", login.EmailVerificationToken);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();


                if (dt.Rows.Count > 0)
                {
                    json.Status = dt.Rows[0]["Status"].ToString();
                    json.Message = dt.Rows[0]["Message"].ToString();
                }
                else
                {
                    json.Status = "F";
                    json.Message = "Invalid token!";
                }
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse CheckEmailVerified(Login login)
        {

            JsonResponse json = new JsonResponse();
            string proc = "SP_CheckEmailVerified";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Email", login.Email);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }

        public JsonResponse ActiveDeactiveUser(Login login)
        {

            JsonResponse json = new JsonResponse();
            string proc = "SP_ActiveDeactiveUser";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", login.ID);
                sqlCommand.Parameters.AddWithValue("@IsActive", login.IsActive);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse FetchUserData(Login login)
        {

            JsonResponse json = new JsonResponse();
            string proc = "SP_FetchUserData";
            List<Login> loginList = new List<Login>();
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", login.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                login.ID = Convert.ToInt32(dt.Rows[0]["UserID"]);
                login.FullName = dt.Rows[0]["Name"].ToString();
                login.Email = dt.Rows[0]["Email"].ToString();
                login.RoleID = Convert.ToInt32(dt.Rows[0]["RoleID"]);
                login.TeamID = dt.Rows[0]["TeamID"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["TeamID"]) : 0;



                json.Status = "S";
                json.Message = "Login Success";
                json.Data = login;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse LoginUser(Login login)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_LoginUser";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Email", login.Email);
                sqlCommand.Parameters.AddWithValue("@UserPassword", login.Password);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                // DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                sqlConnection.Open();
                adapter.Fill(ds);
                sqlConnection.Close();

                json.Status = ds.Tables[0].Rows[0]["Status"].ToString();
                json.Message = ds.Tables[0].Rows[0]["Message"].ToString();
                if (ds.Tables.Count > 1)
                {
                    login.ID = Convert.ToInt32(ds.Tables[1].Rows[0]["ID"]);
                    login.Email = ds.Tables[1].Rows[0]["Email"].ToString();
                    login.FullName = ds.Tables[1].Rows[0]["Name"].ToString();
                    login.RoleID = Convert.ToInt32(ds.Tables[1].Rows[0]["RoleID"]);
                }
                json.Data = login;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }

        public JsonResponse FilterDealsOrTasks(FilterRequest request)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<FilterDealsOrTasks> filterDealsOrTasksList = new List<FilterDealsOrTasks>();
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                string proc = "SP_FilterDealsOrTasks";
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@IsDeals", request.filterDealsOrTasks.IsDeals);
                sqlCommand.Parameters.AddWithValue("@StartDate", request.filterDealsOrTasks.StartDate);
                sqlCommand.Parameters.AddWithValue("@EndDate", request.filterDealsOrTasks.EndDate);
                sqlCommand.Parameters.AddWithValue("@StageID", request.filterDealsOrTasks.StageID);
                sqlCommand.Parameters.AddWithValue("@StatusID", request.filterDealsOrTasks.StatusID);
                sqlCommand.Parameters.AddWithValue("@RoleID", request.crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", request.crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("@UserID", request.crmFilters.UserID);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataSet ds = new DataSet();
                sqlConnection.Open();
                adapter.Fill(ds);
                sqlConnection.Close();
                if (request.filterDealsOrTasks.IsDeals)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var deal = new FilterDealsOrTasks
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            Title = row["Title"].ToString(),
                            LeadName = row["LeadName"].ToString(),
                            Amount = Convert.ToDecimal(row["Amount"]),
                            Stage = row["Stage"].ToString(),
                            CloseDate = Convert.ToDateTime(row["CloseDate"])
                        };
                        filterDealsOrTasksList.Add(deal);
                    }

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        var summary = ds.Tables[1].Rows[0];
                        foreach (var deal in filterDealsOrTasksList)
                        {
                            deal.TotalDeals = Convert.ToInt32(summary["Total Deals"]);
                            deal.TotalWon = Convert.ToInt32(summary["Total Won"]);
                            deal.TotalLost = Convert.ToInt32(summary["Total Lost"]);
                            deal.TotalRevenue = Convert.ToInt32(summary["Total Revenue"]);
                        }
                    }
                }
                else
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var task = new FilterDealsOrTasks
                        {
                            ID = Convert.ToInt32(row["ID"]),
                            Title = row["Title"].ToString(),
                            FullName = row["FullName"].ToString(),
                            DueDate = Convert.ToDateTime(row["DueDate"]),
                            TaskPriority = row["TasksPriority"].ToString(),
                            TaskStatus = row["TasksStatus"].ToString()
                        };
                        filterDealsOrTasksList.Add(task);
                    }

                    if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                    {
                        var summary = ds.Tables[1].Rows[0];
                        foreach (var task in filterDealsOrTasksList)
                        {
                            task.TotalTasks = Convert.ToInt32(summary["Total Tasks"]);
                            task.Completed = Convert.ToInt32(summary["Completed"]);
                            task.Pending = Convert.ToInt32(summary["Pending"]);
                            task.OverDue = Convert.ToInt32(summary["Overdue"]);
                        }
                    }
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = filterDealsOrTasksList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FetchDealsStat()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                FilterDealsOrTasks filterDealsOrTasks = new FilterDealsOrTasks();
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                string proc = "SP_FetchDealsStat";
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                filterDealsOrTasks.TotalDeals = Convert.ToInt32(dt.Rows[0]["Total deals"]);
                filterDealsOrTasks.TotalWon = Convert.ToInt32(dt.Rows[0]["Total Won"]);
                filterDealsOrTasks.TotalLost = Convert.ToInt32(dt.Rows[0]["Total Lost"]);
                filterDealsOrTasks.OpenDeals = Convert.ToInt32(dt.Rows[0]["Open Deals"]);
                filterDealsOrTasks.TotalRevenue = Convert.ToInt32(dt.Rows[0]["Total Revenue"]);

                json.Message = "Success";
                json.Status = "S";
                json.Data = filterDealsOrTasks;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FilterDeals(Deals deals)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Deals> dealsList = new List<Deals>();
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                string proc = "SP_FilterDeals";
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@StageID", deals.StageID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    deals.ID = Convert.ToInt32(dr["ID"]);
                    deals.StageID = Convert.ToInt32(dr["StageID"]);
                    deals.Title = dr["Title"].ToString();
                    deals.Contact = dr["LeadName"].ToString();
                    deals.Amount = Convert.ToDecimal(dr["Amount"]);
                    deals.ContactID = Convert.ToInt32(dr["ContactID"]);
                    deals.Stage = dr["Stage"].ToString();
                    deals.CloseDate = Convert.ToDateTime(dr["CloseDate"]);
                    dealsList.Add(deals);

                }

                json.Message = "Success";
                json.Status = "S";
                json.Data = dealsList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FetchLeadSource()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Leads> sourceList = new List<Leads>();
                string proc = "SP_FetchLeadSource";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Leads leads = new Leads();
                    leads.SourceID = Convert.ToInt32(dr["ID"]);
                    leads.Source = dr["Title"].ToString();
                    sourceList.Add(leads);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = sourceList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FetchRoles()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Roles> rolesList = new List<Roles>();
                string proc = "SP_FetchRoles";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Roles roles = new Roles();
                    roles.ID = Convert.ToInt32(dr["ID"]);
                    roles.Title = dr["Title"].ToString();
                    rolesList.Add(roles);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = rolesList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FetchTeams()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Teams> teamsList = new List<Teams>();
                string proc = "SP_FetchTeams";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Teams teams = new Teams();
                    teams.ID = Convert.ToInt32(dr["ID"]);
                    teams.Team = dr["Team"].ToString();
                    teamsList.Add(teams);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = teamsList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }
        public JsonResponse FetchLeadStatus()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Leads> statusList = new List<Leads>();
                string proc = "SP_FetchLeadStatus";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Leads leads = new Leads();
                    leads.StatusID = Convert.ToInt32(dr["ID"]);
                    leads.Status = dr["Title"].ToString();
                    statusList.Add(leads);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = statusList;
            }
            catch (Exception e)
            {
                json.Message = "Something went wrong";
                json.Status = "F";
            }

            return json;
        }

        public JsonResponse FetchLeadUser(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Login> loginList = new List<Login>();
                string proc = "SP_FetchLeadUser";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Login login = new Login();
                    login.ID = Convert.ToInt32(dr["ID"]);
                    login.RoleID = Convert.ToInt32(dr["RoleID"]);
                    login.TeamID = dr["TeamID"] != DBNull.Value ? Convert.ToInt32(dr["TeamID"]) : 0;
                    login.FullName = dr["Name"].ToString();
                    login.Email = dr["Email"].ToString();
                    login.Password = dr["Password"].ToString();
                    login.ConfirmPassword = dr["ConfirmPassword"].ToString();
                    login.Role = dr["Role"].ToString();
                    login.Team = dr["Team"].ToString();
                    login.IsActive = Convert.ToBoolean(dr["Active"]);
                    loginList.Add(login);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = loginList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchLead(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Leads> leadsList = new List<Leads>();
                string proc = "SP_FetchLeads";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Leads leads = new Leads();
                    leads.ID = Convert.ToInt32(dr["ID"]);
                    leads.Leadname = dr["Name"].ToString();
                    leads.Email = dr["Email"].ToString();
                    leads.Mobile = dr["Mobile"].ToString();
                    leads.StatusID = Convert.ToInt32(dr["StatusID"]);
                    leads.Status = dr["Status"].ToString();
                    leads.SourceID = Convert.ToInt32(dr["SourceID"]);
                    leads.IsConverted = Convert.ToBoolean(dr["IsConverted"]);
                    leads.Source = dr["Source"].ToString();
                    leads.Notes = dr["Notes"].ToString();
                    leadsList.Add(leads);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = leadsList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchCRMStatData(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<CRMFields> crmList = new List<CRMFields>();
                string proc = "SP_FetchCRMStatData";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("@UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    CRMFields crm = new CRMFields();
                    crm.Leads = dr["Total Leads"].ToString();
                    crm.Tasks = dr["Total Tasks"].ToString();
                    crm.Deals = dr["Total Deals"].ToString();
                    crm.Contacts = dr["Total Contacts"].ToString();
                    crm.Revenue = dr["Total Revenue"].ToString();
                    crmList.Add(crm);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = crmList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchContacts(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Contacts> contactsList = new List<Contacts>();
                string proc = "SP_FetchContacts";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Contacts contacts = new Contacts();
                    contacts.ID = Convert.ToInt32(dr["ID"]);
                    contacts.Leadname = dr["Name"].ToString();
                    contacts.Email = dr["Email"].ToString();
                    contacts.Mobile = dr["Mobile"].ToString();
                    contacts.SourceID = Convert.ToInt32(dr["SourceID"]);
                    contacts.Source = dr["Source"].ToString();
                    contacts.Notes = dr["Notes"].ToString();
                    contacts.DealCount = Convert.ToInt32(dr["DealCount"]);
                    contactsList.Add(contacts);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = contactsList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchDeals(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Deals> dealsList = new List<Deals>();
                string proc = "SP_FetchDeals";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Deals deals = new Deals();
                    deals.ID = Convert.ToInt32(dr["ID"]);
                    deals.StageID = Convert.ToInt32(dr["StageID"]);
                    deals.Title = dr["Title"].ToString();
                    deals.Contact = dr["LeadName"].ToString();
                    deals.Amount = Convert.ToDecimal(dr["Amount"]);
                    deals.ContactID = Convert.ToInt32(dr["ContactID"]);
                    deals.Stage = dr["Stage"].ToString();
                    deals.CloseDate = Convert.ToDateTime(dr["CloseDate"]);
                    dealsList.Add(deals);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = dealsList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchRecentDeals(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Deals> dealsList = new List<Deals>();
                string proc = "SP_FetchRecentDeals";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("@UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Deals deals = new Deals();
                    deals.ID = Convert.ToInt32(dr["ID"]);
                    //   deals.StageID = Convert.ToInt32(dr["StageID"]);
                    deals.Title = dr["Title"].ToString();
                    deals.Contact = dr["LeadName"].ToString();
                    deals.Amount = Convert.ToDecimal(dr["Amount"]);
                    //  deals.ContactID = Convert.ToInt32(dr["ContactID"]);
                    deals.Stage = dr["Stage"].ToString();
                    // deals.CloseDate = Convert.ToDateTime(dr["CloseDate"]);
                    dealsList.Add(deals);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = dealsList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchTasks(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                string proc = "SP_FetchTasks";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Tasks tasks = new Tasks();
                    tasks.ID = Convert.ToInt32(dr["ID"]);
                    tasks.DealID = Convert.ToInt32(dr["DealID"]);
                    tasks.Title = dr["Title"].ToString();
                    tasks.Deal = dr["Deal"].ToString();
                    tasks.LeadName = dr["LeadName"].ToString();
                    tasks.ContactID = Convert.ToInt32(dr["ContactID"]);
                    tasks.DueDate = Convert.ToDateTime(dr["DueDate"]);
                    tasks.StatusID = Convert.ToInt32(dr["StatusID"]);
                    tasks.PriorityID = Convert.ToInt32(dr["PriorityID"]);
                    tasks.Status = dr["TasksStatus"].ToString();
                    tasks.Priority = dr["TasksPriority"].ToString();
                    tasks.UserName = dr["FullName"].ToString();
                    tasks.AssignedTo = Convert.ToInt32(dr["AssignedTo"]);
                    tasksList.Add(tasks);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = tasksList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchRecentTasks(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                string proc = "SP_FetchRecentTasks";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("@UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Tasks tasks = new Tasks();
                    tasks.ID = Convert.ToInt32(dr["ID"]);
                    //    tasks.DealID = Convert.ToInt32(dr["DealID"]);
                    tasks.Title = dr["Title"].ToString();
                    //  tasks.Deal = dr["Deal"].ToString();
                    tasks.LeadName = dr["LeadName"].ToString();
                    //     tasks.ContactID = Convert.ToInt32(dr["ContactID"]);
                    tasks.DueDate = Convert.ToDateTime(dr["DueDate"]);
                    //   tasks.StatusID = Convert.ToInt32(dr["StatusID"]);
                    //   tasks.PriorityID = Convert.ToInt32(dr["PriorityID"]);
                    tasks.Status = dr["TasksStatus"].ToString();
                    tasks.Priority = dr["TasksPriority"].ToString();
                    //   tasks.UserName = dr["FullName"].ToString();
                    //   tasks.AssignedTo = Convert.ToInt32(dr["AssignedTo"]);
                    tasksList.Add(tasks);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = tasksList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchCurrMonthTasks(CRMFilters crmFilters)
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                string proc = "SP_FetchCurrMonthTasks";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@RoleID", crmFilters.RoleID);
                sqlCommand.Parameters.AddWithValue("@TeamID", crmFilters.TeamID);
                sqlCommand.Parameters.AddWithValue("@UserID", crmFilters.UserID);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Tasks tasks = new Tasks();
                    tasks.ID = Convert.ToInt32(dr["ID"]);
                    //    tasks.DealID = Convert.ToInt32(dr["DealID"]);
                    tasks.Title = dr["Title"].ToString();
                    //  tasks.Deal = dr["Deal"].ToString();
                    //  tasks.LeadName = dr["LeadName"].ToString();
                    //     tasks.ContactID = Convert.ToInt32(dr["ContactID"]);
                    tasks.DueDate = Convert.ToDateTime(dr["DueDate"]);
                    //   tasks.StatusID = Convert.ToInt32(dr["StatusID"]);
                    //   tasks.PriorityID = Convert.ToInt32(dr["PriorityID"]);
                    //  tasks.Status = dr["TasksStatus"].ToString();
                    // tasks.Priority = dr["TasksPriority"].ToString();
                    //   tasks.UserName = dr["FullName"].ToString();
                    //   tasks.AssignedTo = Convert.ToInt32(dr["AssignedTo"]);
                    tasksList.Add(tasks);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = tasksList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchDealsStages()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Deals> stagesList = new List<Deals>();
                string proc = "SP_FetchDealsStages";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Deals deals = new Deals();
                    deals.ID = Convert.ToInt32(dr["ID"]);
                    deals.Stage = dr["Stage"].ToString();
                    stagesList.Add(deals);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = stagesList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchTasksPriority()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                string proc = "SP_FetchTasksPriority";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Tasks tasks = new Tasks();
                    tasks.ID = Convert.ToInt32(dr["ID"]);
                    tasks.Priority = dr["Priority"].ToString();
                    tasksList.Add(tasks);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = tasksList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }
        public JsonResponse FetchTasksStatus()
        {
            JsonResponse json = new JsonResponse();
            try
            {
                List<Tasks> tasksList = new List<Tasks>();
                string proc = "SP_FetchTasksStatus";
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                sqlDataAdapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow dr in dt.Rows)
                {
                    Tasks tasks = new Tasks();
                    tasks.ID = Convert.ToInt32(dr["ID"]);
                    tasks.Status = dr["Status"].ToString();
                    tasksList.Add(tasks);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = tasksList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }
            return json;
        }

        public JsonResponse AddUpdateLeadSources(LeadSources lead)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddUpdateLeadSources";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", lead.ID);
                sqlCommand.Parameters.AddWithValue("@SourceName", lead.Source);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse AddUpdateDealStages(DealStages deal)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddUpdateDealStages";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", deal.ID);
                sqlCommand.Parameters.AddWithValue("@Stage", deal.Stage);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse AddUpdateLead(Leads lead)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddOrUpdateLead";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", lead.ID);
                sqlCommand.Parameters.AddWithValue("@UserID", lead.UserID);
                sqlCommand.Parameters.AddWithValue("@TeamID", lead.TeamID);
                sqlCommand.Parameters.AddWithValue("@LeadName", lead.Leadname);
                sqlCommand.Parameters.AddWithValue("@Email", lead.Email);
                sqlCommand.Parameters.AddWithValue("@Mobile", lead.Mobile);
                sqlCommand.Parameters.AddWithValue("@SourceID", lead.SourceID);
                sqlCommand.Parameters.AddWithValue("@StatusID", lead.StatusID);
                sqlCommand.Parameters.AddWithValue("@Notes", lead.Notes);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse AddUpdateContact(Contacts contacts)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddOrUpdateContact";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", contacts.ID);
                sqlCommand.Parameters.AddWithValue("@UserID", contacts.UserID);
                sqlCommand.Parameters.AddWithValue("@LeadName", contacts.Leadname);
                sqlCommand.Parameters.AddWithValue("@Email", contacts.Email);
                sqlCommand.Parameters.AddWithValue("@Mobile", contacts.Mobile);
                sqlCommand.Parameters.AddWithValue("@SourceID", contacts.SourceID);
                sqlCommand.Parameters.AddWithValue("@Notes", contacts.Notes);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse AddUpdateDeals(Deals deals)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddOrUpdateDeals";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", deals.ID);
                sqlCommand.Parameters.AddWithValue("@Title", deals.Title);
                sqlCommand.Parameters.AddWithValue("@TeamID", deals.TeamID);
                sqlCommand.Parameters.AddWithValue("@ContactID", deals.ContactID);
                sqlCommand.Parameters.AddWithValue("@Amount", deals.Amount);
                sqlCommand.Parameters.AddWithValue("@StageID", deals.StageID);
                sqlCommand.Parameters.AddWithValue("@CloseDate", deals.CloseDate);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse AddUpdateTasks(Tasks tasks)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddOrUpdateTasks";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", tasks.ID);
                sqlCommand.Parameters.AddWithValue("@Title", tasks.Title);
                sqlCommand.Parameters.AddWithValue("@DealID", tasks.DealID);
                sqlCommand.Parameters.AddWithValue("@ContactID", tasks.ContactID);
                sqlCommand.Parameters.AddWithValue("@AssignedTo", tasks.AssignedTo);
                sqlCommand.Parameters.AddWithValue("@DueDate", tasks.DueDate);
                sqlCommand.Parameters.AddWithValue("@PriorityID", tasks.PriorityID);
                sqlCommand.Parameters.AddWithValue("@StatusID", tasks.StatusID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteLead(Leads leads)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteLead";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", leads.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteDealStages(DealStages deals)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteDealStages";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", deals.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteLeadSources(LeadSources leads)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteLeadSources";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", leads.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteContact(Contacts contacts)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteContact";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", contacts.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteDeal(Deals deals)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteDeal";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", deals.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
        public JsonResponse DeleteTask(Tasks tasks)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_DeleteTask";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", tasks.ID);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();

                json.Status = dt.Rows[0]["Status"].ToString();
                json.Message = dt.Rows[0]["Message"].ToString();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong!!";
            }

            return json;
        }
    }
}
