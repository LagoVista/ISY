using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.ISY.Services;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using LagoVista.ISY.Models;
using LagoVista.Core.ServiceCommon;
using LagoVista.Core.Interfaces;
using LagoVista.Core;

namespace LagoVista.ISY.UI.UWP.Services
{
    public class ISYEventListener : ServiceBase
    {
        private MessageWebSocket _messageWebSocket;
        private const string STR_Isyjson = "isy.json";

        public event EventHandler<ISYEvent> ISYEventReceived;

        private static ISYEventListener _instance = new ISYEventListener();

        public static ISYEventListener Instance { get { return _instance; } }

        LagoVista.ISY.Services.ISYService _isyService;

        public ISYEventListener()
        {
            ConnectionSettings.ValidationAction = () => 
            {
                return !String.IsNullOrEmpty(ConnectionSettings.Uri) &&
                       !String.IsNullOrEmpty(ConnectionSettings.UserName) &&
                       !String.IsNullOrEmpty(ConnectionSettings.Password);
            };
        }

        public async Task<bool> StartListening(LagoVista.ISY.Services.ISYService service, IConnectionSettings settings)
        {
            try
            {
                ConnectionSettings.Uri = settings.Uri;
                ConnectionSettings.UserName = settings.UserName;
                ConnectionSettings.Password = settings.Password;

                _isyService = service;

                var uri = new Uri(String.Format("ws://{0}/rest/subscribe", ConnectionSettings.Uri));

                var authInfo = String.Format("{0}:{1}", ConnectionSettings.UserName, ConnectionSettings.Password);
                authInfo = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authInfo));

                _messageWebSocket = new MessageWebSocket();
                _messageWebSocket.SetRequestHeader("Origin", "com.universal-devices.websockets.isy");
                _messageWebSocket.SetRequestHeader("Authorization", "Basic " + authInfo);

                _messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                _messageWebSocket.Control.SupportedProtocols.Add("ISYSUB");
                _messageWebSocket.MessageReceived += MessageWebSocket_MessageReceived;
                _messageWebSocket.Closed += MessageWebSocket_Closed;

                await _messageWebSocket.ConnectAsync(uri);
                IsConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Core.PlatformSupport.Services.Logger.LogException("ISYListener.StartListening", ex);
                IsConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            lock (this)
            {
                if (_messageWebSocket != null)
                {
                    _messageWebSocket.Close(1000, String.Empty);
                    _messageWebSocket.Dispose();
                    _messageWebSocket = null;
                    IsConnected = false;
                }
            }
        }

        private void MessageWebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            if (_messageWebSocket != null)
            {
                _messageWebSocket.Dispose();
                _messageWebSocket = null;
                IsConnected = false;
            }
        }

        private void MessageWebSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            using (DataReader reader = args.GetDataReader())
            {
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                var eventXml = reader.ReadString(reader.UnconsumedBufferLength);

                var isyUpdateEvent = ISYEvent.Create(eventXml);
                if (isyUpdateEvent == null)
                {
                    return;
                }

                var node = _isyService.DataContext.Devices.Where(addr => addr.Address == isyUpdateEvent.Node).FirstOrDefault();

                var name = node == null ? "?" : node.Name;
                Log("ISY Listener", String.Format("\r\nSeq# {0} Sid# {1} Device =>  {2},{3} = {4} {5} ", isyUpdateEvent.SequenceNumber, isyUpdateEvent.Sid, name, isyUpdateEvent.Node, isyUpdateEvent.Action, DateTime.Now));

                if (node != null)
                {
                    Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
                    {
                        node.Status = isyUpdateEvent.Action;
                    });

                    isyUpdateEvent.Device = node;

                    if (isyUpdateEvent != null)
                    {
                        if (isyUpdateEvent != null && ISYEventReceived != null)
                            ISYEventReceived(this, isyUpdateEvent);

                        if (isyUpdateEvent.ControlType == ISYEvent.ControlTypes.DeviceStatus)
                        {
                            Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
                            {

                                if (node.DeviceType.IsSensor)
                                    ISYService.Instance.DataContext.Events.Insert(0, isyUpdateEvent);

                            });
                        }
                        else
                        {
                            /*Debug.WriteLine("CONTROL TYPE => " + isyUpdateEvent.ControlType.ToString());
                            Debug.WriteLine("ACTION TYPE => " + isyUpdateEvent.ActionType.ToString());
                            if (String.IsNullOrEmpty(isyUpdateEvent.Node))
                                Debug.WriteLine("NODE => " + isyUpdateEvent.Node);

                            if (isyUpdateEvent.EventInfo == null && !String.IsNullOrEmpty(isyUpdateEvent.EventInfo.Status))
                            {
                                Debug.WriteLine("Status => " + isyUpdateEvent.EventInfo.Status);
                            }*/
                        }
                    }
                }
            }
        }
    }
}
