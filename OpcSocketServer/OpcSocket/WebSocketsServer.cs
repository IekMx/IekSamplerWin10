using Newtonsoft.Json;
using opclibrary.Mappings;
using opclibrary.Services;
using OpcSocket.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace OpcSocket
{
    public class WebSocketsServer : IWebSocketsServer
    {
        private opclibrary.Services.OpcManager _opcManager = opclibrary.Services.OpcManager.Instance;
        private List<OperationContext> _operationContexts = new List<OperationContext>();

        public WebSocketsServer()
        {
            var opcx = OperationContext.Current;
            if (!_operationContexts.Contains(opcx))
            {
                _operationContexts.Add(opcx);
            }
            if (_opcManager != null)
            {
                _opcManager.DataChanged += _opcManager_DataChanged;
            }
        }

        private void _opcManager_DataChanged(object sender, opclibrary.Mappings.OpcEventArgs e)
        {
            _operationContexts.ForEach(x => {
                var callback = x.GetCallbackChannel<IProgressContext>();
                if (((IChannel)callback).State != CommunicationState.Opened) { return; }
                callback.ReportProgress(
                    CreateMessage(
                        JsonConvert.SerializeObject(
                            new
                            {
                                handle = e.ItemHandle,
                                value = e.ItemValue.ToString(),
                                name = opclibrary.Services.Module1.TagNameArray.GetValue(e.ItemHandle)
                            }
                        )
                    )
                );
            });
        }

        public void SendMessageToServer(Message msg)
        {
            if (msg.IsEmpty) return;
            var callback = OperationContext.Current.GetCallbackChannel<IProgressContext>();
            try
            {
                var bytes = msg.GetBody<byte[]>();
                var tag = JsonConvert.DeserializeObject<OpcTag>(Encoding.ASCII.GetString(bytes));
                Module1.TagList.Where(x => x.Name == tag.Name).FirstOrDefault().Value = tag.Value;
                _opcManager.Write(tag.Handle);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Message exception: " + e.Message);
            }
            //await Task.Run(() =>
            //{
            //});
        }

        private Message CreateMessage(string msgText)
        {
            Message msg = ByteStreamMessage.CreateMessage(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgText)));

            msg.Properties["WebSocketMessageProperty"] =
                new WebSocketMessageProperty
                {
                    MessageType = WebSocketMessageType.Text
                };

            return msg;
        }
    }

}
