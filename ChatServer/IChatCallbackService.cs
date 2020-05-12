using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    /// <summary>
    /// The contract for communication from the server to the client
    /// </summary>
    [ServiceContract]
    public interface IChatCallbackService
    {
        [OperationContract]
        void MessageReceived(string message, string clientName);

        [OperationContract]
        void FileReceived(byte[] file, string fileName, string clientName);

        [OperationContract]
        void ClientRegistered(string clientName);

        [OperationContract]
        void ClientUnRegistered(string clientName);
    }
}
