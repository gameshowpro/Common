using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameshowPro.Common.Model;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestPropertyFilters
{
    private readonly PropertyChangeFilters _filters;

    public TestPropertyFilters()
    {
        PropertyChangeFilters.AssignLoggerFactory(NullLoggerFactory.Instance);
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
        private string _property1 = property1;
        public string Property1
        {
            get => _property1;
            set => SetProperty(ref _property1, value);
        }
        private int _property2 = property2;
        public int Property2
        {
            get => _property2;
            set => SetProperty(ref _property2, value);
        }
    }
}
