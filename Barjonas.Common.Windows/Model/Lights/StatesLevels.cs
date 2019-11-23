using System.Collections.ObjectModel;

namespace Barjonas.Common.Model.Lights
{
    public class StatesLevels : KeyedCollection<string, StateLevels>
    {
        protected override string GetKeyForItem(StateLevels item)
        {
            return item.Key;
        }
    }
}
