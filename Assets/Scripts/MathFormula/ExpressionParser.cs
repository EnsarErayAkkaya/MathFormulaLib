using System;
using System.Collections.Generic;
using System.Linq;

namespace EEA.MathParser
{
    public class ExpressionParser
    {
        private readonly Dictionary<char, int> precedence = new Dictionary<char, int>()
        {
            { '+', 1 },
            { '-', 1 },
            { '*', 2 },
            { '/', 2 },
            { '~', 2 }
        };

        private Dictionary<char, double> variables = new Dictionary<char, double>();

        public void SetVariable(Variable variable)
        {
            variables.Add(variable.Name, variable.Value);
        }

        public void UpateVariable(Variable variable)
        {
            variables[variable.Name] =  variable.Value;
        }

        public double Parse(string expression)
        {
            Stack<char> operators = new Stack<char>();
            Stack<double> operands = new Stack<double>();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (IsWhiteSpace(c))
                {
                    continue;
                }
                else if (Char.IsDigit(c) || c == '.')
                {
                    operands.Push(ReadNumber(expression, ref i));
                }
                else if (IsVariable(c))
                {
                    operands.Push(variables[c]);
                }
                else if (c == '(')
                {
                    operators.Push(c);
                }
                else if (c == ')')
                {
                    while (operators.Count > 0 && operators.Peek() != '(')
                    {
                        Evaluate(operators, operands);
                    }

                    if (operators.Count == 0 || operators.Peek() != '(')
                    {
                        throw new ArgumentException("Mismatched parentheses");
                    }

                    operators.Pop();
                }
                else if (IsOperator(c))
                {
                    // Check if the operator is unary minus
                    if (c == '-' && (i == 0 || expression[i - 1] == '('))
                    {
                        operators.Push('~');  // Use '~' to represent unary minus
                        continue;
                    }

                    while (operators.Count > 0 && operators.Peek() != '(' && precedence[c] <= precedence[operators.Peek()])
                    {
                        Evaluate(operators, operands);
                    }

                    operators.Push(c);
                }
                else if (c == '#')
                {
                    double val = EvaluateFunction(expression, ref i);
                    operands.Push(val);
                }
                else
                {
                    throw new ArgumentException("Invalid character: " + c);
                }
            }

            while (operators.Count > 0)
            {
                if (operators.Peek() == '(')
                {
                    throw new ArgumentException("Mismatched parentheses");
                }

                Evaluate(operators, operands);
            }

            if (operands.Count != 1)
            {
                throw new ArgumentException("Invalid expression");
            }

            return operands.Pop();
        }

        private double EvaluateFunction(string expression, ref int index)
        {
            string func = "";

            while (index + 1 < expression.Length && (expression[index + 1] != '('))
            {
                func += expression[index + 1];
                index++;
            }

            index++;

            Stack<char> operators = new Stack<char>();

            string innerExpression = "";
            List<double> parameters = new List<double>();

            while (index + 1 < expression.Length)
            {
                if (operators.Count == 0 && (expression[index + 1] == ')' || expression[index + 1] == ','))
                {
                    if (expression[index + 1] == ',')
                    {
                        index++;
                        while (index + 1 < expression.Length && expression[index + 1] != ')')
                        {
                            if (IsWhiteSpace(expression[index + 1]))
                            {
                                index++;
                                continue;
                            }

                            if (expression[index + 1] == '.' || char.IsDigit(expression[index + 1]))
                            {
                                index++;
                                parameters.Add(ReadNumber(expression, ref index));
                            }
                            else if (IsVariable(expression[index + 1]))
                            {
                                parameters.Add(variables[expression[index + 1]]);
                                index++;
                            }
                        }
                    }

                    break;
                }

                innerExpression += expression[index + 1];

                if (expression[index + 1] == '(')
                {
                    operators.Push(expression[index + 1]);
                }
                else if (expression[index + 1] == ')')
                {
                    operators.Pop();
                }

                index++;
            }

            index++;

            double innerValue = (innerExpression.Length > 0) ? Parse(innerExpression) : 0;

            switch (func)
            {
                case "abs":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: abs" );
                    }
                    return Math.Abs(innerValue);
                    
                case "sqrt":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: sqrt");
                    }

                    return Math.Sqrt(innerValue);

                case "pow":
                    if (parameters.Count != 1)
                    {
                        throw new ArgumentException("Invalid functions usage, func: pow");
                    }

                    return Math.Pow(innerValue, parameters[0]);

                case "sin":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: sin");
                    }

                    return Math.Sin(innerValue);

                case "cos":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: cos");
                    }

                    return Math.Cos(innerValue);
                
                case "tan":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: tan");
                    }

                    return Math.Tan(innerValue);

                case "cotan":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: cot");
                    }

                    return 1/Math.Tan(innerValue);

                case "ceil":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: ceil");
                    }

                    return Math.Ceiling(innerValue);

                case "floor":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: floor");
                    }

                    return Math.Floor(innerValue);

                case "clamp":
                    if (parameters.Count != 2)
                    {
                        throw new ArgumentException("Invalid functions usage, func: clamp");
                    }

                    return Math.Clamp(innerValue, parameters[0], parameters[1]);

                case "log":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: log");
                    }

                    return Math.Log(innerValue);

                case "log10":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: Log10");
                    }

                    return Math.Log10(innerValue);

                case "min":
                    if (parameters.Count != 1)
                    {
                        throw new ArgumentException("Invalid functions usage, func: min");
                    }

                    return Math.Min(innerValue, parameters[0]);

                case "max":
                    if (parameters.Count != 1)
                    {
                        throw new ArgumentException("Invalid functions usage, func: max");
                    }

                    return Math.Max(innerValue, parameters[0]);

                case "round":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: round");
                    }

                    return Math.Round(innerValue);

                case "sign":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: sign");
                    }

                    return Math.Sign(innerValue);

                case "trunc":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: trunc");
                    }

                    return Math.Truncate(innerValue);
                case "pi":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: pi");
                    }

                    return Math.PI;

                case "e":
                    if (parameters.Count > 0)
                    {
                        throw new ArgumentException("Invalid functions usage, func: e");
                    }

                    return Math.E;

                default:
                    return 0;
            }
        }

        private void Evaluate(Stack<char> operators, Stack<double> operands)
        {
            char op = operators.Pop();

            if (op == '~') // Unary minus
            {
                if (operands.Count < 1)
                {
                    throw new ArgumentException("Invalid expression");
                }

                double operand = operands.Pop();
                operands.Push(-operand);
                return;
            }

            if (operands.Count < 2)
            {
                throw new ArgumentException("Invalid expression");
            }

            double b = operands.Pop();
            double a = operands.Pop();
            double result = 0;

            switch (op)
            {
                case '+':
                    result = a + b;
                    break;
                case '-':
                    result = a - b;
                    break;
                case '*':
                    result = a * b;
                    break;
                case '/':
                    if (b == 0)
                    {
                        throw new DivideByZeroException();
                    }
                    result = a / b;
                    break;
            }

            operands.Push(result);
        }

        private double ReadNumber(string expression, ref int index)
        {
            char c = expression[index];
            string operand = c.ToString();

            while (index + 1 < expression.Length && (Char.IsDigit(expression[index + 1]) || expression[index + 1] == '.'))
            {
                operand += expression[index + 1];
                index++;
            }

            if (!Double.TryParse(operand, out double value))
            {
                throw new ArgumentException("Invalid operand: " + operand);
            }

            return value;
        }

        private bool IsOperator(char c)
        {
            return precedence.ContainsKey(c);
        }

        private bool IsVariable(char c)
        {
            return variables.ContainsKey(c);
        }

        private bool IsWhiteSpace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }

        public void Reset()
        {
            variables.Clear();
        }
    }
}