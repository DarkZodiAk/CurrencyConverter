using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace CurrencyConverter {
    public partial class MainPage : ContentPage {
        public MainPage() {
            InitializeComponent();

            BindingContext = new CurrencyViewModel();
        }
    }


    public class CurrencyViewModel : BindableObject, INotifyPropertyChanged {
        
        public CurrencyViewModel() {
            LoadPreferences();
        }

        public ObservableCollection<CurrencyRate> Currencies { get; set; } = [];

        private CurrencyRate currency1;
        private CurrencyRate currency2;

        private DateTime selectedDate = DateTime.Today;

        private string tooltipInfo;
        private string currency1Value;
        private string currency2Value;
        private bool lock1 = false;
        private bool lock2 = false;


        public CurrencyRate Currency1 {
            get => currency1;
            set {
                if (currency1 != value) {
                    currency1 = value;
                    lock2 = true;
                    if (!lock1) UpdateConversion1();
                    lock2 = false;
                    SaveCurrency1();
                    OnPropertyChanged();
                }
            }
        }

        public CurrencyRate Currency2 {
            get => currency2;
            set {
                if (currency2 != value) {
                    currency2 = value;
                    lock1 = true;
                    if (!lock2) UpdateConversion2();
                    lock1 = false;
                    SaveCurrency2();
                    OnPropertyChanged();
                }
            }
        }

        public string Currency1Value {
            get => currency1Value;
            set {
                if (currency1Value != value) {
                    currency1Value = value;
                    lock2 = true;
                    if(!lock1) UpdateConversion1();
                    lock2 = false;
                    SaveCurrency1Value();
                    OnPropertyChanged();
                }
            }
        }

        public string Currency2Value {
            get => currency2Value;
            set {
                if (currency2Value != value) {
                    currency2Value = value;
                    lock1 = true;
                    if (!lock2) UpdateConversion2();
                    lock1 = false;
                    OnPropertyChanged();
                }
            }
        }
        
        public DateTime MaxDate { get; } = DateTime.Today;
        public DateTime SelectedDate { 
            get => selectedDate; 
            set {
                if(selectedDate != value) {
                    selectedDate = value;
                    OnPropertyChanged();
                    FetchRates();
                }
            }
        }

        public string TooltipInfo { 
            get => tooltipInfo; 
            set {
                if(tooltipInfo != value) {
                    tooltipInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        private void FetchRates() {
            var code1 = Currency1?.NumCode;
            var code2 = Currency2?.NumCode;
            var date = selectedDate;

            while (true) {
                Currencies.Clear();
                foreach (var item in CurrencyApi.GetCurrencyRates(date)) {
                    Currencies.Add(item);
                };

                if (Currencies.Count < 2 || ((code1 != null && code2 != null) &&
                    (Currencies.All(x => x.NumCode != code1) && Currencies.All(x => x.NumCode != code2)))
                    )
                    date = date.AddDays(-1);
                else break;
            }

            if (code1 != null) {
                lock1 = true;
                Currency1 = Currencies.First(x => x.NumCode == code1);
            }

            if (code2 != null) {
                lock2 = true;
                Currency2 = Currencies.First(x => x.NumCode == code2);
            }

            UpdateConversion1();
            lock2 = false;

            SelectedDate = date;
            SaveDate();
            TooltipInfo = $"Курс на {date:dd MMM yyyy}";
        }

        private void UpdateConversion1() {
            if (Currency1 != null && Currency2 != null && double.TryParse(Currency1Value, out double amount)) {
                double c1 = (Currency1.Value / Currency1.Nominal) * amount;
                double c2 = (Currency2.Value / Currency2.Nominal);
                Currency2Value = (c1 / c2).ToString();
            } 
        }

        private void UpdateConversion2() {
            if (Currency1 != null && Currency2 != null && double.TryParse(Currency2Value, out double amount)) {
                double c1 = (Currency2.Value / Currency2.Nominal) * amount;
                double c2 = (Currency1.Value / Currency1.Nominal);
                Currency1Value = (c1 / c2).ToString();
            }
        }

        private void SaveDate() {
            Preferences.Set(DATE, SelectedDate.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        private void SaveCurrency1() {
            if (Currency1 != null) {
                Preferences.Set(CURRENCY_1_CODE, Currency1.NumCode);
            }
        }

        private void SaveCurrency2() {
            if (Currency2 != null) {
                Preferences.Set(CURRENCY_2_CODE, Currency2.NumCode);
            }
        }

        private void SaveCurrency1Value() {
            if (Currency1Value != null) {
                Preferences.Set(CURRENCY_1_VALUE, Currency1Value);
            }
        }

        private void LoadPreferences() {
            string date = Preferences.Get(DATE, "");
            if(!string.IsNullOrEmpty(date)) {
                SelectedDate = DateTime.Parse(date);
            }
            FetchRates();

            int code1 = Preferences.Get(CURRENCY_1_CODE, -1);
            int code2 = Preferences.Get(CURRENCY_2_CODE, -1);
            if(code1 != -1) {
                Currency1 = Currencies.First(x => x.NumCode == code1);
            }
            if(code2 != -1) {
                Currency2 = Currencies.First(x => x.NumCode == code2);
            }
            Currency1Value = Preferences.Get(CURRENCY_1_VALUE, "0");
        }

        private const string CURRENCY_1_CODE = "CURRENCY_1_CODE";
        private const string CURRENCY_2_CODE = "CURRENCY_2_CODE";
        private const string CURRENCY_1_VALUE = "CURRENCY_1_VALUE";
        private const string DATE = "DATE";


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
