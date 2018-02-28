using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IekOpcSamplerApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region "View"
        ObservableCollection<string> sampleSize = new ObservableCollection<string>();
        double ThisStep { get; set; }
        double LastStep { get; set; }
        #endregion

        #region "Services"
        Services.OpcSocketClient _OpcClient = new Services.OpcSocketClient();
        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.

            sampleSize.Add("8");
            sampleSize.Add("16");
            sampleSize.Add("32");
            sampleSize.Add("Continuo");

            _OpcClient.ConnectionStatusChanged += _OpcClient_ConnectionStatusChanged;
            _OpcClient.TagValueChanged += _OpcClient_TagValueChanged;

            StartServices();
        }
        
        ~MainPage()
        {
        }

        private async void StartServices()
        {
            OpcStatus.Text = "CONNECTING";
            try
            {
                await _OpcClient.ConnectAsync();
                await _OpcClient.SendAsync("HOLA");
            }
            catch (Exception)
            {
            }
        }

        private void ReadStep()
        {
            
        }

        #region Events

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void _OpcClient_ConnectionStatusChanged(object sender, Enums.OpcSocketClientStatus status)
        {
            switch (status)
            {
                case Enums.OpcSocketClientStatus.Good:
                    OpcStatus.Text = "ONLINE";
                    break;
                case Enums.OpcSocketClientStatus.Bad:
                    OpcStatus.Text = "FAILED";
                    break;
                default:
                    break;
            }
        }

        private async void _OpcClient_TagValueChanged(object sender, Models.Tag tag)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    TextV1.Text = tag.Value;
                });
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        await _OpcClient.SendAsync(
                            JsonConvert.SerializeObject(new
                            {
                                handle = 1,
                                name = "B_BERTHA",
                                value = 1
                            }));
                        await Task.Delay(500);
                        await _OpcClient.SendAsync(
                            JsonConvert.SerializeObject(new
                            {
                                handle = 1,
                                name = "B_BERTHA",
                                value = 0
                            }));
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                });
        }
        #endregion
    }
}
