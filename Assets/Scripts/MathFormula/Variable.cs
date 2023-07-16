using UnityEngine;

namespace EEA.MathParser
{
    [System.Serializable]
    public class ScalingVariable
    {
        [SerializeField] private Variable variable;
        
        [SerializeField] private double max;

        [SerializeField] private double step;

        public Variable Variable { get => variable; set => variable = value; }
        public double Max { get => max; set => max = value; }
        public double Step { get => step; set => step = value; }
    }

    [System.Serializable]
    public class Variable
    {
        [SerializeField] private char name;

        [SerializeField] private double value;

        public char Name => name;
        public double Value => value;

        public Variable() { }

        public Variable(char _name, double _value)
        {
            name = _name;
            value = _value;
        }

        public void SetValue(double val) => value = val;
    }
}