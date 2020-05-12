using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    /// <summary>
    /// The contract for communication from the client to the server
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IChatCallbackService))]
    public interface IChatService
    {
        [OperationContract]
        void Register(string name);

        [OperationContract]
        void UnRegister(string name);

        [OperationContract]
        void SendMessage(string message, string clientName);

        [OperationContract]
        void SendFile(byte[] fileData, string fileName, string clientName);
    }
}
