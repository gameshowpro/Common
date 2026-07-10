using System.ComponentModel;
using GameshowPro.Common.Model;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestServiceState
{
    [TestMethod]
    public void ServiceState_UpdateChildren_ShouldTriggerAggregatorCorrectly()
    {
        // 1. Arrange: Create a parent state with one child.
        // The parent state uses GetAggregateState to calculate its aggregate state from children.
        var parent = new ServiceState("ParentKey", "ParentName", new[] { "ChildName" }, ServiceState.GetAggregateState);
        
        // Assert initial states (reflecting the current constructor behavior before we fix it)
        Assert.AreEqual(RemoteServiceStates.Disconnected, parent.AggregateState);
        Assert.IsNull(parent.Detail);
        Assert.IsNull(parent.Progress);

        // 2. Act: Prepare a child update where all properties change.
        var childUpdate = new ServiceState("ChildName", "ChildName", RemoteServiceStates.Connected, "All OK Details", 0.75);

        // Update parent children with this new child state.
        // This will update the existing child in place.
        parent.UpdateChildren(new[] { childUpdate });

        // 3. Assert: Verify if the parent state has aggregated correctly from the updated child.
        System.Diagnostics.Debug.WriteLine($"Parent state after update: State={parent.AggregateState}, Detail='{parent.Detail}', Progress={parent.Progress}");
        
        Assert.AreEqual(RemoteServiceStates.Connected, parent.AggregateState, "Parent state should aggregate to Connected");
        Assert.AreEqual("OK", parent.Detail, "Parent detail should aggregate to OK (according to GetAggregateState logic)");
        Assert.AreEqual(0.75, parent.Progress, "Parent progress should aggregate to child progress (0.75)");
    }
}
