using System;
using System.Text;
using System.Collections.Generic;

namespace uCommerce.SfConnector.Model
{

    public class SitefinityProductType {
        public virtual System.Guid Id { get; set; }
        public virtual DateTime LastModified { get; set; }
        public virtual byte IsShippable { get; set; }
        public virtual string ClrType { get; set; }
        public virtual string ApplicationName { get; set; }
        public virtual short VoaVersion { get; set; }
        public virtual int ProductDeliveryType { get; set; }
        public virtual string Title_ { get; set; }
    }
}
