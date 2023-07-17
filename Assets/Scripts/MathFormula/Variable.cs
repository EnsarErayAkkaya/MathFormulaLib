using Palmmedia.ReportGenerator.Core.Common;
using UnityEngine;

namespace EEA.MathParser
{
    [System.Serializable]
    public class ScalingVariable : Variable
    {        
        [SerializeField] private double max;

        [SerializeField] private double step;

        public double Max { get => max; set => max = value; }
        public double Step { get => step; set => step = value; }

        public ScalingVariable(char _name, double _value, double _max, double _step) : base(_name, _value)
        {
            max = _max;
            step = _step;
        }

        public static new ScalingVariable FromJson(string json)
        {
            return JsonUtility.FromJson<ScalingVariable>(json);
        }
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

        public static Variable FromJson(string json)
        {
            return JsonUtility.FromJson<Variable>(json);
        }
    }
}