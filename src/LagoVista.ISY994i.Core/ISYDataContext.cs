using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ISY994i.Core
{
    [DataContract]
    public class ISYDataContext
    {

        [DataMember] public ObservableCollection<Models.Folder> Folders{ get; private set; }
        [DataMember] public ObservableCollection<Models.Device> Devices{ get; private set; }
        [DataMember] public ObservableCollection<Models.Scene> Scenes { get; private set; }
        [DataMember] public DateTime LastUpdated{ get; set; }

        [IgnoreDataMember] public ObservableCollection<Models.ISYEvent> Events { get; private set; }

        public ISYDataContext()
        {
            Events = new ObservableCollection<Models.ISYEvent>();
            Folders = new ObservableCollection<Models.Folder>();
            Devices = new ObservableCollection<Models.Device>();
            Scenes = new ObservableCollection<Models.Scene>();
        }

        public void PopulateDevices()
        {
            foreach (var folder in Folders)
            {

                var devices = Devices.Where(dvc => dvc.Parent == folder.Id.ToString()).OrderBy(dvc => dvc.Name).ToList();
                folder.Devices.Clear();
                foreach (var device in devices)
                    folder.Devices.Add(device);

                Debug.WriteLine(" Folder {0} - Device Count {1}", folder.Name, folder.Devices.Count);
            }
        }

        public void PopulateScenes()
        {
            foreach (var folder in Folders)
            {
                folder.Scenes.Clear();
                var scenes = Scenes.Where(scn => scn.Parent == folder.Id.ToString()).OrderBy(scn => scn.Name).ToList();
                foreach (var scene in scenes)
                    folder.Scenes.Add(scene);
            }

            foreach (var scene in Scenes)
            {
                scene.Devices.Clear();
                foreach (var addr in scene.MemberAddresses)
                    scene.Devices.Add(Devices.Where(dvc => dvc.Address == addr).FirstOrDefault());
                
            }
        }

        public static ISYDataContext LoadISYData(Stream inputStream)
        {
            var ser = new DataContractJsonSerializer(typeof(ISYDataContext));
            ISYDataContext ctx = ser.ReadObject(inputStream) as ISYDataContext;
            ctx.Events = new ObservableCollection<Models.ISYEvent>();
            ctx.PopulateDevices();
            ctx.PopulateScenes();

            return ctx;
        }

        public void SaveISYData(Stream outputStream)
        {
            var ser = new DataContractJsonSerializer(typeof(ISYDataContext));
            ser.WriteObject(outputStream, this);            
        }
    }
}
