using Server.Abstraction;
using Server.Models;
using System.Net;

namespace ServerTests
{
    public class MockMessageSource : IMessageSource
    {
        Queue<Message> messages = new Queue<Message>();
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Server.Server server;

        public MockMessageSource()
        {
            messages.Enqueue(new Message { Command = Command.Login, FromUser = new User("Ivan") });
            messages.Enqueue(new Message { Command = Command.Login, FromUser = new User("Petr") });
            messages.Enqueue(new Message { Command = Command.Message, FromUser = new User("Ivan"), ToUser = new User("Petr"), Text = "Test1" });
            messages.Enqueue(new Message { Command = Command.Message, FromUser = new User("Petr"), ToUser = new User("Ivan"), Text = "Test2" });
        }
        public void AddServer(Server.Server server )
        {
            this.server = server;
        }

        public async Task<(Message?, IPEndPoint)> ReceiveAsync()
        {
            if (messages.Count == 0)
            {
               Stop();
               return (null, null);
            }
            var msg = messages.Dequeue();
            return (msg, remoteEndPoint);
        }

        public Task SendAsync(Message message, IPEndPoint ep)
        {
            return Task.CompletedTask;
        }

        public void Stop()
        {
            server.Stop();
        }
    }
    public class ServerTests
    {
        [SetUp]
        public void Setup()
        {
            using (ChatContext chatContext = new ChatContext())
            {
                chatContext.Messages.RemoveRange(chatContext.Messages);
                chatContext.Users.RemoveRange(chatContext.Users);
                chatContext.SaveChanges();
            }
        }
        [TearDown]
        public void TearDown()
        {
            using (ChatContext chatContext = new ChatContext())
            {
                chatContext.Messages.RemoveRange(chatContext.Messages);
                chatContext.Users.RemoveRange(chatContext.Users);
                chatContext.SaveChanges();
            }
        }
        [Test]
        public async Task Test1()
        {
            var mock = new MockMessageSource();
            var srv = new Server.Server(mock);
            mock.AddServer(srv);
            await srv.Start(); 
            using (ChatContext chatContext = new ChatContext())
            {
                Assert.IsTrue(chatContext.Users.Count() == 2, "Пользователи не созданы");
                var user1 = chatContext.Users.FirstOrDefault(x => x.Name == "Ivan");
                var user2 = chatContext.Users.FirstOrDefault(x => x.Name == "Petr");
                Assert.IsNotNull(user1, "Пользователь не создан");
                Assert.IsNotNull(user2, "Пользователь не создан");
                var msg1 = chatContext.Messages.FirstOrDefault(x => x.FromUser == user1 && x.ToUser == user2);
                var msg2 = chatContext.Messages.FirstOrDefault(x => x.FromUser == user2 && x.ToUser == user1);
                Assert.AreEqual("Test2", msg2.Text);
                Assert.AreEqual("Test1", msg1.Text);
            }
        }
        [Test]
        public async Task Test2()
        {
            var mock = new MockMessageSource();
            var srv = new Server.Server(mock);
            mock.AddServer(srv);
            srv.Start();

            using (ChatContext chatContext = new ChatContext())
            {
                
                var user1 = new User("Ivan");
                var user2 = new User("Petr");
                await chatContext.Users.AddAsync(user1);
                await chatContext.Users.AddAsync(user2);
                await chatContext.SaveChangesAsync();

                
                var savedUser1 = chatContext.Users.FindAsync(user1.Id);
                var savedUser2 = await chatContext.Users.FindAsync(user2.Id);

                Assert.IsNotNull(savedUser1, "Пользователь не сохранен");
                Assert.IsNotNull(savedUser2, "Пользователь не сохранен");
            }
        }
    }
}