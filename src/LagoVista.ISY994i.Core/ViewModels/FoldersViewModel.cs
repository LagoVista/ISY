using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.ISY994i.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.ISY994i.Core.ViewModels
{
    public class FoldersViewModel : ViewModelBase
    {
        RelayCommand _loginCommand;
        RelayCommand _toggleSettingsPaneCommand;

        public FoldersViewModel()
        {
            _loginCommand = new RelayCommand(() => Login());
            _toggleSettingsPaneCommand = new RelayCommand(() => IsSettingsOpen = !IsSettingsOpen);
        }

        ObservableCollection<Models.Folder> _folders;
        public ObservableCollection<Models.Folder> Folders
        {
            get { return _folders; }
            set { Set(ref _folders, value); }
        }

        public override Task InitAsync()
        {
            Folders = ISYService.Instance.DataContext.Folders;
            Events = ISYService.Instance.DataContext.Events;

            return Task.FromResult(default(object));
        }

        private Models.Folder _selectedFolder;
        public Models.Folder SelectedFolder
        {
            get { return _selectedFolder; }
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    if (value != null)
                    {
                        ViewModelNavigation.NavigateAsync(new ViewModelLaunchArgs() { ViewModelType = typeof(FolderViewModel) });
                    }
                }
            }
        }

        public async void Login()
        {
            await PerformNetworkOperation(async () =>
            {
                if (await ISYService.Instance.RefreshAsync())
                {
                    await ISYService.Instance.SaveCredentialsAsync();
                    IsSettingsOpen = false;
                }
            });
        }

        public ISYService Service
        {
            get { return ISYService.Instance; }
        }

   
        private bool _isSettingsOpen = false;
        public bool IsSettingsOpen
        {
            get { return _isSettingsOpen; }
            set { Set(ref _isSettingsOpen, value); }
        }

        public ObservableCollection<Models.ISYEvent> Events { get; private set; }


        public RelayCommand LoginCommand { get { return _loginCommand; } }

        public RelayCommand ToggleSettingsPaneCommand { get { return _toggleSettingsPaneCommand; } }
    }
}
