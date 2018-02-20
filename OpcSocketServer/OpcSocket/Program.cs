using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace OpcSocket
{
    class Program
    {
        static void Main(string[] args)
        {

            Uri baseAddress = new Uri("http://localhost:8888/opc");

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(WebSocketsServer), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                CustomBinding binding = new CustomBinding();
                binding.Elements.Add(new ByteStreamMessageEncodingBindingElement());
                HttpTransportBindingElement transport = new HttpTransportBindingElement();
                //transport.WebSocketSettings = new WebSocketTransportSettings();
                transport.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
                transport.WebSocketSettings.CreateNotificationOnConnection = true;
                binding.Elements.Add(transport);

                host.AddServiceEndpoint(typeof(IWebSocketsServer), binding, "");
                
                host.Open();
                
                Console.WriteLine("The service is ready at {0}", baseAddress);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }

    [ServiceContract(CallbackContract = typeof(IProgressContext))]
    public interface IWebSocketsServer
    {
        [OperationContract(Action = "*", IsOneWay = true)]
        void SendMessageToServer(Message msg);
    }

    [ServiceContract]
    interface IProgressContext
    {
        [OperationContract(IsOneWay = true, Action = "*")]
        void ReportProgress(Message msg);
    }
    
}
