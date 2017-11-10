using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Models
{
    public class PromoScript
    {
        public string ScriptTemplate { get; set; }
        public string ScriptName { get; set; }
        public string BiScriptCode { get; set; }
        public string Description { get; set; }
        public string CoreOfferSalesTax { get; set; }
        public string CrossSalesTax { get; set; }
        public string BrandCode { get; set; }

    }
}
