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
        public ImageSource ChartImage { get; set; }
        public string Orden { get; set; }
        public string SKU { get; set; }
        public string Adhesivo { get; set; }
        public string Turno { get; set; }
        public string Operador { get; set; }
        public string Area { get; set; }
        public string Longitud { get; set; }
        public string Observaciones { get; set; }

        public PageToPrint()
        {
            this.InitializeComponent();
        }


    }
}
