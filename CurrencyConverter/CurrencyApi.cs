using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace CurrencyConverter {

    public class CurrencyApi {
        public static Collection<CurrencyRate> GetCurrencyRates(DateTime date) {
            string BaseUrl = "https://cbr.ru/scripts/XML_daily.asp";
            string formattedDate = date.ToString("dd/MM/yyyy");

            string url = $"{BaseUrl}?date_req={formattedDate}";

            using (WebClient client = new WebClient()) {
                client.Encoding = Encoding.GetEncoding("Windows-1251");
                try {
                    string xmlData = client.DownloadString(url);
                    var xml = XDocument.Parse(xmlData);

                    if(xml == null) return new Collection<CurrencyRate> { };
                    if(date != GetResponseDate(xml)) return new Collection<CurrencyRate> { };

                    var res = XmlToCurrencies(xml);
                    return res;
                } catch (WebException ex) {
                    Console.WriteLine($"Error fetching data: {ex.Message}");
                    return null;
                }
            }
        }

        private static DateTime GetResponseDate(XDocument currencyRates) {
            string date = (string)currencyRates.Root.Attribute("Date");
            return DateTime.Parse(date);
        }

        private static Collection<CurrencyRate> XmlToCurrencies(XDocument currencyRates) {
            var result = new Collection<CurrencyRate> { RubRate };

            var valutes = currencyRates.Descendants("Valute");

            foreach (var valute in valutes) {
                string id = (string)valute.Attribute("ID");
                string numCode = (string)valute.Element("NumCode");
                string charCode = (string)valute.Element("CharCode");
                string nominal = (string)valute.Element("Nominal");
                string name = (string)valute.Element("Name");
                string value = (string)valute.Element("Value");

                double valueDecimal = double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
                int nominalInt = int.Parse(nominal, CultureInfo.InvariantCulture);
                int numCodeInt = int.Parse(numCode, CultureInfo.InvariantCulture);


                result.Add(new CurrencyRate() { 
                    ID = id,
                    NumCode = numCodeInt,
                    Value = valueDecimal,
                    Nominal = nominalInt,
                    Name = name + " (" + charCode + ")",
                });
            }
            return result;
        }

        public static CurrencyRate RubRate = new CurrencyRate() {
            ID = "0",
            NumCode = 0,
            Value = 1,
            Nominal = 1,
            Name = "Российский рубль (RUB)",
        };
    }


    public class CurrencyRate {
        public string ID { get; set; }
        public int NumCode { get; set; }
        public double Value { get; set; }
        public int Nominal { get; set; }
        public string Name { get; set; }
    }
}
