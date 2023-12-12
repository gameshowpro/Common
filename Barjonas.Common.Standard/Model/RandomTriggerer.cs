using System.Collections.Concurrent;


namespace Barjonas.Common.Model;
public class RandomTriggerer
{
    private const int ReportVersion = 1;
    private readonly TimeSpan _maxMaxRandomTriggerTime = TimeSpan.FromSeconds(4);
    private record TriggerTestStep(TimeSpan Time, EdgeReport? Report);
    public event Action<EdgeReport>? ReportCallback;
    public event Action<EdgeReport[]>? Finished;
    private readonly ConcurrentQueue<TriggerTestStep> _testSteps = new();
    private readonly Timer _testStepper;
    private readonly EdgeReport[] _startingState;
    private static readonly Random s_rnd = new();
    private record InputTime(int Input, TimeSpan FireTime, TimeSpan ReportedTime, bool Rising);
    public RandomTriggerer(EdgeReport[] startingState, TriggerRandomRequest triggerRandomRequest, out bool success, out string? message)
    {
        _testStepper = new(DoTestStep);
        _startingState = startingState;
        if (triggerRandomRequest.MaximumTime > _maxMaxRandomTriggerTime)
        {
            success = false;
            message = $"Maximum time is greater than maximum of {triggerRandomRequest.MaximumTime}";
            return;
        }
        if (triggerRandomRequest.Inputs?.Any() != true)
        {
            success = false;
            message = "No input list was supplied";
            return;
        }
        if (triggerRandomRequest.MinimumTime > triggerRandomRequest.MaximumTime)
        {
            success = false;
            message = "Minimum time is greater than maximum time";
            return;
        }
        if (!_testSteps.IsEmpty)
        {
            success = false;
            message = "Random trigger sequence is already in progress";
            return;
        }
        List<InputTime> inputTimes = [];

        foreach (TriggerDefinition i in triggerRandomRequest.Inputs)
        {
            TimeSpan time = TimeSpan.FromMilliseconds((s_rnd.NextDouble() * (triggerRandomRequest.MaximumTime - triggerRandomRequest.MinimumTime).TotalMilliseconds)) + triggerRandomRequest.MinimumTime;
            inputTimes.Add(new InputTime(i.Input, time, time, i.RisingEdge));
            inputTimes.Add(new InputTime(i.Input, time + TimeSpan.FromSeconds(0.2), time, !i.RisingEdge));
        }
        TimeSpan previous = TimeSpan.Zero;
        int ordinal = 0;
        foreach (InputTime inputTime in inputTimes.OrderBy(i => i.FireTime))
        {
            EdgeReport newReport = new(ReportVersion, inputTime.Input, inputTime.Rising ? ordinal++ : null, inputTime.ReportedTime, inputTime.Rising, true, null);

            _testSteps.Enqueue(new TriggerTestStep(inputTime.FireTime - previous, newReport));
            previous = inputTime.FireTime;
        }
        AddRestoreStep(previous);
        SetTimerForNextStep();

        success = false;
        message = null;
    }

    private void AddRestoreStep(TimeSpan lastStepTime)
    {
        if (!_testSteps.IsEmpty)
        {
            _testSteps.Enqueue(new TriggerTestStep(lastStepTime + TimeSpan.FromSeconds(1), null));
        }
    }

    private void DoTestStep(object? state)
    {
        if (_testSteps.TryDequeue(out TriggerTestStep? step))
        {
            if (step.Report is null)
            {
                Finished?.Invoke(_startingState);
            }
            else
            {
                ReportCallback?.Invoke(step.Report);
                SetTimerForNextStep();
            }
        }
    }

    private void SetTimerForNextStep()
    {
        if (_testSteps.TryPeek(out TriggerTestStep? nextStep))
        {
            _testStepper.Change(nextStep.Time, Timeout.InfiniteTimeSpan);
        }
        else
        {
            _testStepper.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }
    }
}

