using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ChatService : IChatService
    {
        private readonly Dictionary<string, OperationContext> _clients = new Dictionary<string, OperationContext>();

        public void Register(string name)
        {
            if (_clients.ContainsKey(name) == false)
            {
                _clients.Add(name, OperationContext.Current);
                Console.WriteLine($"Client Registered: {name}");
                foreach (var client in _clients.Values)
                {
                    client.GetCallbackChannel<IChatCallbackService>().ClientRegistered(name);
                }
            }
        }

        public void UnRegister(string name)
        {
            if (_clients.ContainsKey(name))
            {
                _clients.Remove(name);
                Console.WriteLine($"Client UnRegistered: {name}");
                foreach (var client in _clients.Values)
                {
                    client.GetCallbackChannel<IChatCallbackService>().ClientUnRegistered(name);
                }
            }
        }

        public void SendFile(byte[] fileData, string fileName, string clientName)
        {
            foreach (var client in _clients.Values)
            {
                client.GetCallbackChannel<IChatCallbackService>().FileReceived(fileData, fileName, clientName);
            }
        }

        public void SendMessage(string message, string clientName)
        {
            Console.WriteLine($"Received Message from {clientName}: {message}");
            foreach (var client in _clients.Values)
            {
                client.GetCallbackChannel<IChatCallbackService>().MessageReceived(message, clientName);
            }
        }
    }
}
