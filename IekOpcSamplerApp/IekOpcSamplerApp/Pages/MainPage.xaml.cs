using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WinRTXamlToolkit.Composition;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IekOpcSamplerApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region "View"
        double ThisStep { get; set; }
        double LastStep { get; set; }
        double LimSup = 5;
        double LimInf = -5;
        int cycleCount = 0;
        double _gaugeXlValue = 0;
        int _retraso = 2000;

        public static MainPage Current;

        ObservableCollection<Models.Point> MainLineCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> UpperBoundCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> LowerBoundCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> MainLineCollectionH = new ObservableCollection<Models.Point>();

        DispatcherTimer timer = new DispatcherTimer();
        int x = 0;
        Line line;
        #endregion

        #region "Services"
        Services.OpcSocketServer _OpcClient = new Services.OpcSocketServer();
        Services.DatabaseService _DBClient = new Services.DatabaseService();
        Services.OpcProcessor _OpcProcessor = new Services.OpcProcessor();
        Services.PrintHelper printHelper;

        #endregion

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            Current = this;
            MainLineSeries.DataContext = MainLineCollection;
            UpperBoundSeries.DataContext = UpperBoundCollection;
            LowerBoundSeries.DataContext = LowerBoundCollection;
            LineChartH.DataContext = MainLineCollectionH;

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (object sender, object e) =>
            {
                if (MainLineCollection.Count == 100)
                {
                    MainLineCollection.Add(MainLineCollection[0]);
                    MainLineCollection.RemoveAt(0);
                }
                x++;
                MainLineCollection.Add(new Models.Point(x, Math.Sin(x)));
                UpperBoundCollection.Add(new Models.Point(x, LimSup));
                LowerBoundCollection.Add(new Models.Point(x, LimInf));

                if (MainLineCollection.Count > 0)
                {
                    labelActual.Text = Math.Sin(x).ToString("00.000");
                    labelPromedio.Text = MainLineCollection.Average(x => x.Y).ToString("00.000");
                }

                //if (Grid1.Children.Contains(line))
                //{
                //    Grid1.Children.Remove(line);
                ////}

                //var axisy = LineChart.ActualAxes[1] as LinearAxis;
                //var linearaximaximum = Convert.ToDouble(axisy.ActualMaximum);
                //var linearaximinimum = Convert.ToDouble(axisy.ActualMinimum);
                //double perinterval = Convert.ToDouble(axisy.ActualHeight / (linearaximaximum - linearaximinimum));

                //var lineY = perinterval * (0.8 - linearaximinimum);

                //var ttv = MainLineSeries.TransformToVisual(Window.Current.Content);
                ////Point screenCoords = ttv.TransformPoint(new Point(0, 0));

                //line = new Line();
                //line.X1 = screenCoords.X;
                //line.X2 = LineChart.ActualWidth;
                //line.Y1 = axisy.ActualHeight - lineY;
                //line.Y2 = axisy.ActualHeight - lineY;
                ////line.Stroke = new SolidColorBrush(Colors.Gray);
                //line.StrokeThickness = 1;
                //Grid1.Children.Add(line);
            };

            //MainLineSeries.IndependentAxis = new LinearAxis
            //{
            //    Margin = new Thickness(10, 0, 0, 10),
            //    Orientation = AxisOrientation.X,
            //    Interval = 50,
            //    AxisLabelStyle = new 
            //    ShowGridLines = false
            //};

            _OpcClient.ConnectionStatusChanged += _OpcClient_ConnectionStatusChanged;
            _OpcClient.TagValueChanged += _OpcClient_TagValueChanged;
            _OpcProcessor.StepsUpdated += _OpcProcessor_HomeCompleted;
            _OpcProcessor.LapCompleted += _OpcProcessor_LapCompleted;
            _OpcProcessor.TagValidated += _OpcProcessor_TagValidated;
            _OpcProcessor.CountChanged += _OpcProcessor_CountChanged;

            StartServices();

            limSupNum.Text = LimSup.ToString();
            limInfNum.Text = LimInf.ToString();
            labelLimites.Text = "[" + LimSup + "," + LimInf + "]";

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

        private void _OpcProcessor_CountChanged(double value)
        {
            _gaugeXlValue = value;
        }

        private async void _OpcProcessor_TagValidated(Models.Tag tag)
        {
            if (tag != null)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    var pos = MainLineCollection.FirstOrDefault(x => x.X == tag.Handle);
                    if (pos != null)
                    {
                        System.Threading.Thread.Sleep(_retraso);
                        pos.Y = _gaugeXlValue;
                        MainLineSeries.Refresh();
                        labelActual.Text = _gaugeXlValue.ToString("000.00");
                        labelPromedio.Text = MainLineCollection.Average(x => x.Y).ToString("000.00");
                    }
                });
            }
        }

        private async void _OpcProcessor_LapCompleted(bool fromHome)
        {
            cycleCount++;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    CycleCount.Text = cycleCount.ToString();
                    if (fromHome)
                    {
                        MainLineCollection.ToList().Where(x => x.X < MainLineCollection.Max(y => y.X)).ToList().ForEach(x =>
                        {
                            MainLineCollectionH.Add(x);
                            x.Y = MainLineCollection.ToList().Average(y => y.Y);
                        });
                    }
                    else
                    {
                        MainLineCollection.ToList().Where(x => x.X > MainLineCollection.Min(y => y.X)).ToList().ForEach(x =>
                        {
                            MainLineCollectionH.Add(x);
                            x.Y = MainLineCollection.ToList().Average(y => y.Y);
                        });
                    }
                    MainLineSeries.Refresh();
                });
        }

        private void _OpcClient_ConnectionStatusChanged(object sender, Enums.OpcSocketClientStatus status)
        {
            switch (status)
            {
                case Enums.OpcSocketClientStatus.Good:
                    OpcStatus.Text = "En Línea";
                    break;
                case Enums.OpcSocketClientStatus.Bad:
                    OpcStatus.Text = "Error";
                    break;
                default:
                    break;
            }
        }

        private async void _OpcProcessor_HomeCompleted(List<KeyValuePair<int, double>> collection)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    labelMuestreo.Text = "[" + _OpcProcessor.Steps + "]";
                    MainLineCollection.Clear();
                    UpperBoundCollection.Clear();
                    LowerBoundCollection.Clear();
                    collection?.ForEach(x =>
                    {
                        if (!MainLineCollection.Any(y => y.X == x.Key))
                        {
                            MainLineCollection.Add(new Models.Point(x.Key, x.Value));
                            UpperBoundCollection.Add(new Models.Point(x.Key, LimSup));
                            LowerBoundCollection.Add(new Models.Point(x.Key, LimInf));
                        }
                        else
                        {
                            var point = MainLineCollection.FirstOrDefault(y => y.X == x.Key);
                            point.Y = x.Value;
                        }
                    });
                });
        }

        private void _OpcClient_TagValueChanged(object sender, Models.Tag tag)
        {
            _OpcProcessor.ProcessTag(tag);
        }

        #endregion


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
                    catch (Exception)
                    {
                        throw;
                    }
                });

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (PrintManager.IsSupported())
            {
                // Tell the user how to print
            }
            else
            {
                // Remove the print button
                PrintButton.Visibility = Visibility.Collapsed;

                // Inform user that Printing is not supported

                // Printing-related event handlers will never be called if printing
                // is not supported, but it's okay to register for them anyway.
            }

            // Initalize common helper class and register for printing
            printHelper = new Services.PrintHelper(this);
            printHelper.RegisterForPrinting();

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (printHelper != null)
            {
                printHelper.UnregisterForPrinting();
            }
        }

        private async void Limit_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            double.TryParse(limSupNum.Text, out LimSup);
            double.TryParse(limInfNum.Text, out LimInf);

            limSupNum.Text = LimSup.ToString();
            limInfNum.Text = LimInf.ToString();

            labelLimites.Text = "[" + LimSup + "," + LimInf + "]";

            UpperBoundCollection.Clear();
            LowerBoundCollection.Clear();

            MainLineCollection.ToList().ForEach(x =>
            {
                UpperBoundCollection.Add(new Models.Point(x.X, LimSup));
                LowerBoundCollection.Add(new Models.Point(x.X, LimInf));
            });

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    UpperBoundSeries.Refresh();
                    LowerBoundSeries.Refresh();
                });
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            switch (PlayButton.Label)
            {
                case "Iniciar":
                    PlayButton.Icon = new SymbolIcon(Symbol.Pause);
                    PlayButton.Label = "Pausar";
                    _OpcProcessor.StepsUpdated -= _OpcProcessor_HomeCompleted;
                    _OpcProcessor.LapCompleted -= _OpcProcessor_LapCompleted;
                    _OpcProcessor.TagValidated -= _OpcProcessor_TagValidated;
                    _OpcProcessor.CountChanged -= _OpcProcessor_CountChanged;
                    break;
                default:
                    PlayButton.Icon = new SymbolIcon(Symbol.Play);
                    PlayButton.Label = "Iniciar";
                    _OpcProcessor.StepsUpdated += _OpcProcessor_HomeCompleted;
                    _OpcProcessor.LapCompleted += _OpcProcessor_LapCompleted;
                    _OpcProcessor.TagValidated += _OpcProcessor_TagValidated;
                    _OpcProcessor.CountChanged += _OpcProcessor_CountChanged;
                    break;
            }
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            MainLineCollection.ToList().ForEach(x =>
            {
                x.Y = 0;
            });

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    MainLineSeries.Refresh();
                });
        }

        private void SaveOrderButton_Click(object sender, RoutedEventArgs e)
        {
            flyoutEdit.Hide();
            labelOrderSku.Text = tbOrderSku.Text;
            labelOrderCustName.Text = tbOrderCliente.Text;
            labelOrderArea.Text = tbOrderArea.Text;
            labelOrderAdhesivo.Text = tbOrderAdhesivo.Text;
            labelOrderLongitud.Text = tbOrderLongitud.Text;
            labelOrderObservaciones.Text = tbOrderObservaciones.Text;
            labelOrderOperador.Text = tbOrderOperador.Text;
            labelOrderTurno.Text = ((ComboBoxItem)tbOrderTurno.SelectedItem)?.Content?.ToString() ?? "";
        }

        private void RetrasoNum_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            int.TryParse(retrasoNum.Text, out _retraso);
            _retraso = _retraso == 0 ? 2000 : _retraso;
        }

        private async void ImprimirButton_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(LineChart);
            printHelper.PreparePrintContent(new PageToPrint
            {
                Orden = labelOrderAdhesivo.Text,
                SKU = labelOrderSku.Text,
                Adhesivo = labelOrderAdhesivo.Text,
                Turno = labelOrderTurno.Text,
                Operador = labelOrderOperador.Text,
                Area = labelOrderArea.Text,
                Longitud = labelOrderLongitud.Text,
                Observaciones = labelOrderObservaciones.Text,
                ChartImage = bitmap
            });

            await printHelper.ShowPrintUIAsync();

            ResetButton_Click(sender, e);

        }
    }
}
