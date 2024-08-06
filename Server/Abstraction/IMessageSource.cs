using Server.Models;
using System.Net;

namespace Server.Abstraction
{
    public interface IMessageSource
    {
        Task SendAsync(Message message, IPEndPoint ep);
        Task<(Message?, IPEndPoint)> ReceiveAsync();
    }
}
