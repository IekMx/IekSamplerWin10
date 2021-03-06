﻿using Newtonsoft.Json;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
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
        double LimSup = 19.5;
        double LimInf = 17.5;
        double LimErr = 20;
        int cycleCount = 0;
        double _gaugeXlValue = 0;
        int _Delay = 5000;

        public static MainPage Current;

        ObservableCollection<Models.Point> MainLineCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> UpperBoundCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> LowerBoundCollection = new ObservableCollection<Models.Point>();
        ObservableCollection<Models.Point> MainLineCollectionH = new ObservableCollection<Models.Point>();

        List<List<Models.Point>> CountingCollection = new List<List<Models.Point>>();

        DispatcherTimer timer = new DispatcherTimer();
        int x = 0;
        #endregion

        #region "Services"
        Services.OpcSocketServer _OpcClient = new Services.OpcSocketServer();
        Services.DatabaseService _DBClient = new Services.DatabaseService();
        Services.OpcProcessor _OpcProcessor = new Services.OpcProcessor();

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
            _OpcProcessor.DelayUpdated += _OpcProcessor_DelayUpdated;

            StartServices();

            limSupNum.Text = LimSup.ToString();
            limInfNum.Text = LimInf.ToString();
            labelLimites.Text = "[" + LimSup + "," + LimInf + "]";

            //timer.Start();

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

        private void _OpcProcessor_DelayUpdated(Models.Tag tag)
        {
            int.TryParse(tag.Value, out _Delay);
            _Delay = _Delay == 0 ? 5000 : _Delay * 1000;
        }

        private void _OpcProcessor_CountChanged(double value)
        {
            _gaugeXlValue = value;
        }

        private async void _OpcProcessor_TagValidated(Models.Tag tag)
        {
            if (tag != null)
            {

                var pos = MainLineCollection.FirstOrDefault(x => x.X == tag.Handle);
                var errp = MainLineCollection.Count(x => x.Y > LimSup || x.Y < LimInf);

                if (pos != null)
                {
                    await Task.Delay(_Delay).ContinueWith(async (h) =>
                     {
                         pos.Y = _gaugeXlValue;
                         await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                         () =>
                         {
                             MainLineSeries.Refresh();
                             labelActual.Text = _gaugeXlValue.ToString("00.00");
                             bool error = errp > LimErr / 100 * MainLineCollection.Count;
                             WarningBanner.Background = error ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)) : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                             var avg = MainLineCollection.Average(x => x.Y);
                             labelPromedio.Text = avg.ToString("00.00");
                             labelPaso.Text = "Pos. " + (MainLineCollection.IndexOf(pos) + 1);
                         });
                     });
                }
            }
        }

        private async void _OpcProcessor_LapCompleted(bool fromHome)
        {
            cycleCount++;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                CycleCount.Text = cycleCount.ToString();
            });

            if (cycleCount == 1) return;

            await Task.Delay(_Delay + 100).ContinueWith(async (h) =>
            {
                var list = new List<Models.Point>();
                MainLineCollection.ToList().ForEach(x =>
                {
                    list.Add(new Models.Point(x.X, x.Y));
                });
                CountingCollection.Add(list);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    CycleCount.Text = cycleCount.ToString();
                    int i = MainLineCollectionH.Count > 0 ? MainLineCollectionH.Max(x => x.X) : 0;
                    MainLineCollection.ToList().ForEach(x =>
                    {
                        var p = new Models.Point(i++, x.Y);
                        MainLineCollectionH.Add(p);
                    });
                    var p0 = new Models.Point(i++, 0);
                    MainLineCollectionH.Add(p0);
                });
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
                    CountingCollection.Clear();
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

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

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

        private void margenErrNum_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
            {
                return;
            }
            double.TryParse(margenErrNum.Text, out LimErr);
            labelMargenError.Text = "[" + LimErr + "%]";

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
            MainLineCollectionH.Clear();
            CountingCollection.Clear();
            cycleCount = 0;
            CycleCount.Text = "-";
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

        private async void ImprimirButton_Click(object sender, RoutedEventArgs e)
        {
            if (CountingCollection.Count <= 0)
            {
                var dialog = new MessageDialog("No se ha completado al menos un ciclo");
                await dialog.ShowAsync();
                return;
            }
            List<Models.Point> avgs = new List<Models.Point>();
            for (int i = 0; i < CountingCollection.First().Count; i++)
            {
                var p = new Models.Point(CountingCollection.First()[i].X, 0);
                avgs.Add(p);
            }

            var csvString = new System.Text.StringBuilder();
            CountingCollection.ForEach(y =>
            {
                var line = "";
                y.ForEach(x =>
                {
                    line += x.Y + ",";
                    avgs.First(z => z.X == x.X).Y += x.Y;
                });
                csvString.AppendLine(line);
            });
            avgs.ForEach(y =>
            {
                y.Y = y.Y / CountingCollection.Count;
            });
            avgs.ForEach(y =>
            {
                MainLineCollection.ToList().First(x => x.X == y.X).Y = y.Y;
            });
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    MainLineSeries.Refresh();
                });
            LineChart.Width = LineChart.ActualWidth;
            LineChart.Height = LineChart.ActualHeight;
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(LineChart, (int)LineChart.Width, (int)LineChart.Height);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            var displayInformation = DisplayInformation.GetForCurrentView();
            var guid = Guid.NewGuid();
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(guid + ".png", CreationCollisionOption.ReplaceExisting);
            var csvfile = await ApplicationData.Current.LocalFolder.CreateFileAsync(guid + ".csv", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                                     BitmapAlphaMode.Premultiplied,
                                     (uint)LineChart.ActualWidth,
                                     (uint)LineChart.ActualHeight,
                                     displayInformation.RawDpiX,
                                     displayInformation.RawDpiY,
                                     pixelBuffer.ToArray());
                await encoder.FlushAsync();
            }
            using (var stream = await csvfile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await stream.AsStreamForWrite().WriteAsync(System.Text.Encoding.ASCII.GetBytes(csvString.ToString()), 0, csvString.Length);
            }
            var page = new PageToPrint();
            var bmp = new BitmapImage(new Uri("ms-appdata:///local/" + guid + ".png"));
            page.ScenarioImage.Source = bmp;
            page.OrdenTextBlock.Text = labelOrderCustName.Text.ToUpper();
            page.SkuTextBlock.Text = labelOrderSku.Text;
            page.OperadorTextBlock.Text = labelOrderOperador.Text.ToUpper();
            page.TurnoTextBlock.Text = labelOrderTurno.Text;
            page.LongitudTextBlock.Text = labelOrderLongitud.Text;
            page.AdhesivoTextBlock.Text = labelOrderAdhesivo.Text;
            page.AreaTextBlock.Text = labelOrderArea.Text;
            var observaciones = new System.Text.StringBuilder();
            var ix = 0;
            if (!string.IsNullOrEmpty(labelOrderObservaciones.Text))
            {
                while (true)
                {
                    var line = "";
                    try
                    {
                        line = labelOrderObservaciones.Text.Substring(ix, 60);
                        ix += 60;
                        observaciones.AppendLine(line);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        line = labelOrderObservaciones.Text.Substring(ix);
                        observaciones.AppendLine(line);
                        break;
                    }
                }
            }

            page.ObservacionesTextBlock.Text = observaciones.ToString();
            page.DensPromTextBlock.Text = avgs.Average(x => x.Y).ToString("#0.00");

            var printHelper = new Services.PrintHelper(this);
            printHelper.StatusChanged += async (string message, int type) =>
            {
                printHelper.UnregisterForPrinting();
                printHelper = null;
                page.ScenarioImage.Source = null;
                try
                {
                    await file.DeleteAsync();
                }
                catch (Exception)
                {
                }
                ResetButton_Click(sender, e);
            };
            printHelper.RegisterForPrinting();
            printHelper.PreparePrintContent(page);
            await printHelper.ShowPrintUIAsync();

        }

    }
}
