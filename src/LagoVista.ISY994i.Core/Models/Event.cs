using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace LagoVista.ISY994i.Core.Models
{

    
    public class EventInfo
    {
        [XmlElement(ElementName ="status")]
        public String Status { get; set; }
    }


    [XmlRoot(ElementName = "Event")]
    public class ISYEvent
    {
        [XmlElement(ElementName = "control")]
        public String Control { get; set; }

        [XmlElement(ElementName = "action")]
        public String Action { get; set; }

        [XmlAttribute(AttributeName = "seqnum")]
        public int SequenceNumber { get; set; }

        [XmlAttribute(AttributeName = "sid")]
        public string Sid { get; set; }

        [XmlElement(ElementName = "node")]
        public String Node { get; set; }

        [XmlElement(ElementName = "eventInfo")]
        public EventInfo EventInfo { get; set; }

        public Device Device { get; set; }

        public static ISYEvent Create(string xml)
        {
            try
            {
                if (xml.Contains("SubscriptionResponse"))
                {

                }
                else
                {

                    var xmlDeserialzier = new XmlSerializer(typeof(ISYEvent));
                    var buffer = System.Text.UTF8Encoding.UTF8.GetBytes(xml);
                    using (var ms = new MemoryStream(buffer))
                    {
                        var evt = xmlDeserialzier.Deserialize(ms) as ISYEvent;
                        evt._raw = xml;
                        return evt;
                    }

                    

                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        private String _raw;
        [JsonIgnore]
        public String Raw { get { return _raw; } }

        public enum ControlTypes
        {
            Uknown,
            DeviceStatus,
            HeartBeat,
            Trigger,
            DeviceSpecific,
            NodeChanged,
            SystemConfigUpdated,
            SystemStatusUpdated,
            InternetAccess,
            ProgressReport,
            SecuritySystemEvent
        }

        public enum ActionTypes
        {
            Unknown,



            EventStatus,
            GetStatus,
            KeyChanged,
            InfoString,
            IRLearnMode,
            ScheduleStatusChanged,
            VariableStatusChanged,
            VariableInitialized,
            Key,

            NodeRenamed,
            NodeRemoved,
            NodeAdded,
            NodeMoved,
            LinkChanged,
            RemovedFromGroup,
            Enabled,
            ParentChanged,
            PowerInfoChanged,
            DeviceIDChanged,
            DevicePropertyChanged,
            GroupRenamed,
            GroupRemvoved,
            GroupAdded,
            FolderRenamed,
            FolderRemoved,
            FolderAdded,
            NodeError,
            ErrorsCleared,
            DiscoveringNodes,
            NodeDiscoveryComplete,
            NetworkRenamed,
            PendingDeviceOperation,
            ProgrammingDevice,
            NodeRevised,

            TimeChanged,
            TImeConfigurationChanged,
            NTPSettingsUpdated,
            NTPCommunicationsError,
            BatchModeUpdated,
            BatterModeProgrammingUpdated,

            NotBusy,
            Busy,
            Idle,
            SafeMode,

            InternatAccessDisabled,
            InternatAccessEnabled,
            InternatAccessFailed,

            ProgressUpdate

        }

        public ControlTypes ControlType
        {
            get
            {
                switch (Control)
                {
                    case "ST":return ControlTypes.DeviceStatus; 
                    case "_0": return ControlTypes.HeartBeat;
                    case "_1": return ControlTypes.Trigger;
                    case "_2": return ControlTypes.DeviceSpecific;
                    case "_3": return ControlTypes.NodeChanged;
                    case "_4": return ControlTypes.SystemConfigUpdated;
                    case "_5": return ControlTypes.SystemStatusUpdated;
                    case "_6": return ControlTypes.InternetAccess;
                    case "_7": return ControlTypes.ProgressReport;
                    case "_8": return ControlTypes.SecuritySystemEvent;
                }

                return ControlTypes.Uknown;
            }
        }

        public ActionTypes ActionType
        {
            get
            {
                switch (ControlType)
                {
                    case ControlTypes.Trigger:
                        switch (Action)
                        {
                         
                            case "0": return ActionTypes.EventStatus;
                            case "1": return ActionTypes.GetStatus;
                            case "2": return ActionTypes.KeyChanged;
                            case "3": return ActionTypes.InfoString;
                            case "4": return ActionTypes.IRLearnMode;
                            case "5": return ActionTypes.ScheduleStatusChanged;
                            case "6": return ActionTypes.VariableStatusChanged;
                            case "7": return ActionTypes.VariableInitialized;
                            case "8": return ActionTypes.Key;
                        }
                        break;
                    case ControlTypes.NodeChanged:
                        switch (Action)
                        {
                            case "NN": return ActionTypes.NodeRenamed;
                            case "NR": return ActionTypes.NodeRemoved;
                            case "ND": return ActionTypes.NodeAdded;
                            case "NM": return ActionTypes.NodeMoved;
                            case "CL": return ActionTypes.LinkChanged;
                            case "RG": return ActionTypes.RemovedFromGroup;
                            case "EN": return ActionTypes.Enabled;
                            case "PI": return ActionTypes.ParentChanged;
                            case "DI": return ActionTypes.PowerInfoChanged;
                            case "DP": return ActionTypes.DeviceIDChanged;
                            case "GN": return ActionTypes.DevicePropertyChanged;
                            case "GR": return ActionTypes.GroupRenamed;
                            case "GD": return ActionTypes.GroupRemvoved;
                            case "FN": return ActionTypes.GroupAdded;
                            case "FR": return ActionTypes.FolderRenamed;
                            case "FD": return ActionTypes.FolderRemoved;
                            case "NE": return ActionTypes.NodeError;
                            case "CE": return ActionTypes.ErrorsCleared;
                            case "SN": return ActionTypes.DiscoveringNodes;
                            case "SC": return ActionTypes.NodeDiscoveryComplete;
                            case "WR": return ActionTypes.NetworkRenamed;
                            case "WH": return ActionTypes.PendingDeviceOperation;
                            case "WD": return ActionTypes.ProgrammingDevice;
                            case "RV": return ActionTypes.NodeRevised;
                        }
                        break;
                    case ControlTypes.SystemConfigUpdated:
                        switch (Action)
                        {
                            case "0": return ActionTypes.TimeChanged;
                            case "1": return ActionTypes.TImeConfigurationChanged;
                            case "2": return ActionTypes.NTPSettingsUpdated;
                            case "3": return ActionTypes.NTPCommunicationsError;
                            case "4": return ActionTypes.BatchModeUpdated;
                            case "5": return ActionTypes.BatterModeProgrammingUpdated;
                        }
                        break;
                    case ControlTypes.InternetAccess:
                        switch (Action)
                        {
                            case "0": return ActionTypes.InternatAccessDisabled;
                            case "1": return ActionTypes.InternatAccessEnabled;
                            case "2": return ActionTypes.InternatAccessFailed;
                        }
                        break;
                    case ControlTypes.ProgressReport:
                        switch (Action)
                        {
                            case "1": return ActionTypes.Busy;
                        }
                        break;


                }

                return ActionTypes.Unknown;
            }
        }
    }
}
