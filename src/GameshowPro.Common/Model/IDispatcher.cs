namespace GameshowPro.Common.Model;

/// <summary>
/// A wrapper intended to allow assemblies without a dependency on System.Windows to use the Dispatcher if required by clients that do.
/// </summary>
public interface IDispatcher
{ 
    bool CheckAccess();
    void BeginInvoke(Delegate method, params object[] args);
}
