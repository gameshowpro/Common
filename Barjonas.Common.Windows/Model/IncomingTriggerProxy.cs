using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barjonas.Common.Model
{
    /// <summary>
    /// A proxy which can be referenced from a model (e.g. contestant objects) allowing the trigger implementation to be changed dynamically (e.g. depending when user requests switch to a different source)
    /// </summary>
    public class IncomingTriggerProxy<TKey> : NotifyingClass, ITrigger
        where TKey : notnull
    {
        private readonly ImmutableDictionary<TKey, IncomingTrigger> _options;
        public event EventHandler<TriggerArgs>? Triggered;
        public IncomingTriggerProxy(ImmutableDictionary<TKey, IncomingTrigger> options)
        {
            _options = options;
        }

        private bool _firstSetIsDone = false; //If TKey is an enum, the source key will already be set to default, but the source will not be set
        private TKey? _currentSourceKey;
        [DisallowNull]
        public TKey? CurrentSourceKey
        {
            get => _currentSourceKey;
            set
            {
                if ((SetProperty(ref _currentSourceKey, value) || !_firstSetIsDone) && _options.TryGetValue(value, out IncomingTrigger? newSource))
                {
                    Source = newSource;
                    _firstSetIsDone = true;
                }
            }
        }

        public bool TryGetOption(TKey key, out IncomingTrigger? option)
            => _options.TryGetValue(key, out option);

        private IncomingTrigger? _source;
        public IncomingTrigger? Source
        {
            get => _source;
            private set {
                IncomingTrigger? previous = _source;
                if (SetProperty(ref _source, value))
                {
                    if (previous is not null)
                    {
                        previous.Triggered -= RelayTrigger;
                    }
                    if (_source is not null)
                    {
                        _source.Triggered += RelayTrigger;
                    }
                }
            }
        }

        private void RelayTrigger(object? sender, TriggerArgs args)
        {
            Triggered?.Invoke(sender, args);
        }
    }
}
