SITEFINITY TO UCOMMERCE DATA MIGRATION
==================================================

Major Entities/Relationships Currently Supported (migrated):
- Single Site/Catalog
- Allowable currencies
- Departments (categories in UCommerce)
- Parent/child relationships in Departments/Categories to nth level
- Product types/definitions  
- Products
- Product relationships to product types/definitions
- Product relationships to categories
- Product variants
- Custom attributes on product types/definitions - currently stored as text only in UCommerce
- Product prices
- Culture specific category names and descriptions
- Culture specific product names and descriptions
- Culture specific product variant values
- Product availability/inventory

Planned Support:
- Product images
- Friendly Urls
- Customers / Orders
- Product reviews
 
Prior to performing migration, perform the following:

1. Backup Sitefinity database
2. Install a fresh instance of UCommerce
3. Update app.config in the Migration.CommandLine.Runner project where appropriate:
	a) SitefinityDepartmentTaxonomyId: this is the id (guid) of the Department taxonomy in the Sitefinity sf_taxonomies database table in Sitefinity. 
	   The current default value should be correct.
	b) DefaultUcommerceCategoryDefinitionName: The name of the UCommerce definition that all imported departments will use in UCommerce as defined 
	   in the UCommerce_Definition database table in UCommerce.  This value defaults to the "Default Category Definition"
	c) DefaultUCommerceCatalogGroupName: the name used for the catalog group created in UCommerce and associated to the UCommerce catalog.
	   Note: the migration tool uses the Sitefinity site name as the name of the UCommerce catalog itself.
    d) SitefinitySiteName: the name of the Sitefinity site to be migrated, stored in the nme field, sf_sites table in the Sitefinity database.
	e) SitefinityBaseUrl: base url of sitefinity and also the sitefinity web services (i.e. http://staging.sitefinity.mycompany.com)
	f) SitefinityUsername: the username that the migration tool will use to access the sitefinity web services on the backend.  
	   Creating a separate account on the Sitefinity Administration backend site is recommended in Administration/Users.  
	   Be sure the account has the Administrators role.
	g) SitefinityPassword: password of the Sitefinity user account mentioned above for accessing web services.
    h) SitefinityConnectionString: valid connection string to the source Sitefinity database.
    i) UCommerceConnectionString: valid connection string to the target UCommerce database.	
	

Additional Technical Notes:

The migration tool is built upon the UCommerce UConnector foundation and its Fluid RTS model.
Anyone familiar with UConnector should be fairly comfortable with the structure of the tool when reviewing source.
Pulling sitefinity data is primarily managed through the sitefinity web services using a lightly modified
version of the timw255.Sitefinity.RestClient (https://github.com/timw255/timw255.Sitefinity.RestClient).
Where necessary, data is pulled directly from the Sitefinity database using the lightweight Dapper
ORM (https://github.com/StackExchange/Dapper).   Data is pushed into UCommerce, facilitated by UConnector
and the UCommerce APIs.  nHibernate is used on the UCommerce side as its ORM.

Testing during initial development was done against a rather small "happy path" dataset.  There are some more minor attributes of Entities 
that are defaulted or ignored in the mapping.   There will be fine tuning done as more extensive testings is performed and 
results analyzed. 

It is anticipated that a batching mechanism will also need to be added, particularly for product migration in product catalogs that are quite large.  
The amount of data placed in memory for a large catalog to process products in one RTS iteration is likely untenable.   Further testing against
real customer catalogs should help reveal any breaking points to guide further design consideration and refactoring.