using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using System.Linq;
using static UnityEditor.PlayerSettings;
using UnityEditor;

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
        Start = -1, Center = 0, End = 1
    }

    [System.Serializable]
    public enum AxisType
    {
        X_Axis, Y_Axis
    }

    [System.Serializable]
    public enum ChartPopulateType
    {
        Dots, Line
    }

    public class ChartBase: MonoBehaviour
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

        protected virtual void OnUpdate()
        {
            if ((chartRect.rect.width != currentSizeX) || (chartRect.rect.height != currentSizeY))
            {
                OnChartSizeChanged();
            }
        }

        public virtual void SetUpChart()
        {
            foreach (var item in axisDatas)
            {
                axisImages.Add(Instantiate(baseImage, chartRect));
            }

            UpdateChartSizeValues();

            SetMainAxisesPositionAndSize();

            for (int i = 0; i < axisDatas.Length; i++)
            {
                axisTicks.Add(new Dictionary<int, Image>());
                axisTexts.Add(new Dictionary<int, TextMeshProUGUI>());
                CreateAxis(i);
            }
        }

        public virtual void OnChartSizeChanged()
        {
            UpdateChartSizeValues();

            SetMainAxisesPositionAndSize();

            for (int i = 0; i < axisDatas.Length; i++)
            {
                UpdateAxis(i);
            }

            if (currentPopulationConfig.type == ChartPopulateType.Dots)
            {
                foreach (var dot in populatedDots)
                {
                    dot.Key.rectTransform.localPosition = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, dot.Value);
                }
            }
            else if (currentPopulationConfig.type == ChartPopulateType.Line)
            {
                int i = 0;
                foreach (var lineCoordinate in populatedLine)
                {
                    chartLine.SetPosition(i++, GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, lineCoordinate));
                }
            }
        }

        public virtual void PopulateChart(PopulationConfig populationConfig)
        {
            currentPopulationConfig = populationConfig;

            if (populationConfig.type == ChartPopulateType.Dots)
            {
                foreach (var item in populationConfig.values)
                {
                    Vector2 pos = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, item);

                    if (chartRect.rect.Contains(pos))
                    {
                        Image dot = DrawCircle(populationConfig.color, currentSizeX * .015f, pos);

                        populatedDots.Add(dot, item);
                    }
                }
            }
            else if (populationConfig.type == ChartPopulateType.Line)
            {
                chartLine.startColor = populationConfig.color;
                chartLine.endColor = populationConfig.color;

                List<Vector2> result = new List<Vector2>();

                foreach (var item in populationConfig.values)
                {
                    Vector2 pos = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, item);

                    if (chartRect.rect.Contains(pos))
                    {
                        populatedLine.Add(item);
                        result.Add(pos);
                    }
                }

                chartLine.positionCount = result.Count;

                int i = 0;
                foreach (var item in result)
                {
                    chartLine.SetPosition(i++, item);
                }
            }
        }

        public virtual void UpdateChartPopulation(PopulationConfig populationConfig)
        {
            currentPopulationConfig = populationConfig;

            if (populationConfig.type == ChartPopulateType.Dots)
            {
                int i;
                if (populationConfig.values.Count > populatedDots.Count)
                {
                    int diff = populationConfig.values.Count - populatedDots.Count;
                    for (i = 0; i < diff; i++)
                    {
                        Image dot = DrawCircle(populationConfig.color, currentSizeX * .015f, GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, Vector3.zero));

                        populatedDots.Add(dot, Vector3.zero);
                    }
                }
                else if (populatedDots.Count > populationConfig.values.Count)
                {
                    int diff = populatedDots.Count - populationConfig.values.Count;
                    for (i = 0; i < diff; i++)
                    {
                        var dot = populatedDots.Last();

                        populatedDots.Remove(dot.Key);
                        Destroy(dot.Key.gameObject);
                    }
                }

                i = 0;
                foreach (var item in populatedDots)
                {
                    Vector2 pos = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, populationConfig.values[i++]);
                    if (chartRect.rect.Contains(pos))
                    {
                        item.Key.rectTransform.localPosition = pos;
                    }
                }
            }
            else if (populationConfig.type == ChartPopulateType.Line)
            {
                chartLine.startColor = populationConfig.color;
                chartLine.endColor = populationConfig.color;

                List<Vector2> result = new List<Vector2>();

                foreach (var item in populationConfig.values)
                {
                    Vector2 pos = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, item);
                    if (chartRect.rect.Contains(pos))
                    {
                        populatedLine.Add(item);
                        result.Add(item);
                    }
                }

                chartLine.positionCount = result.Count;

                int i = 0;
                foreach (var item in result)
                {
                    chartLine.SetPosition(i++, item);
                }
            }
        }

        protected virtual void CreateAxis(int axisIndex)
        {
            AxisData axisData = axisDatas[axisIndex];

            int stepCount = (int)Mathf.Round(1.0f / axisData.stepPercent);
            int stepCountHalf = (int)Mathf.Round(stepCount * .5f);

            float stepAmount = (axisData.max - axisData.min) * axisData.stepPercent;

            for (int i = -stepCountHalf; i <= stepCountHalf; i++)
            {
                var tick = Instantiate(baseImage, chartRect);

                SetTickPositionAndSize(tick, i, axisIndex);

                axisTicks[axisIndex].Add(i, tick);

                if ((i + 1) % axisData.axisTickTextThinnes == 0)
                {
                    var text = Instantiate(baseText, chartRect);

                    text.text = (axisData.min + (stepAmount * (i + stepCountHalf))).ToString();

                    SetTextPositionAndSize(text, i, axisIndex);

                    axisTexts[axisIndex].Add(i, text);
                }
            }
        }

        protected virtual void UpdateAxis(int axisIndex)
        {
            AxisData axisData = axisDatas[axisIndex];

            int stepCount = (int)Mathf.Round(1.0f / axisData.stepPercent);
            int stepCountHalf = (int)Mathf.Round(stepCount * .5f);

            for (int i = -stepCountHalf; i <= stepCountHalf; i++)
            {
                SetTickPositionAndSize(axisTicks[axisIndex][i], i, axisIndex);

                if ((i + 1) % axisData.axisTickTextThinnes == 0)
                {
                    SetTextPositionAndSize(axisTexts[axisIndex][i], i, axisIndex);
                }
            }
        }

        #region SIZE AND POSITION UPDATE UTILITIES
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

        }

        protected virtual void SetMainAxisesPositionAndSize()
        {
            for (int i = 0; i < axisDatas.Length; i++)
            {
                if (axisDatas[i].axisType == AxisType.X_Axis)
                {
                    axisImages[i].rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(axisDatas[i]);
                    axisImages[i].rectTransform.sizeDelta = new Vector2(currentSizeX, 3);
                }
                else
                {
                    axisImages[i].rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(axisDatas[i]);
                    axisImages[i].rectTransform.sizeDelta = new Vector2(3, currentSizeY);
                }
            }
        }

        private Vector2 AxisOriantationOffset(AxisData axisData)
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

            float xPercent = (float)coordinate.x / (float)(xAxisData.max - xAxisData.min);

            float xPos = currentSizeX * xPercent;

            float yPercent = (float)coordinate.y / (float)(yAxisData.max - yAxisData.min);

            float yPos = currentSizeY * yPercent;

            return new Vector3(xPos, yPos);
        }

        #endregion

        protected virtual Image DrawCircle(Color color, float width, Vector2 position)
        {
            var dot = Instantiate(baseCircle, chartRect);

            dot.rectTransform.sizeDelta = new Vector2(width, width);
            dot.rectTransform.localPosition = position;

            dot.color = color;

            return dot;
        }
    }
}