namespace AdManagementSystem.Models
{
    // Models/ClickTokenUsage.cs
    public class ClickTokenUsage
    {
        public int Id { get; set; }
        public string Jti { get; set; } = null!; // JWT ID
        public int AdId { get; set; }
        public int WebsiteId { get; set; }
        public DateTime UsedAt { get; set; }
        public string? UsedByIp { get; set; }
    }

}
