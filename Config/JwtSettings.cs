namespace SillyChatBackend.Config
{
    public class JwtSettings
    {
        public required string AccessSecret { get; set; }
        public required string RefreshSecret { get; set; }
    }
}