using LagoVista.Core.ViewModels;
using System.Threading.Tasks;

namespace LagoVista.ISY994i.Core.ViewModels
{
    public class FolderViewModel : ViewModelBase
    {

        public override Task InitAsync()
        {
            Folder = NavigationParameter as Models.Folder;
            return Task.FromResult(default(object));
        }

        Models.Folder _folder;
        public Models.Folder Folder
        {
            get { return _folder; }
            set { Set(ref _folder, value); }
        }
    }
}
