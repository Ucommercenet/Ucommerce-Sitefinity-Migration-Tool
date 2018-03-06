using System;

namespace uCommerce.SfConnector.Model {
    
    public class SitefinityTaxonomy {
        public virtual System.Guid Id { get; set; }
        public virtual string UrlName_ { get; set; }
        public virtual string Title_ { get; set; }
        public virtual System.Nullable<System.Guid> TaxonomyId { get; set; }
        public virtual byte ShowInNavigation { get; set; }
        public virtual byte RenderAsLink { get; set; }
        public virtual System.Nullable<System.Guid> Parent_Id { get; set; }
        public virtual float Ordinal { get; set; }
        public virtual string Nme { get; set; }
        public virtual DateTime? LastModified { get; set; }
        public virtual string Description { get; set; }
        public virtual string AppName { get; set; }
        public virtual int VoaClass { get; set; }
        public virtual short VoaVersion { get; set; }
        public virtual System.Nullable<System.Guid> FctTxnFctTxId { get; set; }
        public virtual string UrlNameEs { get; set; }
        public virtual string UrlNameEn { get; set; }
        public virtual string TitleEs { get; set; }
        public virtual string TitleEn { get; set; }
        public virtual string DescriptionEs { get; set; }
        public virtual string DescriptionEn { get; set; }
    }
}
