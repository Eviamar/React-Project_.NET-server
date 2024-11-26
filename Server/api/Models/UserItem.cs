namespace Server.api.Models
{
    public class UserItem
    {

        public required int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; } 
        public required string Email { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string FullName { get; set; } = string.Empty;
        public bool IsVerified { get; set; } = false;
    }
}