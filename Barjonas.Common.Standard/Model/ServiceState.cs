using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
#nullable enable
namespace Barjonas.Common.Model
{
    /// <summary>
    /// Record representing the state of a service.
    /// </summary>
    /// <remarks>
    /// Useful for transferring state details between services.
    /// Subclasses may extend ServiceState to add a list of sub-services to give a more detailed report of the current state, e.g. Sounds are OK, GPI card not found.
    /// </remarks>
    /// <param name="Progress">Any progress associated with the state. If finished, value should be 1. If not started, value should be 0. If indeterminate, value should be null. This is represented by a constant spinner.</param>
    public record ServiceState(string Name, RemoteServiceStates AggregateState, string Detail, double? Progress, ImmutableList<ServiceState>? Children)
    {
        /// <summary>
        /// Create service state without any children.
        /// </summary>
        public ServiceState(string Name, RemoteServiceStates AggregateState, string Detail, double? Progress) : this(Name, AggregateState, Detail, Progress, (ImmutableList<ServiceState>?)null)
        { }
        /// <summary>
        /// Create service state with paramater array of children.
        /// </summary>
        public ServiceState(string Name, RemoteServiceStates AggregateState, string Detail, double? Progress, params ServiceState[] children) : this(Name, AggregateState, Detail, Progress, children.ToImmutableList())
        { }

        /// <summary>
        /// Create a service state which automatically aggregates its children.
        /// </summary>
        /// <param name="Name">The name of the service state.</param>
        /// <param name="children">The children from which the service should aggregate its own state.</param>
        public ServiceState(string Name, params ServiceState[] children) : this(Name, GetAggregateState(children), GetAggregateDetail(children), GetAggregateProgress(children), children.ToImmutableList())
        { }

        /// <summary>
        /// Create a service state which automatically aggregates its children.
        /// </summary>
        /// <param name="Name">The name of the service state.</param>
        /// <param name="children">The children from which the service should aggregate its own state.</param>
        public ServiceState(string Name, ImmutableList<ServiceState> children) : this(Name, GetAggregateState(children), GetAggregateDetail(children), GetAggregateProgress(children), children)
        { }

        /// <summary>
        /// Create a service state which automatically aggregates its children.
        /// </summary>
        /// <param name="Name">The name of the service state.</param>
        /// <param name="customDetail">An explicit detail string which, if not null, will override the automatically-generated detail string.</param>
        /// <param name="children">The children from which the service should aggregate its own state.</param>
        public ServiceState(string Name, string? customDetail, params ServiceState[] children) : this(Name, GetAggregateState(children), customDetail ?? GetAggregateDetail(children), GetAggregateProgress(children), children.ToImmutableList())
        { }

        /// <summary>
        /// Create a service state which automatically aggregates its children.
        /// </summary>
        /// <param name="Name">The name of the service state.</param>
        /// <param name="customDetail">An explicit detail string which, if not null, will override the automatically-generated detail string.</param>
        /// <param name="children">The children from which the service should aggregate its own state.</param>
        public ServiceState(string Name, string? customDetail, ImmutableList<ServiceState> children) : this(Name, GetAggregateState(children), customDetail ?? GetAggregateDetail(children), GetAggregateProgress(children), children)
        { }

        public static RemoteServiceStates GetAggregateState(IEnumerable<ServiceState> states)
        {
            if (states.All(s => s.AggregateState == RemoteServiceStates.Connected))
            {
                return RemoteServiceStates.Connected;
            }
            else if (states.All(s => s.AggregateState == RemoteServiceStates.Disconnected))
            {
                return RemoteServiceStates.Disconnected;
            }
            else
            {
                return RemoteServiceStates.Warning;
            }
        }

        public static string GetAggregateDetail(IEnumerable<ServiceState> states)
        {
            List<ServiceState> notConnected = states.Where(s => s.AggregateState != RemoteServiceStates.Connected).ToList();
            if (notConnected.Any())
            {
                if (notConnected.Count == 1)
                {
                    return notConnected[0].Detail;
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

        public static double? GetAggregateProgress(IEnumerable<ServiceState> states)
        {
            if (states.Any(s => s.Progress == null))
            {
                return null;
            }
            else
            {
                return states.Average(s => s.Progress);
            }
        }
    }
}
#nullable restore
