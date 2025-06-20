using System;
using System.Collections.Generic;

namespace ProfuselyViolentProgression.SurgeSuppressor;

public class MultiThrottle<TTarget>
{
    private TimeSpan _interval;
    private Dictionary<TTarget, DateTime> _blockUntilTimes = new();

    public MultiThrottle(TimeSpan interval)
    {
        _interval = interval;
    }

    public MultiThrottle(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
    {
        _interval = new TimeSpan(days, hours, minutes, seconds, milliseconds);
    }

    public void ChangeInterval(int days = 0, int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
    {
        var oldInterval = _interval;
        _interval = new TimeSpan(days, hours, minutes, seconds, milliseconds);
        var offset = _interval - oldInterval;
        foreach (var (key, expireTime) in _blockUntilTimes)
        {
            _blockUntilTimes[key] = expireTime + offset;
        }
    }

    public bool CheckAndTrigger(TTarget target)
    {
        var hasTimer = _blockUntilTimes.ContainsKey(target);
        if (hasTimer && DateTime.Now < _blockUntilTimes[target])
        {
            return true;
        }
        _blockUntilTimes[target] = DateTime.Now.Add(_interval);
        return false;
    }

    public void Prune()
    {
        var now = DateTime.Now;
        foreach (var (key, expireTime) in _blockUntilTimes)
        {
            if (expireTime <= now)
            {
                _blockUntilTimes.Remove(key);
            }
        }
    }
    
}