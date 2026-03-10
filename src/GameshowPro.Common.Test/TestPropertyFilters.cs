using System.ComponentModel;
using GameshowPro.Common.Model;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestPropertyFilters
{
    private readonly PropertyChangeFilters _filters;
    private static bool _loggerAssigned = false;
    public TestPropertyFilters()
    {
        if (!_loggerAssigned)
        {
            PropertyChangeFilters.AssignLoggerFactory(NullLoggerFactory.Instance);
            _loggerAssigned = true;
        }
        _filters = new("test");
    }

    [TestMethod]
    public void ProperyChangeFilters_ShouldFire_WhenConfigPropertyIsEmpty()
    {
        int eventCount = 0;
        TestObject testObject = new("A", 1);
        _filters.AddFilter(FilterAction, new PropertyChangeCondition(testObject, string.Empty));
        Assert.AreEqual(1, eventCount);

        testObject.Property1 = "B";

        Assert.AreEqual(2, eventCount);

        testObject.Property2 = 2;
        Assert.AreEqual(3, eventCount);
        void FilterAction(object? sender, PropertyChangedEventArgs args)
        {
            eventCount++;
        }
    }

    [TestMethod]
    public void ProperyChangeFilters_ShouldFire_OnlyWhenPropertyMatches()
    {
        int eventCount = 0;
        TestObject testObject = new("A", 1);
        _filters.AddFilter(FilterAction, new PropertyChangeCondition(testObject, nameof(TestObject.Property1)));
        Assert.AreEqual(1, eventCount);

        testObject.Property1 = "B";

        Assert.AreEqual(2, eventCount);

        testObject.Property2 = 2;
        Assert.AreEqual(2, eventCount);
        void FilterAction(object? sender, PropertyChangedEventArgs args)
        {
            eventCount++;
        }
    }

    public class TestObject(string property1, int property2) : ObservableClass
    {
        public string Property1
        {
            get;
            set => SetProperty(ref field, value);
        } = property1;

        public int Property2
        {
            get;
            set => SetProperty(ref field, value);
        } = property2;
    }
}
