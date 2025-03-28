using SillyChatBackend.Data;
using SillyChatBackend.Models;

namespace SillyChatBackend.Repositories
{
    public interface IUserRepository
    {
        User? GetUserById(uint id);
        User? GetUserByEmail(string email);
        User? GetUserByUsername(string username);
        User? CreateUser(User user);
        User? UpdateUser(User user);
        void DeleteUser(User user);
    }
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public User? GetUserById(uint id)
        {
            return context.Users.Find(id);
        }

        public User? GetUserByEmail(string email)
        {
            return context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User? GetUserByUsername(string username)
        {
            return context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User? CreateUser(User user)
        {
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public User? UpdateUser(User user)
        {
            context.Users.Update(user);
            context.SaveChanges();
            return user;
        }

        public void DeleteUser(User user)
        {
            context.Users.Remove(user);
            context.SaveChanges();
        }
    }
}