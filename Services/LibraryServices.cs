using CRM.Models;
using CRM.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CRM.Services
{
    public class LibraryServices : ILibraryServices
    {
        private readonly string _connectionString;
        public LibraryServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public JsonResponse AddBook(Library library)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddBook";
            try
            {
                List<Library> libraries = new List<Library>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", library.ID);
                sqlCommand.Parameters.AddWithValue("@Title", library.Title);
                sqlCommand.Parameters.AddWithValue("@Author", library.Author);
                sqlCommand.Parameters.AddWithValue("@CategoryID", library.CategoryID);
                sqlCommand.Parameters.AddWithValue("@CopiesAvailable", library.CopiesAvailable);
                sqlCommand.Parameters.AddWithValue("@isAvailable", library.isAvailable);
                connection.Open();
                DataTable dataTable = new DataTable();
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                sqlDataAdapter.Fill(dataTable);
                DataRow dataRow = dataTable.Rows[0];

                connection.Close();
                json.Status = dataRow["Status"].ToString();
                json.Message = dataRow["Message"].ToString();

            }
            catch (Exception ex)
            {
            }
            return json;
        }

        public JsonResponse AddMember(User user)
        {

            JsonResponse json = new JsonResponse();
            try
            {

                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_AddUpdateMember", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", user.ID);
                sqlCommand.Parameters.AddWithValue("@Membername", user.MemberName);
                sqlCommand.Parameters.AddWithValue("@EmailID", user.EmailID);
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                DataRow dr = dt.Rows[0];
                json.Status = dr["Status"].ToString();
                json.Message = dr["Message"].ToString();
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse AddUpdateBorrowReturnDetails(BorrowOrReturn borrowOrReturn)
        {

            JsonResponse json = new JsonResponse();
            try
            {

                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_AddUpdateBorrowReturnDetails", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@ID", borrowOrReturn.ID);
                sqlCommand.Parameters.AddWithValue("@UserID", borrowOrReturn.UserID);
                sqlCommand.Parameters.AddWithValue("@BookID", borrowOrReturn.BookID);
                sqlCommand.Parameters.AddWithValue("@BookQuantity", borrowOrReturn.BookQuantity);
                sqlCommand.Parameters.AddWithValue("@BorrowedDate", borrowOrReturn.BorrowedDate);
                sqlCommand.Parameters.AddWithValue("@DueDate", borrowOrReturn.DueDate);
                sqlCommand.Parameters.AddWithValue("@BookStatusID", borrowOrReturn.BookStatusID);
                sqlCommand.Parameters.AddWithValue("@ReturnDate", borrowOrReturn.ReturnDate);
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                DataRow dr = dt.Rows[0];
                json.Status = dr["Status"].ToString();
                json.Message = dr["Message"].ToString();
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetStatData()
        {
            Dashboard dashboard = new Dashboard();
            JsonResponse json = new JsonResponse();
            try
            {
                List<Dashboard> dashboardList = new List<Dashboard>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetStatData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    dashboard.TotalBooks = Convert.ToInt32(dr["Total Books"]);
                    dashboard.TotalMembers = Convert.ToInt32(dr["Total Members"]);
                    dashboard.BooksBorrowedToday = Convert.ToInt32(dr["Books Borrowed Today"]);
                    dashboard.BooksReturnedToday = Convert.ToInt32(dr["Books Returned Today"]);
                    dashboard.OverdueBooks = Convert.ToInt32(dr["Overdue Books"]);
                    dashboardList.Add(dashboard);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = dashboardList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetRecentlyBorrowedData()
        {
            RecentlyBorrowed recentlyBorrowed = new RecentlyBorrowed();
            JsonResponse json = new JsonResponse();
            try
            {
                List<RecentlyBorrowed> recentlyBorrowedList = new List<RecentlyBorrowed>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetRecentlyBorrowedData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    recentlyBorrowed.ID = Convert.ToInt32(dr["ID"]);
                    recentlyBorrowed.Title = dr["Title"].ToString();
                    recentlyBorrowed.Author = dr["Author"].ToString();
                    recentlyBorrowed.Category = dr["Category"].ToString();
                    recentlyBorrowedList.Add(recentlyBorrowed);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = recentlyBorrowedList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetRecentlyReturnData()
        {
            RecentlyReturned recentlyReturned = new RecentlyReturned();
            JsonResponse json = new JsonResponse();
            try
            {
                List<RecentlyReturned> recentlyReturnedList = new List<RecentlyReturned>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetRecentlyReturnData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    recentlyReturned.ReturnID = Convert.ToInt32(dr["ID"]);
                    recentlyReturned.MemberName = dr["Member name"].ToString();
                    recentlyReturned.Author = dr["Author"].ToString();
                    recentlyReturned.ReturnDate = dr["Return date"].ToString();
                    recentlyReturnedList.Add(recentlyReturned);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = recentlyReturnedList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetOverdueData()
        {
            Overdue overdue = new Overdue();
            JsonResponse json = new JsonResponse();
            try
            {
                List<Overdue> overdueList = new List<Overdue>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetOverdueData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    overdue.ID = Convert.ToInt32(dr["ID"]);
                    overdue.MemberName = dr["Member name"].ToString();
                    overdue.Title = dr["Title"].ToString();
                    overdue.DueDate = dr["Due date"].ToString();
                    overdueList.Add(overdue);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = overdueList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetBookData()
        {
            Book book = new Book();
            JsonResponse json = new JsonResponse();
            try
            {
                List<Book> bookList = new List<Book>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetBookData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    book.ID = Convert.ToInt32(dr["ID"]);
                    book.Title = dr["Title"].ToString();
                    book.Author = dr["Author"].ToString();
                    book.Category = dr["Category"].ToString();
                    book.CopiesAvailable = Convert.ToInt32(dr["CopiesAvailable"]);
                    book.IsAvailable = Convert.ToBoolean(dr["IsAvailable"]);
                    bookList.Add(book);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = bookList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetMemberData()
        {
            User user = new User();
            JsonResponse json = new JsonResponse();
            try
            {
                List<User> userList = new List<User>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetMemberData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    user.ID = Convert.ToInt32(dr["ID"]);
                    user.MemberName = dr["Membername"].ToString();
                    user.EmailID = dr["EmailID"].ToString();
                    user.BooksCount = Convert.ToInt32(dr["BooksCount"]);
                    userList.Add(user);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = userList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetBorrowedData()
        {
            Borrow borrow = new Borrow();
            JsonResponse json = new JsonResponse();
            try
            {
                List<Borrow> borrowList = new List<Borrow>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetBorrowedData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    borrow.ID = Convert.ToInt32(dr["ID"]);
                    borrow.MemberName = dr["Membername"].ToString();
                    borrow.Title = dr["Title"].ToString();
                    borrow.BookQuantity = Convert.ToInt32(dr["Book Quantity"]);
                    borrow.BorrowedDate = dr["Borrowed Date"].ToString();
                    borrow.DueDate = dr["Due Date"].ToString();
                    borrow.BookStatus = dr["Status"].ToString();
                    borrowList.Add(borrow);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = borrowList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
        public JsonResponse GetReturnData()
        {
            Return ret = new Return();
            JsonResponse json = new JsonResponse();
            try
            {
                List<Return> returnList = new List<Return>();
                SqlConnection connection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand("SP_GetReturnData", connection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                connection.Open();

                DataTable dt = new DataTable();

                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    ret.ID = Convert.ToInt32(dr["ID"]);
                    ret.MemberName = dr["Membername"].ToString();
                    ret.Title = dr["Title"].ToString();
                    ret.BookQuantity = Convert.ToInt32(dr["Book Quantity"]);
                    ret.BorrowedDate = dr["Borrowed Date"].ToString();
                    ret.DueDate = dr["Due Date"].ToString();
                    ret.ReturnDate = dr["Return Date"]?.ToString();
                    ret.BookStatus = dr["Status"].ToString();
                    returnList.Add(ret);
                }
                json.Message = "Success";
                json.Status = "S";
                json.Data = returnList;
                connection.Close();
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = "Something went wrong";
            }
            return json;
        }
    }
}
