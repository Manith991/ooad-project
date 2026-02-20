namespace OOAD_Project.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "employee";
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        private string? _imagePath;
        public string? Image
        {
            get => _imagePath;
            set => _imagePath = value;
        }
        public string? ImagePath
        {
            get => _imagePath;
            set => _imagePath = value;
        }
    }
}