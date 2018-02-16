/*
Product and Product Type relationships are performed through dynamically created tables
in Sitefinity (named sf_ec_prdct_{type name}).   
Here we "normalize" those relationships for easier migration by creating a temporary migration table
migration_types_to_products.

table created: migration_types_to_products
fields:
  typename - string not null : the friendly type name stored in the Title field in Sitefinity
  productid - uniqueidentifier not null : the unique product id associated to the type
*/

declare @type_id uniqueidentifier;
declare @typeName nvarchar(255);
declare @clrType nvarchar(255);
declare @typeTableName nvarchar(255);
declare @sql varchar(500);

if OBJECT_ID('migration_types_to_products') is not null 
-- create the new migration table
begin
  drop table migration_types_to_products; 
end 
create table migration_types_to_products (
    typename nvarchar(255) not null ,
    productid uniqueidentifier not null)
create clustered index ix_migration_types_to_products_productid   
    on dbo.migration_types_to_products (productid);   

-- temporary table to keep track of types already processed
create table #ProcessedTypes(id uniqueidentifier);

select top 1 @type_id = id from sf_ec_product_type;
while @type_id is not null
begin
	-- for each type table, use a dynamic query to fetch any existing product 
	-- associations and append them to the new migration table
    select @typeName = title, @clrType = clr_type from sf_ec_product_type where id = @type_id;
    select @typeTableName = substring(@clrType, 39, len(@clrType))
    set @sql = 'insert into migration_types_to_products (typename, productid) select ''' + @typeName + ''', id from ' + @typeTableName
    exec(@sql);
    insert #ProcessedTypes(id) values (@type_id)
    set @type_id = null
    select top 1 @type_id = id from sf_ec_product_type where id NOT IN (select id from #ProcessedTypes)
end