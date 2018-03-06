using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using uCommerce.SfConnector.Helpers;
using uCommerce.SfConnector.Model;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class TaxonomyFromSitefinity : Configurable, IReceiver<IEnumerable<SitefinityTaxonomy>>
    {
        public string ConnectionString { private get; set; }

        private const string DepartmentTaxonomyId = "D7831091-E7B1-41B8-9E75-DFF32D6A7837"; // TODO move to config

        public IEnumerable<SitefinityTaxonomy> Receive()
        {
            // Get all departments
            using (var connection = SqlSessionFactory.Create(ConnectionString))
            {
                var taxonomyData = connection.Query<SitefinityTaxonomy>($"select * from sf_taxa where taxonomy_id = '{DepartmentTaxonomyId}'");
                return taxonomyData;
            }
        }
    }
}
