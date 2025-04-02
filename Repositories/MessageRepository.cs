using SillyChatBackend.Data;
using SillyChatBackend.Models;

namespace SillyChatBackend.Repositories;

public interface IMessageRepository
{
    public Message[] GetUnreadMessages(uint userId);
    public Message AddMessage(Message message);
    public void MarkMessagesAsRead(Message[] messages);
}

public class MessageRepository(AppDbContext context) : IMessageRepository
{
    public Message[] GetUnreadMessages(uint userId)
    {
        return
        [
            .. context.Messages.Where(m => m.RecipientId == userId && !m.Read)
        ];
    }

    public Message AddMessage(Message message)
    {
        context.Messages.Add(message);
        context.SaveChanges();
        return message;
    }
    
    public void MarkMessagesAsRead(Message[] messages)
    {
        foreach (var message in messages)
        {
            message.Read = true;
        }
        context.SaveChanges();
    }
}