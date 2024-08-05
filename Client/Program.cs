namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("Введите имя: ");
            string name = Console.ReadLine()!;
            Console.Write("Введите порт: ");
            int port = int.Parse(Console.ReadLine()!);
            Client client = new Client(name, port);
            _ = Task.Run(client.ClientReceveAsync);
            await client.ClientSendAsync();
        }
    }
}
