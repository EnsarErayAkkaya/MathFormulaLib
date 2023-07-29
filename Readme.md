# Math Expression Parser
## About
This library is a Math Expression Parser for C#. It is very powerful and strong tool.

## Features
- You can add variables to dynmaicly change values.
- You can use functions like sin, cos, pow and many more.
- You can convert Math Formulas to string.

## How to use it
```
// First create MathFormula instance
MathFormula formula = new MathFormula();
// Create a variable
formula.CreateVariable('x', 3);
// Calculate expression
formula.Calculate("((x*x) + 2)/ (x-1)");
```
