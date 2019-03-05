﻿using System;

namespace SpiceEngine.Scripting.Meters
{
    public class Meter
    {
        public int Value { get; private set; }
        public int TriggerValue { get; set; }
        public bool ResetOnTrigger { get; set; }
        public bool IsTriggered => Value >= TriggerValue;

        public event EventHandler<MeterTriggeredEventArgs> Triggered;

        public Meter() { }
        public Meter(int triggerValue) => TriggerValue = triggerValue;

        public void Increment(int amount = 1)
        {
            Value += amount;

            if (IsTriggered)
            {
                Trigger();
            }
        }

        public void Decrement(int amount = 1) => Value -= amount;

        public void Trigger()
        {
            if (ResetOnTrigger)
            {
                Reset();
            }

            Triggered?.Invoke(this, new MeterTriggeredEventArgs());
        }

        public void Reset() => Value = 0;
    }
}
