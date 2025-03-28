using SillyChatBackend.DTOs;
using SillyChatBackend.Models;
using SillyChatBackend.Repositories;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Services;

public interface IFriendService
{
    FriendInfo[] GetAllFriends(uint userId);
    FriendRequest[] GetAllFriendRequests(uint userId);
    FriendRequestStatus RespondFriendRequest(uint userId, uint friendRequestId, uint senderId, string response);
    FriendRequestStatus SendFriendRequest(uint userId, uint friendId);
}

public class FriendService(
    IUserRepository userRepository,
    IFriendRepository friendRepository,
    WebsocketConnectionManager manager) : IFriendService
{
    public FriendInfo[] GetAllFriends(uint userId)
    {
        var friends = friendRepository.GetAllFriends(userId);
        return
        [
            .. friends.Select(f => new FriendInfo(
                f.Id,
                f.FriendUser!.Username,
                f.FriendUser!.Email
            ))
        ];
    }

    public FriendRequest[] GetAllFriendRequests(uint userId)
    {
        var friendRequests = friendRepository.GetFriendRequestsByUserId(userId);
        return
        [
            .. friendRequests.Select(f => new FriendRequest(
                f.Id,
                f.User!.Id,
                f.User!.Username,
                f.User.Email
            ))
        ];
    }

    public FriendRequestStatus RespondFriendRequest(uint userId, uint friendRequestId, uint senderId, string response)
    {
        var friendRequest = friendRepository.GetFriendById(friendRequestId);
        if (friendRequest == null)
        {
            return FriendRequestStatus.NotFound;
        }

        if (friendRequest.FriendId != userId)
        {
            return FriendRequestStatus.Unauthorized;
        }

        if (friendRequest.Status != "pending")
        {
            return FriendRequestStatus.AlreadyResponded;
        }

        if (response == "accepted" || response == "rejected")
        {
            friendRequest.Status = response;
        }
        else
        {
            return FriendRequestStatus.InvalidResponse;
        }

        friendRepository.UpdateFriend(friendRequest);
        return FriendRequestStatus.Success;
    }

    public FriendRequestStatus SendFriendRequest(uint userId, uint friendId)
    {
        var friend = userRepository.GetUserById(userId);
        if (friend == null)
        {
            return FriendRequestStatus.NotFound;
        }

        var friendRequest = friendRepository.GetFriendByUserIds(userId, friendId);
        if (friendRequest == null)
        {
            friendRequest = new Friend
            {
                UserId = userId,
                FriendId = friendId,
                Status = "pending"
            };
            friendRepository.CreateFriend(friendRequest);
            _ = manager.SendFriendRequestToUser(friendId);
            return FriendRequestStatus.Success;
        }

        if (friendRequest.Status == "accepted")
        {
            return FriendRequestStatus.AlreadyResponded;
        }

        friendRequest.Status = "pending";
        friendRepository.UpdateFriend(friendRequest);
        _ = manager.SendFriendRequestToUser(friendId);
        return FriendRequestStatus.Success;
    }
}