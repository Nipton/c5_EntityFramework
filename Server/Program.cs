using Server.Abstraction;
using System.Net;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IMessageSource messageSource = new UDPMessageSource(5000);
            Server server = new Server(messageSource);
            server.Start();
            Console.ReadLine();
            server.Stop();
            Console.ReadKey();
        }
    }
}
