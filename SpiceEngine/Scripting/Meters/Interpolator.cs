﻿using SpiceEngine.Game;
using System;

namespace SpiceEngine.Scripting.Meters
{
    public class Interpolator : ITick
    {
        public enum InterpolationTypes
        {
            Linear
        }

        public InterpolationTypes InterpolationType { get; private set; }
        public int Duration { get; private set; }
        public float StartValue { get; private set; }
        public float EndValue { get; private set; }
        public float Value { get; private set; }

        public Interpolator(int duration, float startValue, float endValue)
        {
            Duration = duration;
            StartValue = startValue;
            EndValue = endValue;
        }

        public void Tick()
        {

        }
    }
}