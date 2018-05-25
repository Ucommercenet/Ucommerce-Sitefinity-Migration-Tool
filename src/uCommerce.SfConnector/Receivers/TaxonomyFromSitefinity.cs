using System;
using System.Collections.Generic;
using System.Linq;
using MigrationCommon.Exceptions;
using timw255.Sitefinity.RestClient;
using timw255.Sitefinity.RestClient.Model;
using timw255.Sitefinity.RestClient.ServiceWrappers.Configuration;
using timw255.Sitefinity.RestClient.ServiceWrappers.Taxonomies;
using UConnector.Framework;

namespace uCommerce.SfConnector.Receivers
{
    public class TaxonomyFromSitefinity : Configurable, IReceiver<IEnumerable<WcfHierarchicalTaxon>>
    {
        public string SitefinityDepartmentTaxonomyId { private get; set; }
        public string SitefinityBaseUrl { private get; set; }
        public string SitefinityUsername { private get; set; }
        public string SitefinityPassword { private get; set; }
        public log4net.ILog Log { private get; set; }

        /// <summary>
        /// Fetch departments from Sitefinity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<WcfHierarchicalTaxon> Receive()
        {
            var categories = new List<WcfHierarchicalTaxon>();
            try
            {
                using (var sf = new SitefinityRestClient(SitefinityUsername, SitefinityPassword, SitefinityBaseUrl))
                {
                    Log.Info("fetching product types from Sitefinity");
                    categories = GetCategoriesForAllCultures(sf);
                    Log.Info($"{categories.Count()} departments returned from Sitefinity");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal($"A fatal exception occurred trying to fetch department data from Sitefinity: \n{ex}");
                throw new MigrationException("A fatal exception occurred trying to fetch department data from Sitefinity", ex);
            }

            return categories;
        }

        private List<WcfHierarchicalTaxon> GetCategoriesForAllCultures(SitefinityRestClient sf)
        {
            var culturesToMigrate = GetCultures(sf);
            List<WcfHierarchicalTaxon> categories;
            var categoriesWrapper = new HierarchicalTaxonServiceWrapper(sf);
            var defaultCulture = culturesToMigrate.First(x => x.IsDefault);
            categories = categoriesWrapper
                .GetTaxa(new Guid(SitefinityDepartmentTaxonomyId), "", "", 0, 0, "", "", false, "", defaultCulture.Culture).Items
                .ToList();

            AddCultureToCategories(categories, defaultCulture);

            var secondaryCultures = culturesToMigrate.Where(x => x.IsDefault == false);
            foreach (var culture in secondaryCultures)
            {
                var cultureCategories = categoriesWrapper
                    .GetTaxa(new Guid(SitefinityDepartmentTaxonomyId), "", "", 0, 0, "", "", false, "", culture.Culture).Items
                    .ToList();

                AddSecondaryCulturesToCategories(categories, cultureCategories, culture.Culture);
            }

            return categories;
        }

        private void AddCultureToCategories(List<WcfHierarchicalTaxon> categories, CultureViewModel defaultCulture)
        {
            foreach (var category in categories)
            {
                category.CultureCode = defaultCulture.Culture;
            }
        }

        private static void AddSecondaryCulturesToCategories(List<WcfHierarchicalTaxon> categories, List<WcfHierarchicalTaxon> cultureCategories, string culture)
        {
            foreach (var category in categories)
            {
                var translation = cultureCategories.First(x => x.Name == category.Name);
                category.CultureTranslations.Add(culture, translation);
            }
        }

        /// <summary>
        /// TODO: refactor this out to a common location for localizing other entities like products
        /// </summary>
        private static List<CultureViewModel> GetCultures(SitefinityRestClient sf)
        {
            var configuration = new ConfigSectionItemsServiceWrapper(sf);
            var languages = configuration.GetLocalizationBasicSettings(false);

            return languages.Cultures.ToList();
        }
    }
}
