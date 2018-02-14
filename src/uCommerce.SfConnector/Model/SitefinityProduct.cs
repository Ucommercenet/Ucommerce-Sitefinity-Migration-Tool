using System;

namespace uCommerce.SfConnector.Model
{

    public class SitefinityProduct
    {
        public SitefinityProduct() { }
        public virtual System.Guid Id { get; set; }
        public virtual float? Weight { get; set; }
        public virtual byte Visible { get; set; }
        public virtual string UrlName_ { get; set; }
        public virtual string Title_ { get; set; }
        public virtual System.Nullable<System.Guid> TaxClassId { get; set; }
        public virtual int Status { get; set; }
        public virtual string Sku { get; set; }
        public virtual DateTime? SaleStartDate { get; set; }
        public virtual double? SalePrice { get; set; }
        public virtual DateTime? SaleEndDate { get; set; }
        public virtual DateTime? PublicationDate { get; set; }
        public virtual double Price { get; set; }
        public virtual DateTime LastModified { get; set; }
        public virtual byte IsVatTaxable { get; set; }
        public virtual byte IsUSCanadaTaxable { get; set; }
        public virtual byte IsShippable { get; set; }
        public virtual byte IsActive { get; set; }
        public virtual string Description_ { get; set; }
        public virtual System.Nullable<System.Guid> AssociateBuyerWithRole { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual short VoaVersion { get; set; }
        public virtual int VoaClass { get; set; }
        public virtual string ClrType { get; set; }
        public virtual int TrackInventory { get; set; }
        public virtual int OutOfStockOption { get; set; }
        public virtual int Inventory { get; set; }
        public virtual System.Nullable<System.Guid> Ownr { get; set; }
        public virtual System.Nullable<System.Guid> OriginalOwner { get; set; }
        public virtual System.Nullable<System.Guid> OriginalContentId { get; set; }
        public virtual System.Nullable<System.Guid> LastModifiedBy { get; set; }
        public virtual DateTime? ExpirationDate { get; set; }
        public virtual DateTime? DateCreated { get; set; }
        public virtual string ContentState { get; set; }
        public virtual string ApprovalWorkflowState { get; set; }
        public virtual System.Nullable<System.Guid> Id2 { get; set; }
    }
}
