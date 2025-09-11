using CRM.DBContext;
using CRM.Models;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace CRM.Services
{
    public class CommentServices : ICommentServices
    {
        private readonly string _connectionString;

        public CommentServices(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public JsonResponse AddComment(Comments comments)
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_AddComments";
            try
            {
                SqlConnection sqlConnection = new SqlConnection(_connectionString);

                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@UserName", comments.Name);
                sqlCommand.Parameters.AddWithValue("@Comment", comments.Message);
                sqlCommand.Parameters.Add("@ProfileImg", SqlDbType.VarBinary, -1).Value = (object)comments.Photo ?? DBNull.Value;
                sqlCommand.Parameters.AddWithValue("@PostedTime", comments.Time);
                sqlConnection.Open();
                int row = sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                json.Status = "S";
                json.Message = "Successfully added!";
            }
            catch (Exception e)
            {

            }

            return json;
        }

        public JsonResponse GetComments()
        {
            JsonResponse json = new JsonResponse();
            string proc = "SP_GetComments";
            try
            {
                List<Comments> commentList = new List<Comments>();
                SqlConnection sqlConnection = new SqlConnection(_connectionString);
                SqlCommand sqlCommand = new SqlCommand(proc, sqlConnection);
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sqlConnection.Open();
                adapter.Fill(dt);
                sqlConnection.Close();
                foreach (DataRow row in dt.Rows)
                {
                    Comments comments = new Comments();
                    comments.ID = Convert.ToInt32(row["ID"]);
                    comments.Name = row["UserName"].ToString();
                    comments.Message = row["Comment"].ToString();
                    comments.Photo = row["ProfileImg"] == DBNull.Value ? null : (byte[])row["ProfileImg"];
                    comments.Time = row["PostedTime"].ToString();
                    commentList.Add(comments);
                }
                json.Status = "S";
                json.Message = "Fetched comments successfully";
                json.Data = commentList;
            }
            catch (Exception e)
            {
                json.Status = "F";
                json.Message = e.Message;
            }

            return json;
        }
    }
}
