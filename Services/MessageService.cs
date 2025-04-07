using SillyChatBackend.Models;
using SillyChatBackend.Repositories;

namespace SillyChatBackend.Services;

public interface IMessagesService
{
    public void SendMessage(uint senderId, uint recipientId, string message);
    public Message[] GetUnreadMessages(uint userId);
}

public class MessageService(WebsocketConnectionManager manager, IMessageRepository repository) : IMessagesService
{
    public void SendMessage(uint senderId, uint recipientId, string content)
    {
        var recipientOnline = manager.ClientExists(recipientId);
        var message = new Message
        {
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content,
            Read = recipientOnline,
        };
        repository.AddMessage(message);
        if (recipientOnline)
        { 
            _ = manager.SendMessageToUser(message);
        }
    }

    public Message[] GetUnreadMessages(uint userId)
    {
        var unreadMessages = repository.GetUnreadMessages(userId);
        repository.MarkMessagesAsRead(unreadMessages);
        return unreadMessages;
    }
}