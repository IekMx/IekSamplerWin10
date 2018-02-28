using System.ServiceModel;
using System.ServiceModel.Channels;

namespace OpcSocket.Contracts
{
    [ServiceContract(CallbackContract = typeof(IProgressContext))]
    interface IWebSocketsServer
    {
        [OperationContract(Action = "*", IsOneWay = true)]
        void SendMessageToServer(Message msg);
    }
}
