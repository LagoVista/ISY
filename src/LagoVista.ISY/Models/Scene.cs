using LagoVista.ISY;
using LagoVista.ISY.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ISY.Models
{
    [DataContract]
    public class Scene
    {
        public Scene()
        {
            Devices = new ObservableCollection<Device>();
        }

        [DataMember] public string Address { get; set; }
        [DataMember] public String Name { get; set; }
        [DataMember] public List<String> MemberAddresses { get; set; }
        [DataMember] public String Parent { get; set; }
        [IgnoreDataMember] public ObservableCollection<Device> Devices { get; private set; }

        [IgnoreDataMember]
        public String RestAddress
        {
            get
            {
                return Address; 
            }
        }

        async  void SendCommand(String command, Object parameter)
        {
            if (parameter != null)
                await Services.ISYService.Instance.SendCommandAsync(RestAddress, command, Convert.ToInt32(parameter));
            else
                await Services.ISYService.Instance.SendCommand(RestAddress, command);

            await Task.Delay(3000);
        }

        ISYDeviceCommand _deviceOn;
        [IgnoreDataMember]
        public ISYDeviceCommand DeviceOn
        {
            get
            {
                if (_deviceOn == null)
                    _deviceOn = new ISYDeviceCommand((param) => SendCommand(Services.ISYService.DeviceOn, param));

                return _deviceOn;
            }
        }

        ISYDeviceCommand _deviceOff;
        [IgnoreDataMember]
        public ISYDeviceCommand DeviceOff
        {
            get
            {
                if (_deviceOff == null)
                    _deviceOff = new ISYDeviceCommand((param) => SendCommand(Services.ISYService.DeviceOff, param));

                return _deviceOff;
            }
        }
    }
}
