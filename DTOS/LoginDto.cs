namespace CRM.DTOS
{
    public class LoginDto
    {
        public String Email { get; set; }
        public String Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
