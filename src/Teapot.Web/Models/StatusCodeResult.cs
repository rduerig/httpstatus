using System;
using System.Collections.Generic;

namespace Teapot.Web.Models
{
    public class StatusCodeResult
    {
        public StatusCodeResult()
        {
            IncludeHeaders = new Dictionary<string, string>();
        }
        public string Description { get; set; }
        public Dictionary<string, string> IncludeHeaders { get; set; }
        public bool ExcludeBody { get; set; }
        public Uri Link { get; set; }
    }
}