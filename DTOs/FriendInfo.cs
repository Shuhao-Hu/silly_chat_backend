namespace SillyChatBackend.DTOs
{
    public class FriendInfo(uint id, string username, string email)
    {
        public uint Id { get; set; } = id;
        public string Username { get; set; } = username;
        public string Email { get; set; } = email;
    }
}