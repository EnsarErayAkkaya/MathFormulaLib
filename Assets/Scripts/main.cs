using EEA.MathParser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct ExpressionHolder
{
    public string expression;
    public Variable[] variables;
}

public class main : MonoBehaviour
{
    [SerializeField] private ExpressionHolder[] expressions;

    void Start()
    {
        MathFormula formula = new MathFormula();

        foreach (var item in expressions)
        {
            foreach (var variable in item.variables)
            {
                formula.SetVariable(variable.Name, variable.Value);
            }

            Debug.LogFormat("expr: " + item.expression + ", result: " + formula.Parse(item.expression).ToString());

            formula.Reset();
        }
    }
}
