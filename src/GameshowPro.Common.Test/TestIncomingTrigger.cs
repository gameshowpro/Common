using GameshowPro.Common.Model;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameshowPro.Common.Test;

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
        var trigger = new TestTrigger();
        var changed = new List<string?>();
        trigger.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        trigger.IsTied = true;
        trigger.IsTied = true;

        CollectionAssert.AreEqual(new[] { nameof(IncomingTrigger.IsTied) }, changed);
        Assert.IsTrue(trigger.IsTied);
    }

    [TestMethod]
    public void IsTied_IsKept_WhenOrdinalIsSetToAnotherPlace()
    {
        var trigger = new TestTrigger { Ordinal = 0 };
        trigger.IsTied = true;
        trigger.Ordinal = 1;
        Assert.IsTrue(trigger.IsTied);
    }

    [TestMethod]
    [DataRow(-1)]
    [DataRow(null)]
    public void IsTied_IsCleared_WhenOrdinalIsSetToNullOrNegative(int? ordinal)
    {
        var trigger = new TestTrigger { Ordinal = 0 };
        trigger.IsTied = true;

        trigger.Ordinal = ordinal;

        Assert.IsFalse(trigger.IsTied);
    }
}
