using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LagoVista.ISY994i.Core.Resources;
using LagoVista.ISY994i.Core.Services;

namespace LagoVista.ISY994i.Core.Models
{
    [DataContract]
    public class Device : ModelBase
    {
        String _status;

        [DataMember]
        public String Address { get; set; }
        [IgnoreDataMember]
        public String NameLowerCase { get { return Name.ToLower(); } }

        [IgnoreDataMember]
        public String RestAddress
        {
            get
            {
                var bldr = new StringBuilder();
                var parts = Address.Split(' ');
                foreach (var part in parts)
                {
                    bldr.Append(part.TrimStart('0'));
                    bldr.Append(" ");
                }

                return bldr.ToString().Trim();
            }
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public int RoomId { get; set; }
        [DataMember]
        public String Description { get; set; }
        [DataMember]
        public String Key { get; set; }
        [DataMember]
        public DeviceType DeviceType { get; set; }
        [DataMember]
        public String DeviceTypeId { get; set; }
        [DataMember]
        public String PNode { get; set; }
        [DataMember]
        public String Value { get; set; }
        [DataMember]
        public String UOM { get; set; }

        [DataMember]
        public String Parent { get; set; }

        [DataMember]
        public String Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(() => Status);
                    OnPropertyChanged(() => IconImage);
                    OnPropertyChanged(() => IsDeviceOff);
                    OnPropertyChanged(() => IsDeviceOn);
                    OnPropertyChanged(() => StatusDisplay);
                }
            }
        }


        [IgnoreDataMember]
        public String IconImage
        {
            get
            {
                if (Status.ToLower() == "updating")
                    return "ms-appx:///LagoVista.Common.UI.ISY/Images/Transmitting.png";

                switch (DeviceType.DeviceCategory)
                {
                    case DeviceType.DeviceCateogry.Thermostat: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/Temperature.png";
                    case DeviceType.DeviceCateogry.OutdoorModule: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/OutdoorSwitch.png";
                    case DeviceType.DeviceCateogry.MotionDetector: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/MotionDetector.png";
                    case DeviceType.DeviceCateogry.InlineSwitch: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/Outlet.png";
                    case DeviceType.DeviceCateogry.DimmableModule: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/Lamp.png";
                    case DeviceType.DeviceCateogry.LEDLightBulb: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/LightBulb.png";
                    case DeviceType.DeviceCateogry.LoadController: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/LoadController.png";
                    case DeviceType.DeviceCateogry.MultiSwitch: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/MultiSwitch.png";
                    case DeviceType.DeviceCateogry.FanLight: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/FanLight.png";
                    case DeviceType.DeviceCateogry.FanMotor: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/Fan.png";
                    case DeviceType.DeviceCateogry.ContactSwitch: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/ContactSwitch.png";
                    case DeviceType.DeviceCateogry.ToggleSwitch: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/ToggleSwitch.png";
                    case DeviceType.DeviceCateogry.Unknown: return "ms-appx:///LagoVista.ISY994i.UI.UWP/Images/QuestionMark.png";
                }

                return "ms-appx:///LagoVista.Common.UI.ISY/Images/QuestionMark.png";
            }
        }


        public bool IsDimmable
        {
            get { return UOM == "%/on/off"; }
        }

        public bool IsFan
        {
            get { return UOM == "off/low/med/high"; }

        }

        public bool IsThermostat
        {
            get { return DeviceTypeId == "5.10.13.0"; }
        }

        public bool IsStandardOnOff
        {
            get
            {
                return UOM == "on/off"
              && (DeviceTypeId != "7.0.65.0" || Address.EndsWith("2"))
              && DeviceTypeId != "16.2.64.0";
            }
        }

        public String StatusDisplay
        {
            get
            {
                if (Status.ToLower() == "updating")
                    return ISYStringResources.ISY_Common_Updating;

                String statusValue = "?";

                switch (DeviceType.DeviceCategory)
                {
                    case DeviceType.DeviceCateogry.Thermostat:
                        Double temp;
                        if (Double.TryParse(Status, out temp))
                            statusValue = String.Format("{0:0.0}° F", temp);
                        else
                            statusValue = String.Format("{0}", Status);
                        break;
                    case DeviceType.DeviceCateogry.FanMotor:
                        switch (Status)
                        {
                            case "0": statusValue = ISYStringResources.ISY_Common_Off; break;
                            case "63": statusValue = ISYStringResources.ISY_Fan_Low; break;
                            case "191": statusValue = ISYStringResources.ISY_Fan_Medium; break;
                            case "255": statusValue = ISYStringResources.ISY_Fan_High; break;
                        }
                        break;
                    case DeviceType.DeviceCateogry.FanLight:
                    case DeviceType.DeviceCateogry.LEDLightBulb:
                    case DeviceType.DeviceCateogry.DimmableModule:
                        Double percentOn;
                        if (Double.TryParse(Status, out percentOn))
                        {
                            if (percentOn == 0)
                                statusValue = ISYStringResources.ISY_Common_Off;
                            else if (percentOn == 255)
                                statusValue = ISYStringResources.ISY_Common_On;
                            else
                                statusValue = String.Format("{0:0.}%", ((percentOn / 255.0) * 100.0));
                        }
                        else
                            statusValue = Status;

                        break;
                    case DeviceType.DeviceCateogry.ContactSwitch:
                        if (Status == "0")
                            statusValue = ISYStringResources.ISY_Common_Closed;
                        else if (Status == "255")
                            statusValue = ISYStringResources.ISY_Common_Open;

                        break;
                    default:
                        if (Status == "0")
                            statusValue = ISYStringResources.ISY_Common_Off;
                        else if (Status == "255")
                            statusValue = ISYStringResources.ISY_Common_On;
                        else
                            statusValue = Status;
                        break;
                }

                return String.Format("{0}: {1}", ISYStringResources.ISY_Common_Status, statusValue);
            }
        }

        public bool IsDeviceOn
        {
            get
            {
                int statusValue;
                if (Int32.TryParse(Status, out statusValue))
                {
                    return statusValue > 0;
                }
                else
                {
                    if (Status.ToLower() == "on" || Status == "low" || Status == "med" || Status == "high" || Status == "255")
                        return true;
                    else
                        return false;
                }
            }
        }


        public bool IsDeviceOff
        {
            get { return !IsDeviceOn; }
        }

        public static Dictionary<String, DeviceType> DeviceTypeTable = new Dictionary<string, DeviceType>(){
           {"16.2.52.0", new DeviceType() { DeviceName="Contact Switch - Closed", DeviceCategory=Models.DeviceType.DeviceCateogry.ContactSwitch, IsControllable =false, IsSensor = true } },
           {"16.2.67.0", new DeviceType () {DeviceName="Contact Switch - Closed", DeviceCategory=Models.DeviceType.DeviceCateogry.ContactSwitch, IsControllable =false, IsSensor = true} },
           {"0.18.55.0", new DeviceType() { DeviceName= "8 Way Mini Remote", DeviceCategory=Models.DeviceType.DeviceCateogry.MultiSwitch, IsControllable = true, IsSensor = false} },
           {"0.26.56.0", new DeviceType() { DeviceName= "8 Way Mini Remote", DeviceCategory=Models.DeviceType.DeviceCateogry.MultiSwitch, IsControllable = true, IsSensor = false} },
           {"2.8.66.0",new DeviceType() {DeviceName="Oulet Module", DeviceCategory=Models.DeviceType.DeviceCateogry.InlineSwitch, IsControllable = true, IsSensor = false} },
           {"1.14.65.0",new DeviceType() {DeviceName="Inline Dimmer", DeviceCategory=Models.DeviceType.DeviceCateogry.DimmableModule, IsControllable = true, IsSensor = false} },
           {"1.50.65.0",new DeviceType() {DeviceName="Inline Dimmer", DeviceCategory=Models.DeviceType.DeviceCateogry.DimmableModule, IsControllable = true, IsSensor = false} },
           {"1.32.65.0",new DeviceType() {DeviceName="Dimmer Switch", DeviceCategory=Models.DeviceType.DeviceCateogry.DimmableModule, IsControllable = true, IsSensor = false} },
           {"1.33.64.0",new DeviceType() {DeviceName="Dimmer Outlet", DeviceCategory=Models.DeviceType.DeviceCateogry.DimmableModule, IsControllable = true, IsSensor = false} },
           {"1.58.66.0",new DeviceType() {DeviceName="LED Light Bulb", DeviceCategory=Models.DeviceType.DeviceCateogry.LEDLightBulb, IsControllable = true, IsSensor = false} },
           {"1.28.65.0",new DeviceType() {DeviceName="8 Switch Keypad", DeviceCategory=Models.DeviceType.DeviceCateogry.MultiSwitch, IsControllable = true, IsSensor = false} },
           {"2.44.65.0",new DeviceType() {DeviceName="6 Switch Keypad",DeviceCategory=Models.DeviceType.DeviceCateogry.MultiSwitch, IsControllable = true, IsSensor = false} },
           {"2.9.66.0",new DeviceType() {DeviceName="Switch", DeviceCategory=Models.DeviceType.DeviceCateogry.LightSwitch, IsControllable = true, IsSensor = false} },
           {"2.26.65.0",new DeviceType() {DeviceName="Appliance Linc", DeviceCategory=Models.DeviceType.DeviceCateogry.InlineSwitch, IsControllable = true, IsSensor = false} },
           {"5.10.13.0",new DeviceType() {DeviceName="Thermostat", DeviceCategory=Models.DeviceType.DeviceCateogry.Thermostat, IsControllable = false, IsSensor = true} },
           {"9.10.65.0",new DeviceType() {DeviceName="Load Controller", DeviceCategory=Models.DeviceType.DeviceCateogry.LoadController, IsControllable = true, IsSensor = false} },
           {"16.2.64.0",new DeviceType() {DeviceName="On/Off Sensor", DeviceCategory=Models.DeviceType.DeviceCateogry.ContactSwitch, IsControllable = false, IsSensor = true} },
           {"16.1.65.0", new DeviceType() { DeviceName="Motion Detector", DeviceCategory=Models.DeviceType.DeviceCateogry.MotionDetector, IsControllable =false, IsSensor = true} },

           {"2.31.67.0",new DeviceType() {  DeviceName = "Inline Linc", DeviceCategory=Models.DeviceType.DeviceCateogry.InlineSwitch, IsControllable = true, IsSensor = false } },
           {"2.56.67.0", new DeviceType() {DeviceName="On/Off Outdoor Module" , DeviceCategory=Models.DeviceType.DeviceCateogry.OutdoorModule, IsControllable = true, IsSensor = false} },

           {"7.0.65.0.switch",new DeviceType() {DeviceName="I/O Switch", DeviceCategory=Models.DeviceType.DeviceCateogry.ToggleSwitch, IsControllable = true, IsSensor = false} },
           {"7.0.65.0.sensor",new DeviceType() {DeviceName="I/O Sensor", DeviceCategory=Models.DeviceType.DeviceCateogry.ContactSwitch, IsControllable = false, IsSensor = true} },
           {"1.46.65.0.light",new DeviceType() {DeviceName="Fan Light", DeviceCategory=Models.DeviceType.DeviceCateogry.FanLight, IsControllable = true, IsSensor = false} },
           {"1.46.65.0.motor",new DeviceType() { DeviceName="Fan Motor", DeviceCategory=Models.DeviceType.DeviceCateogry.FanMotor, IsControllable = true, IsSensor = false} },
        };

        private bool? _isTransmitting;
        public bool IsTransmitting
        {
            get { return _isTransmitting.HasValue ? _isTransmitting.Value : false; }
            set
            {
                if (!_isTransmitting.HasValue || _isTransmitting.Value != _isTransmitting)
                {
                    _isTransmitting = value;
                    OnPropertyChanged(() => IsTransmitting);
                }
            }
        }

        public async void SendCommand(String command, Object parameter = null)
        {
            IsTransmitting = true;

            Status = ISYStringResources.ISY_Common_Updating;

            if (parameter != null)
                await ISYService.Instance.SendCommandAsync(RestAddress, command, Convert.ToInt32(parameter));
            else
                await ISYService.Instance.SendCommand(RestAddress, command);

            /* SHould happen via WebSockets */
            /*await Task.Delay(1500);
            if (Status == Resources.ISYStringResources.ISY_Common_Updating)
            {
                var result = await ISYService.Instance.RefreshNodeStatusAsync(RestAddress);

                Status = result.Value;

                Value = result.Value;
                UOM = result.UOM;
            }*/

            IsTransmitting = false;
        }

        public void PinDevice()
        {

        }

        [IgnoreDataMember]
        public RelayCommand PinDeviceCommand { get { return new RelayCommand(() => PinDevice()); } }

        [IgnoreDataMember]
        public RelayCommand DeviceOffCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceOff)); } }
        [IgnoreDataMember]
        public RelayCommand DeviceOnCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceOn)); } }
        [IgnoreDataMember]
        public RelayCommand DeviceOnLowCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceOn, "33")); } }
        [IgnoreDataMember]
        public RelayCommand DeviceOnMediumCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceOn, "66")); } }
        [IgnoreDataMember]
        public RelayCommand DeviceOnHighCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceOn, "100")); } }
        [IgnoreDataMember]
        public RelayCommand DeviceFastOffCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceFastOff)); } }
        [IgnoreDataMember]
        public RelayCommand DeviceFastOnCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.DeviceFastOn)); } }
        [IgnoreDataMember]
        public RelayCommand DeviceBrightCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.Bright)); } }
        [IgnoreDataMember]
        public RelayCommand DeviceDimCommand { get { return new RelayCommand(() => SendCommand(Services.ISYService.Dim)); } }
    }

    public class DeviceType
    {
        public enum DeviceCateogry
        {
            LightSwitch = 0, 
            MultiSwitch = 1,
            DimmableModule = 2,
            InlineSwitch = 3,
            FanMotor = 4,
            LEDLightBulb = 5,
            LoadController = 6, 
            FanLight = 7,
            Thermostat = 8,
            OutdoorModule = 9,
            MotionDetector = 10,
            ContactSwitch = 11, 
            ToggleSwitch = 12, 
            Unknown,
        }

        public String DeviceName { get; set; }
        public DeviceCateogry DeviceCategory { get; set; }
        public String DeviceImage { get; set; }
        public bool IsControllable { get; set; }
        public bool IsSensor { get; set; }
    }
}
