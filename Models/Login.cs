namespace CRM.Models
{
    public class Login
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Role { get; set; }
        public int RoleID { get; set; }
        public string Team { get; set; }
        public int? TeamID { get; set; }
        public bool IsActive { get; set; }
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public string EmailVerificationToken { get; set; }
    }
}
