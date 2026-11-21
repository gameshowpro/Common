namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestServiceState
{
    [TestMethod]
    public void ServiceState_UpdateChildren_ShouldTriggerAggregatorCorrectly()
    {
        // 1. Arrange: Create a parent state with one child.
        // The parent state uses GetAggregateState to calculate its aggregate state from children.
        ServiceState parent = new("ParentKey", "ParentName", ["ChildName"], ServiceState.GetAggregateState);

        // Assert initial states (reflecting the current constructor behavior before we fix it)
        Assert.AreEqual(RemoteServiceStates.Disconnected, parent.AggregateState);
        Assert.IsNull(parent.Detail);
        Assert.IsNull(parent.Progress);

        // 2. Act: Prepare a child update where all properties change.
        ServiceState childUpdate = new("ChildName", "ChildName", RemoteServiceStates.Connected, "All OK Details", 0.75);

        // Update parent children with this new child state.
        // This will update the existing child in place.
        parent.UpdateChildren([childUpdate]);

        // 3. Assert: Verify if the parent state has aggregated correctly from the updated child.
        System.Diagnostics.Debug.WriteLine($"Parent state after update: State={parent.AggregateState}, Detail='{parent.Detail}', Progress={parent.Progress}");

        Assert.AreEqual(RemoteServiceStates.Connected, parent.AggregateState, "Parent state should aggregate to Connected");
        Assert.AreEqual("OK", parent.Detail, "Parent detail should aggregate to OK (according to GetAggregateState logic)");
        Assert.AreEqual(0.75, parent.Progress, "Parent progress should aggregate to child progress (0.75)");
    }

    /// <summary>
    /// Regression: hashing a state that has children threw InvalidCastException (DictionaryEntry to KeyValuePair), which
    /// crashed WPF hosts as soon as UI automation hashed a ServiceState shown in an ItemsControl.
    /// </summary>
    [TestMethod]
    public void ServiceState_GetHashCode_WithChildren_MatchesEquals()
    {
        ServiceState first = new("Parent", ["ChildA", "ChildB"]);
        ServiceState second = new("Parent", ["ChildA", "ChildB"]);
        ServiceState different = new("Parent", ["ChildA", "ChildC"]);

        Assert.AreEqual(first, second, "States with the same values and children should be equal");
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode(), "Equal states should have equal hash codes");
        Assert.AreNotEqual(first.GetHashCode(), different.GetHashCode(), "A different child key should change the hash code");
    }
}
