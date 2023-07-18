using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using System.Linq;

namespace EEA.Visualizer
{
    [System.Serializable]
    public struct AxisData
    {
        public float min;
        public float max;
        public float stepPercent;
        public int axisTickTextThinnes;
        public AxisOrientation orientation;
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
        [SerializeField] protected AxisData xAxisData;
        [SerializeField] protected AxisData yAxisData;

        [Header("Visualizing Methods")]
        [SerializeField] protected LineRenderer chartLine;

        protected Image xAxisImage;
        protected Image yAxisImage;

        protected Dictionary<int, Image> xAxisTicks = new Dictionary<int, Image>();
        protected Dictionary<int, Image> yAxisTicks = new Dictionary<int, Image>();
        protected Dictionary<int, TextMeshProUGUI> xAxisTexts = new Dictionary<int, TextMeshProUGUI>();
        protected Dictionary<int, TextMeshProUGUI> yAxisTexts = new Dictionary<int, TextMeshProUGUI>();

        protected Dictionary<Image, Vector2> populatedDots = new Dictionary<Image, Vector2>();
        protected List<Vector2> populatedLine = new List<Vector2>();

        protected float currentSizeX;
        protected float currentSizeY;

        protected float halfWidth;
        protected float halfHeight;

        protected float stepWidth;
        protected float stepHeight;

        protected int xStepCount;
        protected int yStepCount;

        protected ChartPopulateType currentPopulateType;

        // CHECKS CHART SIZE EVERY FRAME
        // UPDATES ONLY IF SIZE CHANGES 
        protected void Update()
        {
            OnUpdate();
        }

        protected virtual void OnUpdate()
        {
            if ((chartRect.sizeDelta.x != currentSizeX) || (chartRect.sizeDelta.y != currentSizeY))
            {
                OnChartSizeChanged();
            }
        }

        public virtual void SetUpChart()
        {
            xAxisImage = Instantiate(baseImage, chartRect);
            yAxisImage = Instantiate(baseImage, chartRect);

            UpdateChartSizeValues();

            SetMainAxisesPositionAndSize();

            for (int i = -xStepCount; i <= xStepCount; i++)
            {
                if (i == 0) continue;

                var tick = Instantiate(baseImage, chartRect);

                SetTickPositionAndSize(tick, i, AxisType.X_Axis);

                xAxisTicks.Add(i, tick);

                if ((i + 1) % xAxisData.axisTickTextThinnes == 0)
                {
                    var text = Instantiate(baseText, chartRect);

                    text.text = (xAxisData.max * xAxisData.stepPercent * i).ToString();

                    SetTextPositionAndSize(text, i, AxisType.X_Axis);

                    xAxisTexts.Add(i, text);
                }
            }

            for (int i = -yStepCount; i <= yStepCount; i++)
            {
                if (i == 0) continue;
                var tick = Instantiate(baseImage, chartRect);

                SetTickPositionAndSize(tick, i, AxisType.Y_Axis);

                yAxisTicks.Add(i, tick);

                if ((i + 1) % yAxisData.axisTickTextThinnes == 0)
                {
                    var text = Instantiate(baseText, chartRect);

                    text.text = (yAxisData.max * yAxisData.stepPercent * i).ToString();

                    SetTextPositionAndSize(text, i, AxisType.Y_Axis);

                    yAxisTexts.Add(i, text);
                }
            }
        }

        public virtual void OnChartSizeChanged()
        {
            UpdateChartSizeValues();

            SetMainAxisesPositionAndSize();

            for (int i = -xStepCount; i <= xStepCount; i++)
            {
                if (i == 0) continue;

                SetTickPositionAndSize(xAxisTicks[i], i, AxisType.X_Axis);

                if ((i + 1) % xAxisData.axisTickTextThinnes == 0)
                {
                    SetTextPositionAndSize(xAxisTexts[i], i, AxisType.X_Axis);
                }
            }

            for (int i = -yStepCount; i <= yStepCount; i++)
            {
                if (i == 0) continue;

                SetTickPositionAndSize(yAxisTicks[i], i, AxisType.Y_Axis);

                if ((i + 1) % yAxisData.axisTickTextThinnes == 0)
                {
                    SetTextPositionAndSize(yAxisTexts[i], i, AxisType.Y_Axis);
                }
            }

            if (currentPopulateType == ChartPopulateType.Dots)
            {
                foreach (var dot in populatedDots)
                {
                    dot.Key.rectTransform.localPosition = GetPopulatedItemPos(dot.Value);
                }
            }
            else if (currentPopulateType == ChartPopulateType.Line)
            {
                int i = 0;
                foreach (var lineCoordinate in populatedLine)
                {
                    chartLine.SetPosition(i++, GetPopulatedItemPos(lineCoordinate));
                }
            }
        }

        public virtual void PopulateChart(ChartPopulateType type, List<Vector2> values, Color color)
        {
            currentPopulateType = type;

            if (type == ChartPopulateType.Dots)
            {
                foreach (var item in values)
                {
                    if ((Mathf.Abs(item.x) <= xAxisData.max) && (Mathf.Abs(item.y) <= yAxisData.max))
                    {
                        Image dot = DrawCircle(color, currentSizeX * .015f, GetPopulatedItemPos(item));

                        populatedDots.Add(dot, item);
                    }
                }
            }
            else if (type == ChartPopulateType.Line)
            {
                chartLine.startColor = color;
                chartLine.endColor = color;

                List<Vector2> result = new List<Vector2>();

                foreach (var item in values)
                {
                    if ((Mathf.Abs(item.x) <= xAxisData.max) && (Mathf.Abs(item.y) <= yAxisData.max))
                    {
                        populatedLine.Add(item);
                        result.Add(GetPopulatedItemPos(item));
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

        public virtual void UpdateChartPopulation(ChartPopulateType type, List<Vector2> values, Color color)
        {
            currentPopulateType = type;

            if (type == ChartPopulateType.Dots)
            {
                int i = 0;
                if (values.Count > populatedDots.Count)
                {
                    int diff = values.Count - populatedDots.Count;
                    for (i = 0; i < diff; i++)
                    {
                        Image dot = DrawCircle(color, currentSizeX * .015f, GetPopulatedItemPos(Vector3.zero));

                        populatedDots.Add(dot, Vector3.zero);
                    }
                }
                else if (populatedDots.Count > values.Count)
                {
                    int diff = populatedDots.Count - values.Count;
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
                    item.Key.rectTransform.localPosition = GetPopulatedItemPos(values[i++]);
                }
            }
            else if (type == ChartPopulateType.Line)
            {
                chartLine.startColor = color;
                chartLine.endColor = color;

                List<Vector2> result = new List<Vector2>();

                foreach (var item in values)
                {
                    if ((Mathf.Abs(item.x) <= xAxisData.max) && (Mathf.Abs(item.y) <= yAxisData.max))
                    {
                        populatedLine.Add(item);
                        result.Add(GetPopulatedItemPos(item));
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

        #region SIZE AND POSITION UPDATE UTILITIES

        protected virtual void SetTickPositionAndSize(Image tick, int index, AxisType axis)
        {
            if (axis == AxisType.X_Axis)
            {
                tick.rectTransform.localPosition = new Vector2(index * stepWidth, 0) + AxisOriantationOffset(AxisType.X_Axis);

                tick.rectTransform.sizeDelta = new Vector2(3, halfWidth * .02f);
            }
            else if (axis == AxisType.Y_Axis)
            {
                tick.rectTransform.localPosition = new Vector2(0, index * stepHeight) + AxisOriantationOffset(AxisType.Y_Axis);

                tick.rectTransform.sizeDelta = new Vector2(halfHeight * .02f, 3);
            }
        }

        protected virtual void SetTextPositionAndSize(TextMeshProUGUI text, int index, AxisType axis)
        {
            if (axis == AxisType.X_Axis)
            {
                text.rectTransform.localPosition = new Vector2(index * stepWidth, stepHeight * -.3f) + AxisOriantationOffset(AxisType.X_Axis);
            }
            else if (axis == AxisType.Y_Axis)
            {
                text.rectTransform.localPosition = new Vector2(stepHeight * -.3f, index * stepHeight) + AxisOriantationOffset(AxisType.Y_Axis);
            }

            text.fontSize = halfWidth / 25.0f;

        }

        protected virtual void SetMainAxisesPositionAndSize()
        {
            xAxisImage.rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(AxisType.X_Axis);// new Vector2(0, halfHeight) * (int)xAxisData.orientation;
            yAxisImage.rectTransform.localPosition = Vector2.zero + AxisOriantationOffset(AxisType.Y_Axis);//new Vector2(halfWidth, 0) * (int)yAxisData.orientation;

            xAxisImage.rectTransform.sizeDelta = new Vector2(currentSizeX, 3);
            yAxisImage.rectTransform.sizeDelta = new Vector2(3, currentSizeY);
        }

        private Vector2 AxisOriantationOffset(AxisType axisType)
        {
            if (axisType == AxisType.X_Axis)
                return new Vector2(0, halfHeight) * (int)xAxisData.orientation;
            else 
                return new Vector2(halfWidth, 0) * (int)yAxisData.orientation;
        }

        protected virtual void UpdateChartSizeValues()
        {
            currentSizeX = chartRect.rect.width;
            currentSizeY = chartRect.rect.height;

            halfWidth = currentSizeX * .5f;
            halfHeight = currentSizeY * .5f;

            stepWidth = halfWidth * xAxisData.stepPercent;
            stepHeight = halfHeight * yAxisData.stepPercent;

            xStepCount = (int)(1.0f / xAxisData.stepPercent);
            yStepCount = (int)(1.0f / yAxisData.stepPercent);
        }

        protected virtual Vector3 GetPopulatedItemPos(Vector2 coordinate)
        {
            float xPercent = (float)coordinate.x / (float)xAxisData.max;

            float xPos = (currentSizeX * .5f) * xPercent;

            float yPercent = (float)coordinate.y / (float)yAxisData.max;

            float yPos = (currentSizeY * .5f) * yPercent;

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