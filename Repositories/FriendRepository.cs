using Microsoft.EntityFrameworkCore;
using SillyChatBackend.Data;
using SillyChatBackend.Models;

namespace SillyChatBackend.Repositories;

public interface IFriendRepository
{
    Friend? GetFriendById(uint id);
    Friend[] GetAllFriends(uint userId);
    Friend[] GetFriendRequestsByUserId(uint userId);
    Friend? CreateFriend(Friend friend);
    Friend? UpdateFriend(Friend friend);
    void DeleteFriend(Friend friend);
    Friend? GetFriendByUserIds(uint userId, uint friendId);
}

public class FriendRepository(AppDbContext context) : IFriendRepository
{
    public Friend? GetFriendById(uint id)
    {
        return context.Friends.Find(id);
    }

    public Friend[] GetAllFriends(uint userId)
    {
        return
        [
            .. context.Friends.Where(f => f.FriendId == userId && f.Status == "accepted").Include(f => f.FriendUser)
        ];
    }

    public Friend[] GetFriendRequestsByUserId(uint userId)
    {
        return
        [
            .. context.Friends
                .Where(f => f.FriendId == userId && f.Status == "pending")
                .Include(f => f.User)
                .Include(f => f.FriendUser)
        ];
    }

    public Friend CreateFriend(Friend friend)
    {
        context.Friends.Add(friend);
        context.SaveChanges();
        return friend;
    }

    public Friend UpdateFriend(Friend friend)
    {
        context.Friends.Update(friend);
        context.SaveChanges();
        return friend;
    }

    public void DeleteFriend(Friend friend)
    {
        context.Friends.Remove(friend);
        context.SaveChanges();
    }

    public Friend? GetFriendByUserIds(uint userId, uint friendId)
    {
        return context.Friends.FirstOrDefault(f => f.UserId == userId && f.FriendId == friendId);
    }
}