using LagoVista.Core.UWP.Services;
using LagoVista.ISY994i.Core.Models;
using LagoVista.ISY994i.Core.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace LagoVista.ISYAutomation.Services
{
    public class RESTClient : SSDPServer
    {
        public override async Task<bool> HandleRequestAsync(StreamSocket socket, string path)
        {
            Core.PlatformSupport.Services.Logger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "SSDPClient.HandleRequestAsync", "Handling Web Request: " + path);

            path = path.ToLower();

            if (path == "/ping")
            {
                await WriteResponseAsync(socket, "application/json", 200, "{\"ping\":\"ok\"}");
                return true;
            }
            if (path.StartsWith("/nodes"))
            {
                var parts = path.Split('/');
                if (parts.Length == 3)
                {
                    switch (parts[2])
                    {
                        case "multibutton":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.MultiSwitch && dvc.Address.EndsWith(" 1"));
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "ledbulb":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.LEDLightBulb);
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "thermostat":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.Thermostat);
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "fanmotor":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.FanMotor);
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "fanlight":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.FanLight);
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "contact":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.ContactSwitch && dvc.Address.EndsWith(" 1"));
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }
                        case "switch":
                            {
                                var devices = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.ToggleSwitch ||
                                                                                                                       dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.InlineSwitch ||
                                                                                                                       dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.ToggleSwitch ||
                                                                                                                       dvc.DeviceType.DeviceCategory == DeviceType.DeviceCateogry.OutdoorModule);
                                var json = JsonConvert.SerializeObject(devices);
                                await WriteResponseAsync(socket, "application/json", 200, json);
                                return true;
                            }


                    }
                }
            }
            else if (path.StartsWith("/node"))
            {
                var parts = path.Split('/');
                if (parts.Length == 4 || parts.Length == 5)
                {
                    var addr = parts[2].ToUpper().Replace("%20", " ");
                    var action = parts[3].ToUpper();
                    int? level = null;
                    if (parts.Length == 5)
                        level = Convert.ToInt32(parts[4]);

                    var node = ISYService.Instance.DataContext.Devices.Where(dvc => dvc.Address.ToUpper() == addr).FirstOrDefault();
                    if (node == null)
                    {
                        await WriteResponseAsync(socket, "application/json", 200, "{'success':'false','errorMsg':'nodeNotFound','addr':'" + addr + @"'}");
                    }
                    else
                    {
                        switch (action)
                        {
                            case "LVL": node.SendCommand(ISYService.DeviceOn, level); break;
                            case "ON": node.DeviceOnCommand.Execute(null); break;
                            case "OFF": node.DeviceOffCommand.Execute(null); break;
                        }

                        await WriteResponseAsync(socket, "application/json", 200, "{\"success\":\"true\"}");

                    }

                    return true;
                }
            }
            else
            {
                var json = JsonConvert.SerializeObject(ISYService.Instance.DataContext.Devices.Take(10));
                await WriteResponseAsync(socket, "application/json", 200, json);
                return true;
            }


            return false;
        }
    }
}
