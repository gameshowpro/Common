namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestPropertyFilters
{
    private readonly PropertyChangeFilters _filters;
    private static bool s_loggerAssigned;
    public TestPropertyFilters()
    {
        if (!s_loggerAssigned)
        {
            PropertyChangeFilters.AssignLoggerFactory(NullLoggerFactory.Instance);
            s_loggerAssigned = true;
        }
        _filters = new("test");
    }

    [TestMethod]
    public void ProperyChangeFilters_ShouldFire_WhenConfigPropertyIsEmpty()
    {
        int eventCount = 0;
        TestObject testObject = new("A", 1);
        _ = _filters.AddFilter(FilterAction, new PropertyChangeCondition(testObject, string.Empty));
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
        _ = _filters.AddFilter(FilterAction, new PropertyChangeCondition(testObject, nameof(TestObject.Property1)));
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
