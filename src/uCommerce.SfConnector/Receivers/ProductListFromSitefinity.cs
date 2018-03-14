using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Adapters.Receivers
{
    /// <summary>
    /// Receiver for product data
    /// </summary>
    public class ProductListFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityProduct>>
    {
        public string ConnectionString { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Query all product data
        /// </summary>
        /// <returns>A list of sitefinity products</returns>
        public IEnumerable<SitefinityProduct> Receive()
        {
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                var languageSpecificFields = GetLanguageSpecificFields(connection);
                var productQuery = "select id, weight, visible, track_inventory, title_, tax_class_id, " +
                                   "status, sku, sale_start_date, sale_price, sale_end_date, " +
                                   "price, is_vat_taxable, is_u_s_canada_taxable, is_shippable " +
                                   "is_active, inventory, expiration_date, description_ " +
                                   languageSpecificFields +
                                   ", typename from sf_ec_product prod inner join " +
                                   "migration_types_to_products prodtypes ON " +
                                   "prod.id = prodtypes.productid where status > 0";

                return connection.Query<SitefinityProduct>(productQuery);
            }
        }

        /// <summary>
        /// When new cultures/languages are added to Sitefinity, Sitefinity extends its schema rather than adding
        /// rows for these values.  This method queries a list of cultures from one of the metadata tables and 
        /// assembles a list of fields based on those cultures using Sitefinity's convention based naming scheme
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>Language/Culure specific Title and Description fields</returns>
        private string GetLanguageSpecificFields(IDbConnection connection)
        {
            string languages = connection.Query<string>(
                @"select cultures from sf_schema_vrsns where module_name = 'Telerik.Sitefinity.Modules.Ecommerce.Catalog.Data.OpenAccessCatalogDataProvider'").FirstOrDefault();

            if (string.IsNullOrEmpty(languages))
            {
                return string.Empty;
            }

            var languageFields = new StringBuilder();
            foreach (var language in languages.Split(','))
            {
                languageFields.Append($",title_{language.Replace('-', '_')}");
                languageFields.Append($",description_{language.Replace('-', '_')}");
            }

            return languageFields.ToString();
        }
    }
}
