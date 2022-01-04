using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barjonas.Common.Model
{
    /// <param name="Version">The version of this object, to aid backwards compatability implementation if the type changes.</param>
    /// <param name="Index">The zero-based index on the GPI which changed state.</param>
    /// <param name="Ordinal">In the case of a faceoff, the zero-based ordinal of this trigger. 0 = 1st.</param>
    /// <param name="TimeStamp">In the case of a faceoff, the high-precision interval elapsed since the faceoff started.</param>
    /// <param name="IsDown">If this is rising edge (i.e. button is down) then true. If this is a falling edge (e.g. button coming back up) then false.</param>
    /// <param name="IsTest">If this edge report is the result of test request then true, otherwise false.</param>
    public record EdgeReport(int Version, int Index, int? Ordinal, TimeSpan? TimeStamp, bool IsRising, bool IsTest);
}
