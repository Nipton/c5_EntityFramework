using Microsoft.EntityFrameworkCore;
using Server.Abstraction;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server
{
    public class Server
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        IMessageSource messageSource;
        Dictionary<string, IPEndPoint> clients;
        //UdpClient server;
        public Server(IMessageSource source)
        {
            messageSource = source;
            clients = new Dictionary<string, IPEndPoint>();
            //server = new UdpClient(5000);
        }
        public Task Start()
        {
            return Task.Run(async () => await RunServerAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
        public async Task LoginAsync(User user, IPEndPoint iPEndPoint)
        {
            clients[user.Name] = iPEndPoint;

            using (ChatContext chatContext = new ChatContext())
            {
                try
                {
                    if (chatContext.Users.FirstOrDefault(x => x.Name == user.Name) == null)
                    {
                        await chatContext.Users.AddAsync(user);
                        await chatContext.SaveChangesAsync();
                    }
                    else
                    {
                        await CheckUnreadMessagesAsync(user.Name);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public async Task RunServerAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Сервер запущен.");
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    //var buffer = await server.ReceiveAsync();
                    //var data = Encoding.UTF8.GetString(buffer.Buffer);
                    //Message? message = Message.FromJson(data);
                    var (message, remoteEndPoint) = await messageSource.ReceiveAsync();
                    if (message != null)
                    {
                        await HandleMessageAsync(message, remoteEndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task HandleMessageAsync(Message message, IPEndPoint remoteEndPoint)
        {
            Console.WriteLine(message);
            if (message.Command == Command.Login)
            {
                if (message.FromUser != null)
                    await LoginAsync(message.FromUser, remoteEndPoint);
            }
            else if (message.Command == Command.Message)
            {
                bool statusSend = await SendMessageAsync(message); // можно потом добавить ответ сервера, если пользователь не найден.
            }
            else if (message.Command == Command.Confirmation)
            {
                await ConfirmMessageReceiptAsync(message);
            }
        }
        public async Task<bool> SendMessageAsync(Message message)
        {
            using (ChatContext chatContext = new ChatContext())
            {
                bool statusSend = false;               
                if(message.ToUser == null || message.FromUser == null)
                    return statusSend;
                var toUser = chatContext.Users.FirstOrDefault(x => x.Name == message.ToUser.Name);
                var fromUser = chatContext.Users.FirstOrDefault(x => x.Name == message.FromUser!.Name);
                if (toUser != null && fromUser != null)
                {
                    IPEndPoint newiPEndPoint;
                    Message newMessage; //= new Message { Id = message.Id, FromUser =  fromUser, ToUser = toUser, Text = message.Text, TimeMessage = message.TimeMessage };
                    try
                    {
                        if(chatContext.Messages.Any(x => x.Id == message.Id))
                        {
                            newMessage = chatContext.Messages.Find(message.Id)!;
                        }
                        else
                        {
                            newMessage = new Message { Id = message.Id, FromUser = fromUser, ToUser = toUser, Text = message.Text, TimeMessage = message.TimeMessage };
                            chatContext.Messages.Add(newMessage);
                        }                      
                        await chatContext.SaveChangesAsync();
                        
                        if(clients.TryGetValue(message.ToUser.Name, out newiPEndPoint!))
                        {
                            //var data = Encoding.UTF8.GetBytes(newMessage.ToJson());
                            //await server.SendAsync(data, newiPEndPoint);
                            await messageSource.SendAsync(newMessage, newiPEndPoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    return true;
                }
                else 
                    return statusSend;
            }
        }
        public async Task ConfirmMessageReceiptAsync(Message message)
        {
            using (ChatContext chatContext = new ChatContext())
            {

                try
                {
                    var msg = chatContext.Messages.FirstOrDefault(x => x.Id == message.Id);
                    if (msg != null)
                    {
                        msg.ReceivedStatus = true;
                        await chatContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public async Task CheckUnreadMessagesAsync(string name)
        {
            using (ChatContext chatContext = new ChatContext())
            {
                try
                {
                    User? user = await chatContext.Users.SingleOrDefaultAsync(u => u.Name == name);
                    if (user == null)
                        return;
                    var unreadMessages = await chatContext.Messages.Where(m => m.ToUserId == user.Id && !m.ReceivedStatus).ToListAsync();

                    foreach (var message in unreadMessages)
                    {
                        message.FromUser = chatContext.Users.FirstOrDefault(user => user.Id == message.FromUserId);
                        await SendMessageAsync(message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            Console.WriteLine("Сервер остановлен.");
        }
    }
}
