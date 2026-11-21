namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestIncomingTrigger
{
    private sealed class TestSetting : IncomingTriggerSetting { }

    private sealed class TestTrigger : IncomingTrigger
    {
        public TestTrigger() : base(new TestSetting(), null, NullLogger.Instance) { }
    }

    [TestMethod]
    public void IsTied_RaisesPropertyChanged_OnlyWhenValueChanges()
    {
        TestTrigger trigger = new();
        List<string?> changed = [];
        trigger.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        trigger.IsTied = true;
        trigger.IsTied = true;

        string?[] expected = [nameof(IncomingTrigger.IsTied)];
        Assert.AreSequenceEqual(expected, changed);
        Assert.IsTrue(trigger.IsTied);
    }

    [TestMethod]
    public void IsTied_IsKept_WhenOrdinalIsSetToAnotherPlace()
    {
        TestTrigger trigger = new() { Ordinal = 0, IsTied = true };
        trigger.Ordinal = 1;
        Assert.IsTrue(trigger.IsTied);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(null)]
    public void IsTied_IsCleared_WhenOrdinalIsSetToNullOrNegative(int? ordinal)
    {
        TestTrigger trigger = new() { Ordinal = 0, IsTied = true };

        trigger.Ordinal = ordinal;

        Assert.IsFalse(trigger.IsTied);
    }
}
