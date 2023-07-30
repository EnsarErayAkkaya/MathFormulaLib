using System.Collections.Generic;
using UnityEngine;

namespace EEA.MathParser
{
    public class MathFormula
    {
        ExpressionParser parser = new ExpressionParser();
        private string expression;

        public Dictionary<char, double> Variables => parser.Variables;
        public string Expression => expression;

        /// <summary>
        /// Creates a variable with name and value and adds it to formula. If variable already exist updates it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void CreateVariable(char name, double value = 0)
        {
            if (parser.Variables.ContainsKey(name))
            {
                //UnityEngine.Debug.Log("Variable with name: " + name + " already exist. Updating instead.");
                UpdateVariable(name, value);
                return;
            }

            Variable variable = new Variable(name, value);

            parser.SetVariable(variable);
        }

        /// <summary>
        /// Adds the variable to formula.  If variable already exist updates it.
        /// </summary>
        /// <param name="variable"></param>
        public void CreateVariable(Variable variable)
        {
            if (parser.Variables.ContainsKey(variable.Name))
            {
                //UnityEngine.Debug.Log("Variable with name: " + variable.Name + " already exist. Updating instead.");
                UpdateVariable(variable.Name, variable.Value);
                return;
            }

            parser.SetVariable(variable);
        }

        /// <summary>
        /// Update a variable with name and value.  If variable doesn't exist create it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void UpdateVariable(char name, double value)
        {
            if (!parser.Variables.ContainsKey(name))
            {
                //UnityEngine.Debug.Log("Variable with name: " + name + " doesn't exist. Creating instead.");
                CreateVariable(name, value);
                return;
            }

            parser.UpateVariable(name, value);
        }

        /// <summary>
        /// Calculates the given expression with variables set.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public double Calculate(string expression)
        {
            this.expression = expression;
            return parser.Parse(expression);
        }

        /// <summary>
        /// Calculates the expression with given variable.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public double Calculate(string expression, Variable variable)
        {
            this.expression = expression;

            CreateVariable(variable);

            return parser.Parse(expression);
        }

        /// <summary>
        /// Resets the math formula. Best practice to call when formula/expression changed.
        /// </summary>
        public void Reset()
        {
            parser.Reset();
        }

        public override string ToString()
        {
            string variablesString = "";

            int i = 0;

            foreach (var item in parser.Variables)
            {
                i++;
                variablesString += "\tName: " + item.Key + " , Value: " + item.Value;

                if (i < parser.Variables.Count)
                {
                    variablesString += ",\n";
                }
            }

            return "Expression: " + expression + "\nVariables:\n" + variablesString;
        }
    }
}
