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
## How to update variables
```
formula.UpdateVariable('x', 5);

formula.Calculate("((x*x) + 2)/ (x-1)");
```

## How to reset variables
```
// To reset variables yo need to call Reset method
formula.Reset();
// Now you can add new variables
formula.CreateVariable('y', 5);
formula.Calculate("((y*y) + 2)/ (y-1)");
```