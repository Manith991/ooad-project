namespace OOAD_Project.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "employee";
        public string Status { get; set; } = "active";
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
