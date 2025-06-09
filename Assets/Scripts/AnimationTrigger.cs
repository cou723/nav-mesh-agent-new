public abstract class AnimationTrigger
{
    public abstract string TriggerName { get; init; }
    protected AnimationTrigger(string str)
    {
        TriggerName = str;
    }
}

public class Moving : AnimationTrigger
{
    public Moving() : base("Moving") { }
    public override string TriggerName { get; init; } = "Moving";
}

public class Eating : AnimationTrigger
{
    public Eating() : base("Eating") { }
    public override string TriggerName { get; init; } = "Eating";
}

public class Idle : AnimationTrigger
{
    public Idle() : base("Idle") { }
    public override string TriggerName { get; init; } = "Idle";
}

public class Dead : AnimationTrigger
{
    public Dead() : base("Dead") { }
    public override string TriggerName { get; init; } = "Dead";
}