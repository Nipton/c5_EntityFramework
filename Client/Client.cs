using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class Client
    {
        readonly IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
        //readonly Message message = new Message();
        readonly UdpClient client;
        string name;
        User fromUser;
        public Client(string name, int port)
        {
            client = new UdpClient(port);
            this.name = name;
            fromUser = new User(name);
        }
        public async Task ClientReceveAsync()
        {
            await Register();
            try
            {
                while (true)
                {
                    var receiveAnswer = await client.ReceiveAsync();
                    string str = Encoding.UTF8.GetString(receiveAnswer.Buffer);
                    var answer = Message.FromJson(str);
                    if (answer != null)
                    {
                        Console.WriteLine(answer);
                        await ConfirmAsync(answer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public async Task ConfirmAsync(Message message)
        {
            message.Command = Command.Confirmation;
            await SendMessage(message);
        }
        public async Task Register()
        {
            Message message = new Message();
            message.FromUser = fromUser;
            await SendMessage(message);
        }
        public async Task ClientSendAsync()
        {
            while (true)
            {
                Message message = new Message();
                Console.WriteLine("Введите имя получателя: ");
                string toName = Console.ReadLine()!;
                message.ToUser = new User(toName);
                Console.WriteLine("Введите сообщение: ");
                message.FromUser = fromUser;
                message.Text = Console.ReadLine()!;
                message.TimeMessage = DateTime.Now;
                message.Command = Command.Message;
                await SendMessage(message);
            }
        }
        public async Task SendMessage(Message message)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(message.ToJson());
                await client.SendAsync(data, remotePoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
