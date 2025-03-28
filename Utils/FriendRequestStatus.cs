namespace SillyChatBackend.Utils;

public enum FriendRequestStatus
{
    Success,
    Unauthorized, // User ID does not match
    NotFound, // Friend request ID not found
    InvalidSender, // Sender ID does not match
    InvalidResponse, // Response is invalid
    AlreadyResponded // The friend request was already accepted/declined
}