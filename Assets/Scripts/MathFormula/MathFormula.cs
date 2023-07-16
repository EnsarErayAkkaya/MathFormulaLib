using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EEA.MathParser
{
    public class MathFormula
    {
        ExpressionParser parser = new ExpressionParser();

        public List<Variable> variables = new List<Variable>();

        public void CreateVariable(char name, double value = 0)
        {
            Variable variable = new Variable(name, value);

            variables.Add(variable);

            parser.SetVariable(variable);
        }

        public void UpdateVariable(char name, double value)
        {
            var variable =variables.FirstOrDefault(s => s.Name == name);
            
            variable.SetValue(value);

            parser.UpateVariable(variable);
        }

        public double Parse(string expression)
        {
            return parser.Parse(expression);
        }

        public void Reset()
        {
            variables.Clear();
            parser.Reset();
        }
    }
}
