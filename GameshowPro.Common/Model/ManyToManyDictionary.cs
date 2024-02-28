namespace GameshowPro.Common.Model;

public abstract class ManyToManyDictionary<TKeyA, TKeyB, TPair>
    where TKeyA : notnull
    where TKeyB : notnull
    where TPair : Tuple<TKeyA, TKeyB>
{
    private readonly Dictionary<TKeyA, Dictionary<TKeyB, TPair>> _pairsByTKeyA = [];
    private readonly Dictionary<TKeyB, Dictionary<TKeyA, TPair>> _pairsByTKeyB = [];

    protected virtual void PairAdded(TPair pair) { }
    protected virtual void PairRemoved(TPair pair) { }

    protected bool TryAdd(TPair pair)
    {
        Dictionary<TKeyA, TPair> byKeyA;
        Dictionary<TKeyB, TPair> byKeyB;
        if (_pairsByTKeyA.ContainsKey(pair.Item1))
        {
            byKeyB = _pairsByTKeyA[pair.Item1];
        }
        else
        {
            byKeyB = [];
            _pairsByTKeyA.Add(pair.Item1, byKeyB);
        }
        if (byKeyB.ContainsKey(pair.Item2))
        {
            return false;
        }
        byKeyB.Add(pair.Item2, pair);

        if (_pairsByTKeyB.ContainsKey(pair.Item2))
        {
            byKeyA = _pairsByTKeyB[pair.Item2];
        }
        else
        {
            byKeyA = [];
            _pairsByTKeyB.Add(pair.Item2, byKeyA);
        }
        if (byKeyA.ContainsKey(pair.Item1))
        {
            throw new InvalidOperationException("Dictionaries are unbalanced!");
        }
        byKeyA.Add(pair.Item1, pair);
        PairAdded(pair);
        return true;
    }

    private TKeyA GetKeyA(TPair pair) => pair.Item1;
    private TKeyB GetKeyB(TPair pair) => pair.Item2;

    private bool TryRemove<TPrimary, TForeign>(
        TPrimary key,
        Dictionary<TPrimary, Dictionary<TForeign, TPair>> primaryDictionary,
        Dictionary<TForeign, Dictionary<TPrimary, TPair>> foreignDictionary,
        Func<TPair, TForeign> getForeignKey
        )
        where TPrimary : notnull
        where TForeign : notnull
    {
        if (primaryDictionary.TryGetValue(key, out Dictionary<TForeign,TPair>? commandsForTrigger))
        {
            foreach (TPair pair in commandsForTrigger.Values)
            {
                TForeign foreign = getForeignKey(pair);
                if (foreignDictionary.TryGetValue(foreign, out Dictionary<TPrimary, TPair>? triggersForCommand))
                {
                    PairRemoved(pair);
                    if (triggersForCommand.ContainsKey(key))
                    {
                        _ = triggersForCommand.Remove(key);
                        if (triggersForCommand.Count <= 0)
                        {
                            _ = foreignDictionary.Remove(foreign);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Dictionaries are unbalanced!");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Dictionaries are unbalanced!");
                }

            }
            commandsForTrigger.Clear();
            _ = primaryDictionary.Remove(key);
        }
        return false;
    }

    public bool TryRemove(TKeyA key)
        => TryRemove(key, _pairsByTKeyA, _pairsByTKeyB, GetKeyB);

    public bool TryRemove(TKeyB key)
        => TryRemove(key, _pairsByTKeyB, _pairsByTKeyA, GetKeyA);

    public void Clear()
    {
        foreach (Dictionary<TKeyB, TPair> byKeyB in _pairsByTKeyA.Values)
        {
            foreach (TPair pair in byKeyB.Values)
            {
                PairRemoved(pair);
            }
        }
        _pairsByTKeyB.Clear();
        _pairsByTKeyA.Clear();
    }
}

