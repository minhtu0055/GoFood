﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.System.Users
{
    public class PagingRequestBase
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}
