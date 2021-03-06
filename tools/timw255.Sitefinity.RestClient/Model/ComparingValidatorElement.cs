﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace timw255.Sitefinity.RestClient.Model
{
    public class ComparingValidatorElement : ConfigElement
    {
        public string ControlToCompare { get; set; }
        public ValidationCompareOperator Operator { get; set; }
        public string ValidationDataType { get; set; }
        public string ValidationViolationMessage { get; set; }

        public ComparingValidatorElement() { }
    }
}
