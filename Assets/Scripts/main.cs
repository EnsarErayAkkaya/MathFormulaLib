using EEA.MathParser;
using EEA.Visualizer;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
struct ExpressionHolder
{
    public string expression;
    public ScalingVariable[] variables;
}

public class main : MonoBehaviour
{
    [SerializeField] private ChartVisualizer chartVisualizer;
    [SerializeField] private ExpressionHolder expression;

    void Start()
    {
        MathFormula formula = new MathFormula();

        foreach (var item in expression.variables)
        {
            formula.CreateVariable(item.Variable.Name, item.Variable.Value);
        }

        List<Vector2> values = new List<Vector2>();

        bool variableReachedLimit = false;


        while (!variableReachedLimit)
        {
            float result = (float)formula.Parse(expression.expression);

            Debug.LogFormat("expr: " + expression.expression + ", result: " + result.ToString());

            values.Add(new Vector2((float)expression.variables[0].Variable.Value, result));

            // UPDATE VARIABLE WITH STEP 
            foreach (var item in expression.variables)
            {
                double val = item.Variable.Value + item.Step;

                item.Variable.SetValue(val);

                if (item.Variable.Value >= item.Max)
                {
                    variableReachedLimit = true;
                    break;
                }

                formula.UpdateVariable(item.Variable.Name, item.Variable.Value);
            }
        }

        chartVisualizer.SetUpChart();

        chartVisualizer.PopulateChart(ChartPopulateType.Line, values, Color.red);
    }
}
