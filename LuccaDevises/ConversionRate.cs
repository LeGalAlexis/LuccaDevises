using System;
using System.Collections.Generic;
using System.Text;

namespace LuccaDevises
{
    class ConversionRate
    {
        public string StartCurrencyName { get; set; }
        public string TargetCurrencyName { get; set; }
        public decimal Rate { get; set; }

        public ConversionRate(string startName, string targetName, decimal rate)
        {
            StartCurrencyName = startName;
            TargetCurrencyName = targetName;
            Rate = rate;
        }
    }
}
