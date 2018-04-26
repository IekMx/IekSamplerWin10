using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Controls.DataVisualization;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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
        int LimSup = 0;
        int LimInf = 0;
        ObservableCollection<KeyValuePair<int, double>> MainLineCollection = new ObservableCollection<KeyValuePair<int, double>>();
        ObservableCollection<KeyValuePair<int, double>> UpperLimitCollection = new ObservableCollection<KeyValuePair<int, double>>();
        ObservableCollection<KeyValuePair<int, double>> LowerLimitCollection = new ObservableCollection<KeyValuePair<int, double>>();
        ObservableCollection<KeyValuePair<int, double>> MainLineCollectionH = new ObservableCollection<KeyValuePair<int, double>>();
        DispatcherTimer timer = new DispatcherTimer();
        int x = 0;
        Line line;
        #endregion

        #region "Services"
        Services.OpcSocketServer _OpcClient = new Services.OpcSocketServer();
        Services.DatabaseService _DBClient = new Services.DatabaseService();
        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            MainLineSeries.DataContext = MainLineCollection;
            UpperLimitSeries.DataContext = UpperLimitCollection;
            LowerLimitSeries.DataContext = LowerLimitCollection;
            LineChartH.DataContext = MainLineCollectionH;

            sampleSize.Add("8");
            sampleSize.Add("16");
            sampleSize.Add("32");
            sampleSize.Add("Continuo");

            _OpcClient.ConnectionStatusChanged += _OpcClient_ConnectionStatusChanged;
            _OpcClient.TagValueChanged += _OpcClient_TagValueChanged;

            StartServices();


            timer.Start();

        }

        ~MainPage()
        {
        }

        private async void StartServices()
        {
            OpcStatus.Text = "CONNECTING";
            try
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
                await _OpcClient.ConnectAsync();
                await _OpcClient.SendAsync("HOLA");
            }
            catch (Exception)
            {
            }
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
                    if (MainLineCollection.Count == 100)
                    {
                        MainLineCollectionH.Add(MainLineCollection[0]);
                        MainLineCollection.RemoveAt(0);
                        UpperLimitCollection.RemoveAt(0);
                        LowerLimitCollection.RemoveAt(0);
                    }
                    MainLineCollection.Add(new KeyValuePair<int, double>(x, double.Parse(tag.Value)));
                    UpperLimitCollection.Add(new KeyValuePair<int, double>(x, LimSup));
                    LowerLimitCollection.Add(new KeyValuePair<int, double>(x, LimInf));
                    x++;
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

        private void limitButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(limSupNum.Text, out LimSup);
            int.TryParse(limInfNum.Text, out LimInf);

            switch (sender.GetType().GetProperties().Where(x => x.Name == "Name").Select(x => x.GetValue(sender)).FirstOrDefault())
            {
                case "limSupUp":
                    LimSup++;
                    break;
                case "limSupDown":
                    LimSup--;
                    break;
                case "limInfUp":
                    LimInf++;
                    break;
                case "limInfDown":
                    LimInf--;
                    break;
                default:
                    break;
            }
            limSupNum.Text = LimSup.ToString();
            limInfNum.Text = LimInf.ToString();
            for (var i = 0; i < UpperLimitCollection.Count; i++)
            {
                UpperLimitCollection[i] = new KeyValuePair<int, double>(UpperLimitCollection[i].Key, LimSup);
            }
            for (var i = 0; i < LowerLimitCollection.Count; i++)
            {
                LowerLimitCollection[i] = new KeyValuePair<int, double>(LowerLimitCollection[i].Key, LimInf);
            }
        }

    }
}
