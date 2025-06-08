using UnityEngine;

public class Health
{
    public float Value { get; private set; }
    public float MaxValue { get; } = 200f;

    public Health(float initialValue, float maxValue = 200f)
    {
        MaxValue = maxValue;
        Value = Mathf.Clamp(initialValue, 0, MaxValue);
    }

    public void Add(float amount)
    {
        Debug.Log($"Health updated: {Value}+{amount} = {Value+amount}");
        Value = Mathf.Clamp(Value + amount, 0, 200f);
    }

    public void Consume(float amount)
    {
        Value = Mathf.Clamp(Value - amount, 0, 200f);
    }
}