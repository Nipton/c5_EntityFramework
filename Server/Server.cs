using Microsoft.EntityFrameworkCore;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Server
    {
        Dictionary<string, IPEndPoint> clients;
        UdpClient server;
        public Server()
        {
            clients = new Dictionary<string, IPEndPoint>();
            server = new UdpClient(5000);
        }

        public async Task LoginAsync(string name, IPEndPoint iPEndPoint)
        {
            clients[name] = iPEndPoint;

            using (ChatContext chatContext = new ChatContext())
            {
                try
                {
                    if (chatContext.Users.FirstOrDefault(x => x.Name == name) == null)
                    {
                        await chatContext.AddAsync(new User(name));
                        await chatContext.SaveChangesAsync();
                    }
                    else
                    {
                        await CheckUnreadMessagesAsync(name);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        public async Task RunServerAsync()
        {
            Console.WriteLine("Сервер запущен.");
            try
            {
                while (true)
                {
                    var buffer = await server.ReceiveAsync();
                    var data = Encoding.UTF8.GetString(buffer.Buffer);
                    Message? message = Message.FromJson(data);
                    if (message != null)
                    {
                        await HandleMessageAsync(message, buffer.RemoteEndPoint);
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
                    await LoginAsync(message.FromUser.Name, remoteEndPoint);
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
                if(message.ToUser == null)
                    return statusSend;
                var toUser = chatContext.Users.FirstOrDefault(x => x.Name == message.ToUser.Name);
                if (toUser != null)
                {
                    IPEndPoint newiPEndPoint;
                    toUser.MessagesToReceive.Add(message);
                    try
                    {
                        await chatContext.SaveChangesAsync();
                        if(clients.TryGetValue(message.ToUser.Name, out newiPEndPoint!))
                        {
                            var data = Encoding.UTF8.GetBytes(message.ToJson());
                            await server.SendAsync(data, newiPEndPoint);
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
                    User? user = chatContext.Users.Include(m => m.MessagesToReceive).FirstOrDefault(x => x.Name == name);
                    if (user != null)
                    {
                        foreach(var message in user.MessagesToReceive)
                        {
                            await SendMessageAsync(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
