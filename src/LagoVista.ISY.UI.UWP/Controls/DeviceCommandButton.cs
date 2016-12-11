using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace LagoVista.Common.UI.ISY.Controls
{
    public class DeviceCommandButton : Button
    {
        public String CommandName { get; set; }
        public String Level { get; set; }
    }
}
