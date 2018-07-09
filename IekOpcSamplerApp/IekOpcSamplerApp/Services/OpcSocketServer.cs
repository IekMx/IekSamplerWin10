using IekOpcSamplerApp.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace IekOpcSamplerApp.Services
{
    public delegate void OpcSocketClientStatusMonitor(object sender, Enums.OpcSocketClientStatus status);
    public delegate void OpcSocketClientTagValueMonitor(object sender, Tag tag);

    internal class OpcSocketServer
    {
        public event OpcSocketClientStatusMonitor ConnectionStatusChanged;
        public event OpcSocketClientTagValueMonitor TagValueChanged;

        private MessageWebSocket _socket;
        private DataWriter _writer;
        public int IsBusy { get; set; }
        public bool IsConnected { get; set; }

        public async Task ConnectAsync()
        {
            Uri webSocketUri;
            Uri.TryCreate("ws://localhost:8889/opc", UriKind.Absolute, out webSocketUri);
            Uri server = webSocketUri;
            if (server == null)
            {
                return;
            }

            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.MessageReceived += _socket_MessageReceived;
            _socket.Closed += _socket_Closed;

            try
            {
                await _socket.ConnectAsync(server);
                IsConnected = true;
                ConnectionStatusChanged?.Invoke(this, Enums.OpcSocketClientStatus.Good);
            }
            catch (Exception ex)
            {
                ConnectionStatusChanged?.Invoke(this, Enums.OpcSocketClientStatus.Bad);
                //log4net.LogManager.GetLogger(this.GetType()).Error(JsonConvert.SerializeObject(ex));
                LogService.AddEntry(this.GetType().Name, ex);
                IsConnected = false;
                _socket.Dispose();
                _socket = null;

                return;
            }

            try
            {
                _writer = new DataWriter(_socket.OutputStream);
            }
            catch (Exception e)
            {
                LogService.AddEntry(this.GetType().Name, e);
            }
        }

        public async Task SendAsync(string message)
        {
            _writer.WriteString(message);
            try
            {
                await _writer.StoreAsync();
            }
            catch (Exception ex)
            {
                LogService.AddEntry(this.GetType().Name, ex);
            }
        }

        private void _socket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {

        }

        private void _socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {

            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                try
                {
                    string message = reader.ReadString(reader.UnconsumedBufferLength);
                    TagValueChanged?.Invoke(this, JsonConvert.DeserializeObject<Tag>(message));
                }
                catch (Exception ex)
                {
                    LogService.AddEntry(this.GetType().Name, ex);
                }
            }
        }

        private void CloseSocket()
        {
            if (_writer != null)
            {
                _writer.DetachStream();
                _writer.Dispose();
                _writer = null;
            }

            if (_socket != null)
            {
                try
                {
                    _socket.Close(1000, "Closed due to user request.");
                }
                catch (Exception ex)
                {
                    LogService.AddEntry(this.GetType().Name, ex);
                }
                _writer = null;
            }
        }

    }
}
