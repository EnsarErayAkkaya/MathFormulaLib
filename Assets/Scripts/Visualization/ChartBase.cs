using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using System.Linq;
using static UnityEditor.PlayerSettings;
using UnityEditor;
using System;

namespace EEA.Visualizer
{
    [System.Serializable]
    public struct AxisData
    {
        public float min;
        public float max;
        public float stepPercent;

        public string title;
        public string key;

        public AxisType axisType;
        public Color color;

        public int axisTickTextThinnes;
        public AxisOrientation orientation;
    }

    [System.Serializable]
    public struct PopulationConfig
    {
        public string xAxisKey;
        public string yAxisKey;

        public ChartPopulateType type;
        public List<Vector2> values;
        public Color color;
    }

    [System.Serializable]
    public enum AxisOrientation
    {
        Start = 0, Center = 1, End = 2
    }

    [System.Serializable]
    public enum AxisType
    {
        X_Axis, Y_Axis
    }

    [System.Serializable]
    public enum ChartPopulateType
    {
        Dots, Line, Column
    }

    public abstract class ChartBase: MonoBehaviour
    {
        [Header("Chart Rect")]
        [SerializeField] protected RectTransform chartRect;

        [Header("Prefabs")]
        [SerializeField] protected Image baseImage;
        [SerializeField] protected TextMeshProUGUI baseText;
        [SerializeField] protected Image baseCircle;

        [Header("Chart AxisDatas")]
        [SerializeField] protected AxisData[] axisDatas;

        [Header("Visualizing Methods")]
        [SerializeField] protected LineRenderer chartLine;

        protected List<Image> axisImages = new List<Image>();

        protected List<Dictionary<int, Image>> axisTicks = new List<Dictionary<int, Image>>();
        protected List<Dictionary<int, TextMeshProUGUI>> axisTexts = new List<Dictionary<int, TextMeshProUGUI>>();

        protected Dictionary<Image, Vector2> populatedColumns = new Dictionary<Image, Vector2>();
        protected Dictionary<Image, Vector2> populatedDots = new Dictionary<Image, Vector2>();
        protected List<Vector2> populatedLine = new List<Vector2>();

        protected List<float> stepDists = new List<float>();

        protected float currentSizeX;
        protected float currentSizeY;

        protected float halfWidth;
        protected float halfHeight;

        protected PopulationConfig currentPopulationConfig;

        // CHECKS CHART SIZE EVERY FRAME
        // UPDATES ONLY IF SIZE CHANGES 
        protected void Update()
        {
            OnUpdate();
        }

        protected abstract void OnUpdate();

        // SETUP CHART
        public abstract void SetUpChart();

        // CALLED WHEN CHART SIZE CHANGED
        public abstract void OnChartSizeChanged();

        public abstract void PopulateChart(PopulationConfig populationConfig);

        public abstract void UpdateChartPopulation(PopulationConfig populationConfig);

        protected abstract void CreateAxis(int axisIndex);

        #region SIZE AND POSITION UPDATE UTILITIES

        protected virtual void UpdateAxis(int axisIndex)
        {
            AxisData axisData = axisDatas[axisIndex];

            int stepCount = (int)Mathf.Round(1.0f / axisData.stepPercent);

            for (int i = 0; i <= stepCount; i++)
            {
                SetTickPositionAndSize(axisTicks[axisIndex][i], i, axisIndex);

                if ((i + 1) % axisData.axisTickTextThinnes == 0)
                {
                    SetTextPositionAndSize(axisTexts[axisIndex][i], i, axisIndex);
                }
            }
        }

        protected virtual void SetTickPositionAndSize(Image tick, int index, int axisIndex)
        {
            if (axisDatas[axisIndex].axisType == AxisType.X_Axis)
            {
                tick.rectTransform.localPosition = new Vector2(index * stepDists[axisIndex], 0) + AxisOriantationOffset(axisDatas[axisIndex]);

                tick.rectTransform.sizeDelta = new Vector2(3, halfWidth * .02f);
            }
            else if (axisDatas[axisIndex].axisType == AxisType.Y_Axis)
            {
                tick.rectTransform.localPosition = new Vector2(0, index * stepDists[axisIndex]) + AxisOriantationOffset(axisDatas[axisIndex]);

                tick.rectTransform.sizeDelta = new Vector2(halfHeight * .02f, 3);
            }

            tick.rectTransform.anchorMin = GetPopulatedItemAnchor();
            tick.rectTransform.anchorMax = GetPopulatedItemAnchor();
        }

        protected virtual void SetTextPositionAndSize(TextMeshProUGUI text, int index, int axisIndex)
        {
            if (axisDatas[axisIndex].axisType == AxisType.X_Axis)
            {
                text.rectTransform.localPosition = new Vector2(index * stepDists[axisIndex], halfHeight* -.03f) + AxisOriantationOffset(axisDatas[axisIndex]);
            }
            else if (axisDatas[axisIndex].axisType == AxisType.Y_Axis)
            {
                text.rectTransform.localPosition = new Vector2(halfWidth * -.03f, index * stepDists[axisIndex]) + AxisOriantationOffset(axisDatas[axisIndex]);
            }

            text.fontSize = halfWidth / 25.0f;
            text.rectTransform.anchorMin = GetPopulatedItemAnchor();
            text.rectTransform.anchorMax = GetPopulatedItemAnchor();
        }

        protected virtual void SetMainAxisesPositionAndSize()
        {
            for (int i = 0; i < axisDatas.Length; i++)
            {
                if (axisDatas[i].axisType == AxisType.X_Axis)
                {
                    axisImages[i].rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(axisDatas[i]);
                    axisImages[i].rectTransform.sizeDelta = new Vector2(currentSizeX, 5);
                }
                else
                {
                    axisImages[i].rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(axisDatas[i]);
                    axisImages[i].rectTransform.sizeDelta = new Vector2(5, currentSizeY);
                }

                axisImages[i].rectTransform.pivot = Vector2.zero;
                axisImages[i].rectTransform.anchorMin = GetPopulatedItemAnchor();
                axisImages[i].rectTransform.anchorMax = GetPopulatedItemAnchor();
            }
        }

        protected Vector2 AxisOriantationOffset(AxisData axisData)
        {
            if (axisData.axisType == AxisType.X_Axis)
                return new Vector2(0, halfHeight) * (int)axisData.orientation;
            else 
                return new Vector2(halfWidth, 0) * (int)axisData.orientation;
        }

        protected virtual void UpdateChartSizeValues()
        {
            currentSizeX = chartRect.rect.width;
            currentSizeY = chartRect.rect.height;

            halfWidth = currentSizeX * .5f;
            halfHeight = currentSizeY * .5f;

            stepDists.Clear();

            foreach (var item in axisDatas)
            {
                if (item.axisType == AxisType.X_Axis)
                {
                    stepDists.Add(currentSizeX * item.stepPercent);
                }
                else
                {
                    stepDists.Add(currentSizeY * item.stepPercent);
                }
            }

        }

        protected virtual Vector3 GetPopulatedItemPos(string xAxisKey, string yAxisKey, Vector2 coordinate)
        {
            AxisData xAxisData = axisDatas.FirstOrDefault(s => s.key == xAxisKey);
            AxisData yAxisData = axisDatas.FirstOrDefault(s => s.key == yAxisKey);

            float xPercent = coordinate.x.Remap01(xAxisData.min, xAxisData.max);

            float xPos = currentSizeX * xPercent;

            float yPercent = coordinate.y.Remap01(yAxisData.min, yAxisData.max);

            float yPos = currentSizeY * yPercent;

            return new Vector3(xPos, yPos);
        }

        protected virtual Vector3 ConvertAnchoredPosToCenteredPos(Vector3 pos)
        {
            return new Vector3(-halfWidth, -halfHeight) + pos;
        }

        protected virtual Vector2 GetPopulatedItemAnchor()
        {
            return Vector2.zero;
        }

        #endregion

        protected int GetAxisIndexByKey(string key)
        {
            for (int i = 0; i < axisDatas.Length; i++)
            {
                if (axisDatas[i].key == key)
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual Image DrawCircle(Color color, float width, Vector2 position)
        {
            var dot = Instantiate(baseCircle, chartRect);

            dot.rectTransform.sizeDelta = new Vector2(width, width);
            dot.rectTransform.localPosition = position;

            dot.rectTransform.anchorMin = GetPopulatedItemAnchor();
            dot.rectTransform.anchorMax = GetPopulatedItemAnchor();

            dot.color = color;

            return dot;
        }
    }
}