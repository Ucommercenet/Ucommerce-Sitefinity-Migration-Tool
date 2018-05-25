﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace timw255.Sitefinity.RestClient.Model
{
    public class BlogViewModel : HierarchicalContentViewModelBase
    {
        public DateTime? LastPostDate { get; set; }
        public int PostCount { get; set; }

        public BlogViewModel() { }
    }
}
