using System.Collections.Generic;
using timw255.Sitefinity.RestClient.Model;

namespace uCommerce.SfConnector.Model
{
    public class SitefinityCatalogCurrencyCulture
    {
        public string CatalogName { get; set; }
        public CurrenciesAllowedSettingsViewModel AllowedCurrencies { get; set; }
        public List<CultureElement> Cultures { get; set; }
    }
}
