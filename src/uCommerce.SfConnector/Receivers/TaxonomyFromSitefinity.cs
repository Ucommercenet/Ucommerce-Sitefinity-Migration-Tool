using System.Collections.Generic;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class TaxonomyFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityTaxonomy>>
    {
        public string ConnectionString { private get; set; }
        public string SitefinityDepartmentTaxonomyId { private get; set; }
        public log4net.ILog Log { private get; set; }

        public IEnumerable<SitefinityTaxonomy> Receive()
        {
            // Get all departments
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                var taxonomyData = connection.Query<SitefinityTaxonomy>($"select * from sf_taxa where taxonomy_id = '{SitefinityDepartmentTaxonomyId}'");
                return taxonomyData;
            }
        }
    }
}
