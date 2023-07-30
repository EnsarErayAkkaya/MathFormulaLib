using EEA.MathParser;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEditor.SearchService;

namespace EEA.MathParser.Demo
{
    public class DemoScript : MonoBehaviour
    {
        [Header("Referances")]
        [SerializeField] private TMP_InputField expressionInput;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI errorText;
        [SerializeField] private RectTransform variablesParent;

        [Header("Prefabs")]
        [SerializeField] private VariableInputUI variableInputPrefab;

        private char[] variableNames = { 'x', 'y', 'z', 'k', 'l' };

        private List<VariableInputUI> currentVariables = new List<VariableInputUI>();

        private int maxVariableCount = 5;

        private MathFormula formula = new MathFormula();

        void Start()
        {
            try
            {
                CreateVariableUI();
                //formula.CreateVariable('x', Convert.ToDouble(((TextMeshProUGUI)variableInput.placeholder).text));

                resultText.text = formula.Calculate(((TextMeshProUGUI)expressionInput.placeholder).text).ToString();
                errorText.text = "No Error";
            }
            catch (Exception e)
            {
                errorText.text = e.Message;
                Debug.LogException(e);
            }
        }

        public void OnApply()
        {
            try
            {
                foreach (var item in currentVariables)
                {
                    formula.UpdateVariable(item.label.text[0], Convert.ToDouble(item.input.text));    
                }

                resultText.text = formula.Calculate(expressionInput.text).ToString();
                errorText.text = "No Error";
            }
            catch (Exception e)
            {
                errorText.text = e.Message;
                Debug.LogException(e);
            }
        }

        public void CreateVariableUI()
        {
            if (currentVariables.Count >= maxVariableCount) return;

            var obj = Instantiate(variableInputPrefab, variablesParent);

            obj.input.text = "1";

            obj.label.text = variableNames[currentVariables.Count].ToString();

            currentVariables.Add(obj);

            formula.CreateVariable(obj.label.text[0],1);
        }

        public void RemoveVariableUI()
        {
            var obj = currentVariables[currentVariables.Count - 1];

            Destroy(obj.gameObject);

            currentVariables.Remove(obj);
        }
    }
}