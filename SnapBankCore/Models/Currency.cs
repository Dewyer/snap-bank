using System;
using System.Collections.Generic;
using System.Text;

namespace SnapBank.Core.Models
{
    public class Currency
    {
        public long  Amount { get; set; }
        //public CurrencyType CurrencyType { get; set; }

        public static bool operator <(Currency c1,Currency c2)
        {
            return c1.Amount < c2.Amount;
        }
        public static bool operator >(Currency c1, Currency c2)
        {
            return c1.Amount > c2.Amount;
        }

    }
    /*
    public enum CurrencyType
    {
        HUF=0,
        USD=1,
        EUR=2
    }
    */
}
