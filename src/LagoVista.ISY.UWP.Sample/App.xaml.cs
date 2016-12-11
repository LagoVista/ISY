using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Newtonsoft.Json;
using LagoVista.ISYAutomation.Services;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using LagoVista.UWP.UI;
using LagoVista.Core.UWP.Services;
using LagoVista.ISY.ViewModels;
using LagoVista.Core.UWP.ViewModels.Common;
using LagoVista.ISY.Services;
using LagoVista.ISY.UI.UWP.Services;
using LagoVista.ISY.Models;

namespace LagoVista.ISY.UWP.Sample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        SSDPClient _restClient;
        DateTime _startTime;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            SLWIOC.Register<IViewModelNavigation>(Navigation.Instance);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            _startTime = DateTime.Now;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                Navigation.Instance.Initialize(rootFrame);
                Window.Current.Content = rootFrame;

                UWPDeviceServices.Init(Window.Current.Dispatcher);

               // await SmartThingsHubs.Instance.InitAsync();

                UnhandledException += App_UnhandledException;

                Navigation.Instance.Add<FolderViewModel, Views.FolderView>();
                Navigation.Instance.Add<DeviceDiscoveryViewModel, DeviceDiscoveryView>();

                _restClient = new SSDPClient();
                if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.IoT")
                {
                    _restClient.MakeDiscoverable(9301, new SSDPDiscoveryConfiguration()
                    {
                        DeviceType = "X_LagoVista_ISY_Kiosk_Device",
                        FriendlyName = "ISY Remote Kiosk",
                        Manufacture = "Software Logistics, LLC",
                        ManufactureUrl = "www.TheWolfBytes.com",
                        ModelDescription = "ISY Remote UI and SmartThings Bridge",
                        ModelName = "ISYRemoteKiosk",
                        ModelNumber = "SL001",
                        ModelUrl = "www.TheWolfBytes.com",
                        SerialNumber = "KSK001"
                    });
                }

                ISYService.Instance.Connected += Instance_Connected;
                ISYService.Instance.Disconnected += Instance_Disconnected;
                ISYEventListener.Instance.ISYEventReceived += Instance_ISYEventReceived;

                await ISYService.Instance.InitAsync();
                await ISYService.Instance.RefreshAsync();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window

            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private async void Instance_ISYEventReceived(object sender, ISYEvent evt)
        {
            if ((DateTime.Now - _startTime).TotalSeconds > 30)
            {
                var json = JsonConvert.SerializeObject(evt);
        //        await SmartThingsHubs.Instance.SendToHubsAsync(json);
            }
            else
                Debug.WriteLine("START UP MESSAGE" + evt.Device.Address);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs ex)
        {
            Core.PlatformSupport.Services.Logger.LogException("Unhandled Exception", ex.Exception);
        }

        private void Instance_Disconnected(object sender, EventArgs e)
        {
            ISYEventListener.Instance.Disconnect();
        }

        private async void Instance_Connected(object sender, EventArgs e)
        {
            await ISYEventListener.Instance.StartListening(ISYService.Instance, ISYService.Instance.ConnectionSettings);
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            var kind = args.Kind;
            if (args.Kind == ActivationKind.Launch)
            {

            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
