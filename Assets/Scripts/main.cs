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
    [SerializeField] private ChartBase chartVisualizer;
    [SerializeField] private ExpressionHolder expression;

    MathFormula formula = new MathFormula();

    void Start()
    {
        foreach (var item in expression.variables)
        {
            formula.CreateVariable(item.Name, item.Value);
        }

        List<Vector2> values = new List<Vector2>();

        bool variableReachedLimit = false;

        while (!variableReachedLimit)
        {
            float result = (float)formula.Parse(expression.expression);

            //Debug.LogFormat("expr: " + expression.expression + ", result: " + result.ToString());

            values.Add(new Vector2((float)expression.variables[0].Value, result));

            // UPDATE VARIABLE WITH STEP 
            foreach (var item in expression.variables)
            {
                double val = item.Value + item.Step;

                item.SetValue(val);

                if (item.Value >= item.Max)
                {
                    variableReachedLimit = true;
                    break;
                }

                formula.UpdateVariable(item.Name, item.Value);
            }
        }

        chartVisualizer.SetUpChart();

        PopulationConfig config;
        config.type = ChartPopulateType.Column;
        config.values = values;
        config.color = Color.red;
        config.xAxisKey = "mainX";
        config.yAxisKey = "mainY";

        chartVisualizer.PopulateChart(config);
    }
}
