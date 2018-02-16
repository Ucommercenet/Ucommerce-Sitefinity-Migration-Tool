using Dapper;

namespace uCommerce.SfConnector.Helpers
{
    /// <summary>
    /// Product and Product Type relationships are performed through dynamically created tables
    /// in Sitefinity (named sf_ec_prdct_{name}).  Since these tables are dynamic, it makes
    /// building the product to type relationships a bit more challenging in a migration.  
    /// Here we "normalize" those relationships, creating a temporary migration table
    /// migration_types_to_products, for use later in the data migration process.
    /// Considerations: different database engines?
    /// </summary>
    public static class CreateTypesToProductsMigrationData
    {
        public static void PopulateData(string connectionString)
        {
            using (var session = SqlSessionFactory.Create(connectionString))
            {
                var createRelationshipDataSql = @"
                    
                    declare @type_id uniqueidentifier;
                    declare @typeName nvarchar(255);
                    declare @clrType nvarchar(255);
                    declare @typeTableName nvarchar(255);
                    declare @sql varchar(500);

                    if OBJECT_ID('migration_types_to_products') IS NOT NULL 
                    begin
                        drop table migration_types_to_products 
                    end 
                    create table migration_types_to_products (
                        typename nvarchar(255) not null ,
                        productid uniqueidentifier not null)
                    create clustered index ix_migration_types_to_products_productid   
                        on dbo.migration_types_to_products (productid);   
                    create table #ProcessedTypes(id uniqueidentifier);

                    select top 1 @type_id = id from sf_ec_product_type;
                    while @type_id is not null
                    begin
                        select @typeName = title, @clrType = clr_type from sf_ec_product_type where id = @type_id;
                        select @typeTableName = substring(@clrType, 39, len(@clrType))
                        set @sql = 'insert into migration_types_to_products (typename, productid) select ''' + @typeName + ''', id from ' + @typeTableName
                        exec(@sql);
                        insert #ProcessedTypes(id) values (@type_id)
                        set @type_id = null
                        select top 1 @type_id = id from sf_ec_product_type where id NOT IN (select id from #ProcessedTypes)
                    end
                ";

                session.Execute(createRelationshipDataSql);
            }
        }
    }
}
