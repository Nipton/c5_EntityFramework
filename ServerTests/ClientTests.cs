using Server.Abstraction;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatTests
{
    public class MockMessageSource : IMessageSource
    {
        Queue<Message> messages = new Queue<Message>();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public Task<(Message?, IPEndPoint)> ReceiveAsync()
        {
            Message message = new Message { Command = Command.Message, FromUser = new User("Ivan"), ToUser = new User("Petr"), Text = "Test1" };
            return Task.FromResult((message, remoteEndPoint));
        }

        public Task SendAsync(Message message, IPEndPoint ep)
        {
            return Task.CompletedTask;
        }
    }
    internal class ClientTests
    {
        [Test]
        public void TestReceive()
        {
            var source = new MockMessageSource();
            var client = new Client.Client("Ivan", source);
            var (message, _) = source.ReceiveAsync().Result;
            Assert.AreEqual("Test1", message?.Text);
            Assert.IsNotNull(message);
            Assert.IsNotNull(message.FromUser);
        }
    }
}
