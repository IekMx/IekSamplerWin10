using System.ServiceModel;
using System.ServiceModel.Channels;

namespace OpcSocket.Contracts
{
    [ServiceContract]
    interface IProgressContext
    {
        [OperationContract(IsOneWay = true, Action = "*")]
        void ReportProgress(Message msg);
    }
}
