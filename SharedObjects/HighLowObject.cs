using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedObjects
{
    public class HighLowObject
    {
        private readonly static string TOSTRING = "Age: {0}; HighLow: {1}";

        public string HighOrLow { get; set; }

        public int CurrentAge { get; set; }

        public override string ToString()
        {
            var output = String.Format(TOSTRING, CurrentAge, HighOrLow);
            return output;
        }
    }

}