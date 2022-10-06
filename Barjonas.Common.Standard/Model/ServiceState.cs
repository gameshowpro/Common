using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MessagePack;
using MessagePack.Formatters;
using Newtonsoft.Json.Linq;
#nullable enable
namespace Barjonas.Common.Model;

/// <summary>
/// Record representing the state of a service.
/// </summary>
/// <remarks>
/// Useful for transferring state details between services.
/// Subclasses may extend ServiceState to add a list of sub-services to give a more detailed report of the current state, e.g. Sounds are OK, GPI card not found.
/// </remarks>
/// <param name="Progress">Any progress associated with the state. If finished, value should be 1. If not started, value should be 0. If indeterminate, value should be null. This is represented by a constant spinner.</param>
[MessagePackObject, MessagePackFormatter(typeof(MsgPackResolver))]
public class ServiceState : INotifyPropertyChanged, IEquatable<ServiceState>
{
    public delegate void SetAllDelegate(RemoteServiceStates aggregateState, string? detail, double? progress = 0);
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? AllUpdated;
    [IgnoreMember]
    private readonly Func<ServiceState, RemoteServiceStates>? _serviceStateAggregator;
    [IgnoreMember]
    private readonly Func<ServiceState, string>? _detailAggregator;
    [IgnoreMember]
    private readonly Func<ServiceState, double?>? _progressAggregator;

    public static ServiceState CreateUnknown(string key, string name)
        => new
        (
            key,
            name,
            new ObservableDictionary<string, ServiceState>(),
            RemoteServiceStates.Disconnected,
            "Unknown",
            0,
            GetAggregateState,
            GetAggregateDetail,
            GetAggregateProgress
        );

    public static ServiceState CreateUnknown(string name)
        => CreateUnknown(name, name);

    /// <summary>
    /// Create service state with manual values that will not be automatically aggregated. Default JSON constructor.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="aggregateState"></param>
    /// <param name="detail"></param>
    /// <param name="progress"></param>
    /// <param name="children"></param>
    [JsonConstructor, SerializationConstructor]
    public ServiceState(string key, string? name, RemoteServiceStates? aggregateState, string? detail, double? progress, IDictionary<string, ServiceState>? children) :
        this
        (
            key,
            name ?? key,
            children == null ? new ObservableDictionary<string, ServiceState>() : new ObservableDictionary<string, ServiceState>(children),
            aggregateState ?? RemoteServiceStates.Disconnected,
            detail,
            progress,
            null,
            null,
            null)
    {
    }

    /// <summary>
    /// The base-level constructor. By this point, if any aggregators are null, we presume that no automatic aggregation is required.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="name"></param>
    /// <param name="aggregateState"></param>
    /// <param name="detail"></param>
    /// <param name="progress"></param>
    /// <param name="children"></param>
    /// <param name="serviceStateAggregator"></param>
    /// <param name="detailAggregator"></param>
    /// <param name="progressAggregator"></param>
    public ServiceState(
        string key,
        string name,
        ObservableDictionary<string, ServiceState> children,
        RemoteServiceStates aggregateState,
        string? detail, 
        double? progress,
        Func<ServiceState, RemoteServiceStates>? serviceStateAggregator,
        Func<ServiceState, string>? detailAggregator,
        Func<ServiceState, double?>? progressAggregator
    )
    {
        Key = key;
        Name = name;
        Children = children;
        _aggregateState = aggregateState;
        _detail = detail;
        _progress = progress;
        if (serviceStateAggregator != null)
        {
            _serviceStateAggregator = serviceStateAggregator;
            Children.ItemPropertyChanged += InvokeServiceStateAggregator;
            Children.CollectionChanged += (s, e) => _serviceStateAggregator.Invoke(this);
            serviceStateAggregator.Invoke(this);
        }
        if (detailAggregator != null)
        {
            _detailAggregator = detailAggregator;
            Children.ItemPropertyChanged += InvokeDetailAggregator;
            Children.CollectionChanged += (s, e) => _detailAggregator.Invoke(this);
            detailAggregator.Invoke(this);
        }
        if (progressAggregator != null)
        {
            _progressAggregator = progressAggregator;
            Children.ItemPropertyChanged += InvokeProgressAggregator;
            Children.CollectionChanged += (s, e) => _progressAggregator.Invoke(this);
            _progressAggregator.Invoke(this);
        }
    }

    private void InvokeServiceStateAggregator(object? sender, PropertyChangedEventArgs e)
    {
        if (_serviceStateAggregator != null && e.PropertyName == nameof(AggregateState))
        {
            AggregateState = _serviceStateAggregator.Invoke(this);
        }
    }

    private void InvokeDetailAggregator(object? sender, PropertyChangedEventArgs e)
    {
        if (_detailAggregator != null && e.PropertyName == nameof(Detail))
        {
            Detail = _detailAggregator.Invoke(this);
        }
    }

    private void InvokeProgressAggregator(object? sender, PropertyChangedEventArgs e)
    {
        if (_progressAggregator != null && e.PropertyName == nameof(Progress))
        {
            Progress = _progressAggregator.Invoke(this);
        }
    }

    /// <summary>
    /// Create service state with key but without any children and automatic aggregation..
    /// </summary>
    public ServiceState
        (
            string key, 
            string name, 
            RemoteServiceStates aggregateState, 
            string? detail, 
            double? progress
        )
        :
        this
        (
            key,
            name,
            new ObservableDictionary<string, ServiceState>(),
            aggregateState,
            detail,
            progress,
            GetAggregateState,
            GetAggregateDetail,
            GetAggregateProgress
        )
    { }

    /// <summary>
    /// Create service state without any children and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string name, 
            RemoteServiceStates aggregateState, 
            string? detail, 
            double? progress
        ) 
        : 
        this
        (
            name, 
            name, 
            aggregateState, 
            detail, 
            progress
        )
    { }

    public ServiceState(string name)
        :
        this
        (
            name,
            RemoteServiceStates.Disconnected,
            null,
            0
        )
    { }

    public ServiceState(string key, string name, IEnumerable<string> children)
    :
    this
    (
        key,
        name,
        children,
        GetAggregateState,
        GetAggregateDetail,
        GetAggregateProgress
    )
    { }

    public ServiceState(string name, IEnumerable<string> children)
    :
    this
    (
        name,
        name,
        children
    )
    { }

    public ServiceState(string key, string name, IEnumerable<ServiceState> children)
    :
    this
    (
        key,
        name,
        children,
        GetAggregateState,
        GetAggregateDetail,
        GetAggregateProgress
    )
    { }

    public ServiceState(string name, IEnumerable<ServiceState> children)
    :
    this
    (
        name,
        name,
        children
    )
    { }

    /// <summary>
    /// Create service state with key and named children in default states and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string key,
            string name,
            Func<ServiceState, RemoteServiceStates> serviceStateAggregator,
            Func<ServiceState, string> detailAggregator,
            Func<ServiceState, double?> progressAggregator
        )
        :
        this
        (
            key,
            name,
            new ObservableDictionary<string, ServiceState>(),
            RemoteServiceStates.Disconnected,
            null,
            0,
            serviceStateAggregator,
            detailAggregator,
            progressAggregator
        )
    { }

    /// <summary>
    /// Create service state with key and named children in default states and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string key,
            string name,
            IEnumerable<ServiceState> children,
            Func<ServiceState, RemoteServiceStates> serviceStateAggregator,
            Func<ServiceState, string> detailAggregator,
            Func<ServiceState, double?> progressAggregator
        )
        :       
        this
        (
            key,
            name,
            new ObservableDictionary<string, ServiceState>(children.ToDictionary(s => s.Key, s => s)),
            RemoteServiceStates.Disconnected,
            null,
            0,
            serviceStateAggregator,
            detailAggregator,
            progressAggregator
      )
    { }

    /// <summary>
    /// Create service state with key and named children in default states and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string name,
            IEnumerable<ServiceState> children,
            Func<ServiceState, RemoteServiceStates> serviceStateAggregator,
            Func<ServiceState, string> detailAggregator,
            Func<ServiceState, double?> progressAggregator
        )
        :
        this
        (
            name,
            name,
            children,
            serviceStateAggregator,
            detailAggregator,
            progressAggregator
      )
    { }

    /// <summary>
    /// Create service state with key and named children in default states and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string key,
            string name,
            IEnumerable<string> children,
            Func<ServiceState, RemoteServiceStates> serviceStateAggregator,
            Func<ServiceState, string> detailAggregator,
            Func<ServiceState, double?> progressAggregator
        )
        :
        this
        (
            key,
            name,
            children.Select(s => new ServiceState(s)),
            serviceStateAggregator,
            detailAggregator,
            progressAggregator
      )
    { }

    /// <summary>
    /// Create service state with key and named children in default states and automatic aggregation.
    /// </summary>
    public ServiceState
        (
            string name,
            IEnumerable<string> children,
            Func<ServiceState, RemoteServiceStates> serviceStateAggregator,
            Func<ServiceState, string> detailAggregator,
            Func<ServiceState, double?> progressAggregator
        )
        :
        this
        (
            name,
            name,
            children,
            serviceStateAggregator,
            detailAggregator,
            progressAggregator
      )
    { }

    [Key(0)]
    public string Key { get; }
    [Key(1)]
    public string Name { get; }

    [IgnoreMember]
    private RemoteServiceStates _aggregateState;
    [Key(2)]
    public RemoteServiceStates AggregateState
    {
        get => _aggregateState;
        set
        {
            if (_aggregateState != value)
            {
                _aggregateState = value;
                PropertyChanged?.Invoke(this, new(nameof(AggregateState)));
            }
        }
    }

    [IgnoreMember]
    private string? _detail;
    [Key(3)]
    public string? Detail
    {
        get => _detail;
        set
        {
            if (_detail != value)
            {
                _detail = value;
                PropertyChanged?.Invoke(this, new(nameof(Detail)));
            }
        }
    }

    [IgnoreMember]
    private double? _progress = 0;
    [Key(4)]
    public double? Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                PropertyChanged?.Invoke(this, new(nameof(Progress)));
            }
        }
    }

    [Key(5)]
    public ObservableDictionary<string, ServiceState> Children { get; }

    public void SetAll(RemoteServiceStates aggregateState, string? detail = "", double? progress = 0)
    {
        List<string> changes = new(3);
        if (_aggregateState != aggregateState)
        {
            _aggregateState = aggregateState;
            changes.Add(nameof(AggregateState));
        }
        if (_detail != detail)
        {
            _detail = detail;
            changes.Add(nameof(Detail));
        }
        if (_progress != progress)
        {
            _progress = progress;
            changes.Add(nameof(Progress));
        }
        changes.ForEach(s => PropertyChanged?.Invoke(this, new(s)));
        AllUpdated?.Invoke(this, new());
    }

    public static RemoteServiceStates GetAggregateState(ServiceState state)
    {
        if (state.Children.All(s => s.Value.AggregateState == RemoteServiceStates.Connected))
        {
            return RemoteServiceStates.Connected;
        }
        else if (state.Children.All(s => s.Value.AggregateState == RemoteServiceStates.Disconnected))
        {
            return RemoteServiceStates.Disconnected;
        }
        else
        {
            return RemoteServiceStates.Warning;
        }
    }

    public static string GetAggregateDetail(ServiceState state)
    {
        List<ServiceState> notConnected = state.Children.Values.Where(s => s.AggregateState != RemoteServiceStates.Connected).ToList();
        if (notConnected.Any())
        {
            
            if (notConnected.Count == 1)
            {
                string? firstWarning = notConnected[0].Detail;
                return firstWarning ?? "1 warning";
            }
            else
            {
                return $"{notConnected.Count} warnings";
            }
        }
        else
        {
            return "OK";
        }
    }

    public static double? GetAggregateProgress(ServiceState state)
    {
        if (state.Children.Any(s => s.Value.Progress == null))
        {
            return null;
        }
        else
        {
            return state.Children.Average(s => s.Value.Progress) ?? 0;
        }
    }

    public static bool operator ==(ServiceState obj1, ServiceState obj2)
    {
        if (ReferenceEquals(obj1, obj2))
        {
            return true;
        }
        if (obj1 is null)
        {
            return false;
        }
        if (obj2 is null)
        {
            return false;
        }

        return obj1.Equals(obj2);
    }

    public static bool operator !=(ServiceState obj1, ServiceState obj2)
    {
        return !(obj1 == obj2);
    }

    public bool Equals(ServiceState? other)
    {
        if (other is null)
        {
            return false;
        }
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other.Name == Name &&
        other.Detail == Detail &&
        other.AggregateState == AggregateState &&
        other.Progress == Progress &&
        other.Children.SequenceEqual(Children);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ServiceState);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Name.GetHashCode();
            hashCode = (hashCode * 397) ^ (Detail?.GetHashCode() ?? 1);
            hashCode = (hashCode * 397) ^ AggregateState.GetHashCode();
            hashCode = (hashCode * 397) ^ Progress.GetHashCode();
            foreach (var child in Children)
            {
                hashCode = (hashCode * 397) ^ child.GetHashCode();
            }
            return hashCode;
        }
    }

    public void UpdateChildren(params ServiceState[] children)
        => UpdateChildren((IEnumerable<ServiceState>)children);

    public void UpdateChildren(IEnumerable<ServiceState>? children)
    {
        if (children == null)
        {
            Children.Clear();
            return;
        }
        HashSet<string> remainingKeys = new(Children.Keys);
        foreach(ServiceState child in children)
        {
            if (Children.TryGetValue(child.Key, out ServiceState? existingChild))
            {
                existingChild.UpdateFrom(child);
                remainingKeys.Remove(child.Key);
            }
            else
            {
                Children.Add(child.Key, child);
            }    
        }
        foreach (string key in remainingKeys)
        {
            _ = Children.Remove(key);
        }
    }

    public void UpdateFrom(ServiceState other)
    {
        AggregateState = other.AggregateState;
        Detail = other.Detail;
        Progress = other.Progress;
        UpdateChildren(other.Children.Values);
        AllUpdated?.Invoke(this, new ());
    }
    public class MsgPackResolver : IMessagePackFormatter<ServiceState>
    {
        private const int CurrentFieldCount = 6;
        public ServiceState Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            int fieldcount = reader.ReadArrayHeader();
            if (fieldcount < CurrentFieldCount)
            {
                throw new MessagePackSerializationException($"Expected at least {CurrentFieldCount} fields. Only found {fieldcount}");
            }
            string name = reader.ReadString();
            string key = reader.IsNil ? name : reader.ReadString();
            RemoteServiceStates state = (RemoteServiceStates)reader.ReadByte();
            string? detail = reader.IsNil ? null : reader.ReadString();
            double? progress = reader.IsNil ? null : reader.ReadDouble();
            ObservableDictionary<string, ServiceState> children = new();
            int childCount = reader.ReadArrayHeader();
            for (int i = 0; i < childCount; i++)
            {
                ServiceState child = Deserialize(ref reader, options);
                children.Add(child.Key, child);
            }
            return new ServiceState(name, key, state, detail, progress, children);
        }

        public void Serialize(ref MessagePackWriter writer, ServiceState value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(CurrentFieldCount);
            writer.Write(value.Name);
            WriteNullOrString(ref writer, value.Key);
            writer.WriteUInt8((byte)value.AggregateState);
            WriteNullOrString(ref writer, value.Detail);
            WriteNullOrDouble(ref writer, value.Progress);
            writer.WriteArrayHeader(value.Children.Count);
            foreach (ServiceState child in value.Children.Values)
            {
                Serialize(ref writer, child, options);
            }
        }

        private static void WriteNullOrString(ref MessagePackWriter writer, string? value)
        {
            if (value is null)
            {
                writer.WriteNil();
            }
            else
            {
                writer.Write(value);
            }
        }

        private static void WriteNullOrDouble(ref MessagePackWriter writer, double? value)
        {
            if (value.HasValue)
            {
                writer.Write(value.Value);
            }
            else
            {
                writer.WriteNil();
            }
        }
    }
}
#nullable restore
