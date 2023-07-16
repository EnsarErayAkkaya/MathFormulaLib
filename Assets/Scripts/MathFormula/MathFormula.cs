using System.Collections.Generic;
using System.Diagnostics;

namespace EEA.MathParser
{
    public class MathFormula
    {
        ExpressionParser parser = new ExpressionParser();

        public List<Variable> variables = new List<Variable>();

        public void SetVariable(char name, double value)
        {
            Variable variable = new Variable(name, value);

            parser.SetVariable(variable);
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
