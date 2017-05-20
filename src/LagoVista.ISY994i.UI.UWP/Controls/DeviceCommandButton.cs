using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LagoVista.ISY994i.UI.UWP.Controls
{
    public class DeviceCommandButton : Button
    {
        public String CommandName { get; set; }
        public String Level { get; set; }
    }
}
