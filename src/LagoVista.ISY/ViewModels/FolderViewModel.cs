using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.ISY.ViewModels
{
    public class FolderViewModel : ViewModelBase
    {

        public async override Task InitAsync()
        {
            await Task.Delay(1);

            Folder = NavigationParameter as Models.Folder;
        }

        Models.Folder _folder;
        public Models.Folder Folder
        {
            get { return _folder; }
            set { Set(ref _folder, value); }
        }
    }
}
