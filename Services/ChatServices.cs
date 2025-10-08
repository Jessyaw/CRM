using CRM.Models;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Services
{
    public class ChatServices : IChatServices
    {
        private readonly string _connectionString;
        private readonly string _geminiKey = "AIzaSyCjPe8shwaJvDxLNb95Hq84yf_iI5RCYtk";
        public ChatServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            //_geminiKey = configuration["GEMINI_KEY"];
        }

        public async Task<Chat> ListAvailableModels()
        {
            Chat chat = new Chat();
            var client = new HttpClient();

            // Add your API key to the header
            client.DefaultRequestHeaders.Add("x-goog-api-key", _geminiKey);

            // ✅ Use v1 (latest stable endpoint)
            string url = "https://generativelanguage.googleapis.com/v1/models";

            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            chat.Message = json;
            //Execute SQL on your SSMS DB


            return chat;

        }

        public async Task<Chat> Ask(Chat chat)
        {
            if (string.IsNullOrEmpty(chat.Question))
            {
                chat.Status = "F";
                chat.Message = "Empty query";
                return chat;
            }
            string prompt =
                "You are an intelligent SQL assistant for my Library Management database (SQL Server). " +
                "Your job is to analyze the user's question and decide the correct type of response:\n\n" +
                "1. If the question clearly asks for data that can be answered using the library database — " +
                "for example, details about members, books, borrowing history, categories, or book status — " +
                "generate a single valid and executable SQL SELECT query.\n\n" +
                "2️. If the question is vague or unclear but seems related to the library (e.g., 'Can you help me get library details?', 'Tell me something about the library'), " +
                "do NOT generate SQL. Instead, reply naturally in plain text asking for clarification, for example: " +
                "'Sure! Could you please specify what details you want — books, members, or borrow records?'\n\n" +
                "3️. If the question is casual or unrelated (like 'hi', 'thanks', 'how are you', 'who are you'), " +
                "reply naturally as a chatbot (e.g., 'Hi there!', Hi! I’m Bibliobot, your library assistant. How can I help you today?').\n\n" +
                "If you decide to generate SQL, follow these rules strictly:\n" +
                "- Return **only** the raw SQL query (no markdown, no ```sql fences, no explanations, no extra text).\n" +
                "only select isActive users or books or anything"+
                "- Only SELECT queries are allowed.\n" +             
                "- Never include these fields: ID, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn.\n" +
                "Whenever you use an aggregate function (like SUM, COUNT, AVG, MAX, MIN), " +
                "always use a meaningful column alias using AS — for example: " +
                "'SELECT SUM(BookQuantity) AS TotalBooksBorrowed ...' instead of leaving it unnamed.\n" +                "Use the following schema and columns exactly:\n" +
                "jessy.tbl_LibraryUsers(ID, Membername, EmailID, BooksCount, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn), " +
                "jessy.tbl_LibraryBooks(ID, Title, Author, CategoryID, CopiesAvailable, isAvailable, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn), " +
                "jessy.tbl_BorrowOrReturn(ID, UserID, BookID, CategoryID, BookQuantity, BorrowedDate, DueDate, ReturnDate, BookStatusID, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn), " +
                "jessy.tbl_BookStatus(ID, BookStatus, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn), " +
                "jessy.tbl_Category(ID, Category, IsActive, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn).\n" +
                "Foreign keys:\n" +
                "tbl_BorrowOrReturn.UserID -> tbl_LibraryUsers.ID,\n" +
                "tbl_BorrowOrReturn.BookID -> tbl_LibraryBooks.ID,\n" +
                "tbl_BorrowOrReturn.CategoryID -> tbl_Category.ID,\n" +
                "tbl_BorrowOrReturn.BookStatusID -> tbl_BookStatus.ID,\n" +
                "tbl_LibraryBooks.CategoryID -> tbl_Category.ID.\n\n" +
                "Question: " + chat.Question;




            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-goog-api-key", _geminiKey);
            var body = new
            {
                contents = new[]
           {
                new { parts = new[] { new { text = prompt } } }
            }
            };
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent", content);
            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var sqlQuery = data["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(sqlQuery))
            {
                chat.Status = "F";
                chat.Message = "Gemini failed to generate SQL";
                return chat;
            }


            if (!sqlQuery.TrimStart('\r', '\n', ' ', '\t').StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                chat.Status = "T";
                chat.Message = sqlQuery;
                return chat;
            }

            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand command = new SqlCommand(sqlQuery, connection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command);
            DataTable dt = new DataTable();
            connection.Open();
            sqlDataAdapter.Fill(dt);
            connection.Close();
            var rows = new List<Dictionary<string, object>>();

            if (dt.Rows.Count == 0)
            {
                chat.Status = "T";
                chat.Message = "No data available for the selected criteria!!";
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    var dictionary = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        dictionary[col.ColumnName] = dr[col];
                       
                    }
                    rows.Add(dictionary);
                }
                chat.Response = Newtonsoft.Json.JsonConvert.SerializeObject(rows);


                //var stringBuilder = new StringBuilder();
                //foreach (DataColumn col in dt.Columns)
                //{
                //    stringBuilder.Append(col.ColumnName + '\t');
                //}
                //stringBuilder.AppendLine();

                //foreach (DataRow row in dt.Rows)
                //{
                //    foreach (DataColumn col in dt.Columns)
                //    {
                //        stringBuilder.Append(row[col]?.ToString() + '\t');
                //    }
                //    stringBuilder.AppendLine();
                //}
                chat.Status = "S";
                chat.Message = "Fetching data successfully!!";



            }



            //Execute SQL on your SSMS DB


            return chat;
        }

    }
}
