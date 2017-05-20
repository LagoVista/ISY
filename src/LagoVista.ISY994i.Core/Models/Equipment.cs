using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ISY994i.Core.Models
{
    [DataContract]
    public class Equipment
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int RoomId { get; set; }
        [DataMember]
        public String Key { get; set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String Description { get; set; }
    }
}
