using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barjonas.Common.ViewModel
{
    public class StringItem
    {
        internal StringItem(string value) => Value = value;

        public string Value { get; }
    }
}
