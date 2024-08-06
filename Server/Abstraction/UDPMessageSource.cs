using Server.Models;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Abstraction
{
    public class UDPMessageSource : IMessageSource
    {
        private readonly UdpClient server;
        public UDPMessageSource(int port)
        {
            server = new UdpClient(port);
        }
        public async Task<(Message?, IPEndPoint)> ReceiveAsync()
        {
            var buffer = await server.ReceiveAsync();
            var data = Encoding.UTF8.GetString(buffer.Buffer);
            var tuple = (Message.FromJson(data), buffer.RemoteEndPoint);
            return tuple;
        }

        public async Task SendAsync(Message message, IPEndPoint ep)
        {
            var data = Encoding.UTF8.GetBytes(message.ToJson());
            await server.SendAsync(data, ep);
        }
        public void Stop()
        {
            server.Close();
        }
    }
}
