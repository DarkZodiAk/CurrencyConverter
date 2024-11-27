using System.Text;

namespace CurrencyConverter {
    public partial class App : Application {
        public App() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
