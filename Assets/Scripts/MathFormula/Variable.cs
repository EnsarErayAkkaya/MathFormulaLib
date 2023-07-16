using UnityEngine;

namespace EEA.MathParser
{
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
    }
}