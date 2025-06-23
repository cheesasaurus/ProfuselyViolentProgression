using System;

namespace ProfuselyViolentProgression.FrostDashFreezeFix;

[Flags]
public enum InflictionFlags : ulong
{
    None = 0,
    Leech = 1,
    Ignite = 1 << 1,
    Condemn = 1 << 2,
    Weaken = 1 << 3,
    Chill = 1 << 4,
    Static = 1 << 5,
}


public class InflictionFlagsState
{
    public InflictionFlags Value;

    public bool Leech { get => IsSet(InflictionFlags.Leech); }
    public bool Ignite { get => IsSet(InflictionFlags.Ignite); }
    public bool Condemn { get => IsSet(InflictionFlags.Condemn); }
    public bool Weaken { get => IsSet(InflictionFlags.Weaken); }
    public bool Chill { get => IsSet(InflictionFlags.Chill); }
    public bool Static { get => IsSet(InflictionFlags.Static); }

    public InflictionFlagsState(InflictionFlags flags = InflictionFlags.None)
    {
        Value = flags;
    }

    public void Set(InflictionFlags flags)
    {
        Value |= flags;
    }

    public bool IsSet(InflictionFlags flags)
    {
        return (Value & flags) != 0;
    }

    public override string ToString()
    {
        return Value.ToString();
    }    

}

