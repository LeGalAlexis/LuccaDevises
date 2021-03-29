using System;
using System.Collections.Generic;
using System.Text;

namespace LuccaDevises
{
    class Currency
    {
        public string Name { get; set; }
        public IList<ConversionRate> ConversionRates { get; set; }

        public Currency()
        {
            ConversionRates = new List<ConversionRate>();
        }
    }
}
