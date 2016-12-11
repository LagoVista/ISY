using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ISY.Models
{
    public class Room
    {
        public int Id { get; set; }
        public String RoomName { get; set; }
        public int ISYFolderId { get; set; }
        public String Description { get; set; }
        public String Key { get; set; }
    }
}
