using FluentNHibernate.Mapping;
using uCommerce.SfConnector.Model;
using UCommerce.EntitiesV2;

namespace uCommerce.SfConnector.Mappings
{
    public class SitefinityProductMap : ClassMap<SitefinityProduct>
    {
        public SitefinityProductMap()
        {
            Id(x => x.Id);
            Map(x => x.Id);
            Map(x => x.Sku);
            Map(x => x.Weight);
            Map(x => x.Title).Column("title_");
            Map(x => x.Description).Column("description_");
            Map(x => x.Visible);
            Table("sf_ec_product");
        }
    }
}
