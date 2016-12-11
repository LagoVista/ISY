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
    public class Folder
    {
        public Folder()
        {
            Scenes = new ObservableCollection<Scene>();
            Devices = new ObservableCollection<Device>();
        }

        [DataMember]
        public String RoomImage
        {
            get { return "ms-appx:///LagoVista.Common.UI.ISY/Images/Room.png"; }
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public String Name { get; set; }

        [IgnoreDataMember]
        public List<Device> ControlableDevices { get { return Devices.Where(dvc => dvc.DeviceType.IsControllable == true).OrderBy(dvc => dvc.Name).ToList(); } }
        [IgnoreDataMember]
        public List<Device> SensorDevices { get { return Devices.Where(dvc => dvc.DeviceType.IsSensor == true).OrderBy(dvc => dvc.Name).ToList(); } }
        [IgnoreDataMember]
        public ObservableCollection<Scene> Scenes { get; private set; }
        [IgnoreDataMember]
        public ObservableCollection<Device> Devices { get; private set; }
    }
}
