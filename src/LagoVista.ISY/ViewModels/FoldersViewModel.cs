using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System.Collections.ObjectModel;

namespace LagoVista.ISY.ViewModels
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

        public override void Init()
        {
            Folders = ISY.Services.ISYService.Instance.DataContext.Folders;
            Events = ISY.Services.ISYService.Instance.DataContext.Events;
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
                        ShowViewModel<FolderViewModel>(value);
                }
            }
        }

        public async void Login()
        {
            await PerformNetworkOperation(async () =>
            {
                if (await ISY.Services.ISYService.Instance.RefreshAsync())
                {
                    await ISY.Services.ISYService.Instance.SaveCredentialsAsync();
                    IsSettingsOpen = false;
                }
            });
        }

        public ISY.Services.ISYService Service
        {
            get { return ISY.Services.ISYService.Instance; }
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
