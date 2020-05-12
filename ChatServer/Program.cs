using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ChatService service = new ChatService();
            ServiceHost host = new ServiceHost(service);
            host.Open();
            Console.WriteLine($"Host opened on {host.BaseAddresses[0]}");

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("Press Esc to Exit");
            }
        }
    }
}
