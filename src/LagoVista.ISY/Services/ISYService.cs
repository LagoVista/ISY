using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using LagoVista.ISY.Models;
using LagoVista.Core.ServiceCommon;

namespace LagoVista.ISY.Services
{
    public class ISYService : ServiceBase
    {

        /*
/rest/status -- returns the status for all the nodes 

/rest/status/<node> -- returns the status for the given node 
*/

        /*
/rest/nodes/<node>/cmd/<command_name>/<param1>/<param2>/.../<param5> eg: 

/rest/nodes/<node>/cmd/DOF - turn off a device or a scene 

/rest/nodes/<node>/cmd/DON/128 - turn on a device to 50% - valid parameters = 0 - 255 

/rest/nodes/<node>/cmd/DFON - turn on a device fast 

/rest/nodes/<node>/cmd/DFOF - turn off a device fast 

/rest/nodes/<node>/cmd/BRT - increase brightness of a device by ~3% 

/rest/nodes/<node>/cmd/DIM - decrease brightness of a device by ~3% 

/rest/nodes/<node>/cmd/BMAN - begin manual dimming 

/rest/nodes/<node>/cmd/SMAN - stop manual dimming 
        */


        public const String DeviceOff = "DOF";
        public const String DeviceOn = "DON";
        public const String DeviceFastOff = "DFOF";
        public const String DeviceFastOn = "DFON";
        public const String Bright = "BRT";
        public const String Dim = "DIM";

        private String GetStatus(List<XElement> properyElements, string attrId)
        {
            var status = (from ele
                         in properyElements
                          where ele.Attribute("id") != null && ele.Attribute("id").Value == "ST"
                          select ele.Attribute(attrId).Value).FirstOrDefault();

            return status;
        }

        private void ParseStream(XDocument doc, ISYDataContext ctx)
        {
            var folders = (from fld
                  in doc.Descendants()
                           where fld.Name == "folder"
                           select new Models.Folder
                           {
                               Id = Convert.ToInt32(fld.Element("address").Value),
                               Name = fld.Element("name").Value
                           }).ToList();

            foreach (var folder in folders)
                ctx.Folders.Add(folder);

            var devices = (from dvc
                        in doc.Descendants()
                           where dvc.Name == "node"
                           select new Models.Device
                           {
                               Address = dvc.Element("address").Value,
                               Name = dvc.Element("name").Value,
                               DeviceTypeId = dvc.Element("type").Value,
                               PNode = dvc.Element("pnode").Value,
                               Parent = dvc.Element("parent") != null ? dvc.Element("parent").Value : String.Empty,
                               Status = GetStatus(dvc.Elements().Where(ele => ele.Name == "property").ToList(), "formatted"),
                               Value = GetStatus(dvc.Elements().Where(ele => ele.Name == "property").ToList(), "value"),
                               UOM = GetStatus(dvc.Elements().Where(ele => ele.Name == "property").ToList(), "uom")
                           }).ToList();
            ctx.Devices.Clear();
            foreach (var device in devices)
                ctx.Devices.Add(device);

            var scenes = (from dvc
                      in doc.Descendants()
                          where dvc.Name == "group"
                          select new Models.Scene
                          {
                              Address = dvc.Element("address").Value,
                              Name = dvc.Element("name").Value,
                              MemberAddresses = (from lnk in dvc.Descendants() where lnk.Name == "link" select lnk.Value).ToList(),
                              Parent = dvc.Element("parent") != null ? dvc.Element("parent").Value : String.Empty
                          }).ToList();

            ctx.Scenes.Clear();
            foreach (var scene in scenes)
                ctx.Scenes.Add(scene);


            foreach (var device in ctx.Devices)
            {
                var deviceId = device.DeviceTypeId;
                if (deviceId == "1.46.65.0")
                {
                    if (device.Address.EndsWith("1"))
                        device.DeviceTypeId = "1.46.65.0.light";
                    else if (device.Address.EndsWith("2"))
                        device.DeviceTypeId = "1.46.65.0.motor";
                }

                if (deviceId == "7.0.65.0")
                {
                    if (device.Address.EndsWith("1"))
                        device.DeviceTypeId = "7.0.65.0.sensor";
                    else if (device.Address.EndsWith("2"))
                        device.DeviceTypeId = "7.0.65.0.switch";
                }



                device.DeviceType = Models.Device.DeviceTypeTable.ContainsKey(device.DeviceTypeId) ? Models.Device.DeviceTypeTable[device.DeviceTypeId] :
                    new DeviceType() { DeviceName = device.DeviceTypeId, DeviceCategory = DeviceType.DeviceCateogry.Unknown };
            }

            var unknownDevices = ctx.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.Unknown).ToList();

            ctx.PopulateDevices();
            ctx.PopulateScenes();
        }


        private ISYDataContext _dataContext = new ISYDataContext();
        public ISYDataContext DataContext { get { return _dataContext; } }


        public Task<bool> RefreshAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                var webAddress = String.Format("http:{0}//{1}:{2}/rest/nodes", ConnectionSettings.IsSSL ? "S" : String.Empty, ConnectionSettings.Uri, ConnectionSettings.Port);
                var request = HttpWebRequest.CreateHttp(webAddress);
                request.Method = "GET";
                string authInfo = String.Format("{0}:{1}", ConnectionSettings.UserName, ConnectionSettings.Password); ;
                authInfo = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authInfo));
                request.Headers["Authorization"] = "Basic " + authInfo;
                request.Accept = "text/xml; charset=utf-8";

                request.BeginGetResponse((asynchronousResult) =>
                {
                    var rqst = (WebRequest)asynchronousResult.AsyncState;

                    try
                    {
                        // End the Asynchronous response.
                        using (var response = rqst.EndGetResponse(asynchronousResult))
                        using (var responseStream = response.GetResponseStream())
                        {
                            var doc = XDocument.Load(responseStream);
                            Core.PlatformSupport.Services.DispatcherServices.Invoke(() =>
                            {
                                ParseStream(doc, _dataContext);
                                IsConnected = true;
                                tcs.SetResult(true);
                                responseStream.Dispose();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.PlatformSupport.Services.Logger.LogException("ISY.RefreshAsync", ex);
                        IsConnected = false;
                        tcs.SetResult(false);
                    }

                }, request);
            }
            catch (Exception ex)
            {
                Core.PlatformSupport.Services.Logger.LogException("ISY.RefreshAsync", ex);
                IsConnected = false;
                tcs.SetResult(false);
            }

            return tcs.Task;
        }

        //<?xml version="1.0" encoding="UTF-8"?><properties><property id="ST" value="0" formatted="Off" uom="%/on/off"/></properties>

        public async Task SendCommand(String node, String command)
        {
            await SendCommandAsync(node, command, null);
        }


        public async Task SendCommandAsync(String node, String command, int? level)
        {
            var requestUrl = String.Format("http{0}://{1}:{2}/rest/nodes/{3}/cmd/{4}", ConnectionSettings.IsSSL ? "s" : "", ConnectionSettings.Uri, ConnectionSettings.Port, node, command);
            if (level.HasValue)
                requestUrl += String.Format("/{0}", (level.Value * 255) / 100);

            requestUrl = requestUrl.Replace(" ", "%20");

            var client = new HttpClient();

            if (ConnectionSettings.UserName.Length > 0 && ConnectionSettings.Password.Length > 0)
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", ConnectionSettings.UserName, ConnectionSettings.Password));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            using (var response = await client.GetAsync(requestUrl))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Response Text " + responseText);
            }

            Debug.WriteLine("REQUEST => " + requestUrl);
        }

        private static ISYService _instance = new ISYService();
        public static ISYService Instance
        {
            get { return _instance; }
        }

        public delegate void statusMethod(DeviceStatus parts);

        public async Task<DeviceStatus> RefreshNodeStatusAsync(String node)
        {
            var client = new HttpClient();

            if (ConnectionSettings.UserName.Length > 0 && ConnectionSettings.Password.Length > 0)
            {
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(String.Format("{0}:{1}", ConnectionSettings.UserName, ConnectionSettings.Password));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

            var requestUrl = String.Format("http{0}://{1}:{2}/rest/status/{3}", ConnectionSettings.IsSSL ? "s" : "", ConnectionSettings.Uri, ConnectionSettings.Port, node).Replace(" ", "%20");
            using (var response = await client.GetAsync(requestUrl))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                var doc = XDocument.Load(stream);

                var property = (from stat
                           in doc.Descendants()
                                where stat.Name == "property"
                                  && stat.Attribute("id") != null
                                  && stat.Attribute("id").Value == "ST"
                                select new DeviceStatus()
                                {
                                    Value = stat.Attribute("value").Value,
                                    Display = stat.Attribute("formatted").Value,
                                    UOM = stat.Attribute("uom").Value
                                }).FirstOrDefault();

                return property;
            }
        }

        public override string GetCredentialsPrefix()
        {
            return "ISY994i";
        }
    }
}
