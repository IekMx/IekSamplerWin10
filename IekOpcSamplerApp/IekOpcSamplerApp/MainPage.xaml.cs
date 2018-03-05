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
        ObservableCollection<KeyValuePair<int, double>> kvList = new ObservableCollection<KeyValuePair<int, double>>();
        ObservableCollection<KeyValuePair<int, double>> kvListH = new ObservableCollection<KeyValuePair<int, double>>();
        DispatcherTimer timer = new DispatcherTimer();
        int x = 0;
        Line line;
        #endregion

        #region "Services"
        Services.OpcSocketClient _OpcClient = new Services.OpcSocketClient();
        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            LineChart.DataContext = kvList;
            LineChartH.DataContext = kvListH;

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (object sender, object e) =>
            {
                if (kvList.Count == 100)
                {
                    kvListH.Add(kvList[0]);
                    kvList.RemoveAt(0);
                }
                kvList.Add(new KeyValuePair<int, double>(x++, Math.Sin(x)));
                if (Grid1.Children.Contains(line))
                {
                    Grid1.Children.Remove(line);
                }

                var axisy = LineChart.ActualAxes[1] as LinearAxis;
                var linearaximaximum = Convert.ToDouble(axisy.ActualMaximum);
                var linearaximinimum = Convert.ToDouble(axisy.ActualMinimum);
                double perinterval = Convert.ToDouble(axisy.ActualHeight / (linearaximaximum - linearaximinimum));

                var lineY = perinterval * (0.8 - linearaximinimum);

                var ttv = lineSeries.TransformToVisual(Window.Current.Content);
                Point screenCoords = ttv.TransformPoint(new Point(0, 0));

                line = new Line();
                line.X1 = screenCoords.X;
                line.X2 = LineChart.ActualWidth;
                line.Y1 = axisy.ActualHeight - lineY;
                line.Y2 = axisy.ActualHeight - lineY;
                line.Stroke = new SolidColorBrush(Colors.Gray);
                line.StrokeThickness = 1;
                Grid1.Children.Add(line);
            };

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
                });
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            //    async () =>
            //    {
            //        try
            //        {
            //            await _OpcClient.SendAsync(
            //                JsonConvert.SerializeObject(new
            //                {
            //                    handle = 1,
            //                    name = "B_BERTHA",
            //                    value = 1
            //                }));
            //            await Task.Delay(500);
            //            await _OpcClient.SendAsync(
            //                JsonConvert.SerializeObject(new
            //                {
            //                    handle = 1,
            //                    name = "B_BERTHA",
            //                    value = 0
            //                }));
            //        }
            //        catch (Exception ex)
            //        {
            //            throw;
            //        }
            //    });

        }
        #endregion

        private void limSupUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void limSupNum_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void limSupDown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void limInfUp_Click(object sender, RoutedEventArgs e)
        {

        }

        private void limInfNum_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void limInfDown_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
