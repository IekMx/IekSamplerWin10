using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IekOpcSamplerApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageToPrint : Page
    {
        public ImageSource ChartImage
        {
            get
            {
                return ScenarioImage.Source;
            }
            set
            {
                ScenarioImage.Source = value;
            }
        }

        public string Orden
        {
            get
            {
                return OrdenTextBlock.Text;
            }
            set
            {
                OrdenTextBlock.Text = value;
            }
        }

        public string SKU
        {
            get
            {
                return SkuTextBlock.Text;
            }
            set
            {
                SkuTextBlock.Text = value;
            }
        }

        public string Adhesivo
        {
            get
            {
                return AdhesivoTextBlock.Text;
            }
            set
            {
                AdhesivoTextBlock.Text = value;
            }
        }

        public string Turno
        {
            get
            {
                return TurnoTextBlock.Text;
            }
            set
            {
                TurnoTextBlock.Text = value;
            }
        }

        public string Operador
        {
            get
            {
                return OperadorTextBlock.Text;
            }
            set
            {
                OperadorTextBlock.Text = value;
            }
        }

        public string Area
        {
            get
            {
                return AreaTextBlock.Text;
            }
            set
            {
                AreaTextBlock.Text = value;
            }
        }

        public string Longitud
        {
            get
            {
                return LongitudTextBlock.Text;
            }
            set
            {
                LongitudTextBlock.Text = value;
            }
        }

        public string Observaciones
        {
            get
            {
                return ObservacionesTextBlock.Text;
            }
            set
            {
                ObservacionesTextBlock.Text = value;
            }
        }

        public PageToPrint()
        {
            this.InitializeComponent();
        }


    }
}
