using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
#nullable enable
namespace Barjonas.Common.Model.Lights
{
    
    internal record FixtureSettings
    (
        string Key,
        string? DisplayName,
        int? StartId
    )
    {
        internal static FixtureSettings FromFixture(Fixture fixture)
            => new(fixture.Key, fixture.DisplayName, fixture.StartId);

        internal void ToFixture(Fixture fixture)
        {
            if (DisplayName != null)
            {
                fixture.DisplayName = DisplayName;
            }
            if (StartId.HasValue)
            {
                fixture.StartId = StartId.Value;
            }
        }
    }
}
#nullable restore
