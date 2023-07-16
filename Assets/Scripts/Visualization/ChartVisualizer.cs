using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

namespace EEA.Visualizer
{
    [System.Serializable]
    public struct AxisData
    {
        public float max;
        public float stepPercent;
    }

    public enum ChartPopulateType
    {
        Dots, Line
    }
    
    public class ChartVisualizer : MonoBehaviour
    {
        [Header("Chart Rect")]
        [SerializeField] private RectTransform chartRect;
        [Header("Prebas")]
        [SerializeField] private Image baseImage;
        [SerializeField] private TextMeshProUGUI baseText;
        [SerializeField] private Image baseCircle;
        [Header("Chart Rect")]
        [SerializeField] private AxisData xAxisData;
        [SerializeField] private AxisData yAxisData;

        [Header("Visualizing Methods")]
        [SerializeField] private LineRenderer chartLine;

        private Image xAxisImage;
        private Image yAxisImage;

        private Dictionary<int, Image> xAxisTicks = new Dictionary<int, Image>();
        private Dictionary<int, Image> yAxisTicks = new Dictionary<int, Image>();
        private Dictionary<int, TextMeshProUGUI> xAxisTexts = new Dictionary<int, TextMeshProUGUI>();
        private Dictionary<int, TextMeshProUGUI> yAxisTexts = new Dictionary<int, TextMeshProUGUI>();

        private float currentSizeX;
        private float currentSizeY;

        // CHECK CHART POSITONS AND SCALES EVERY FRAME
        // UPDATES ONLY IF SIZE CHANGES 
        private void Update()
        {
            if ((chartRect.sizeDelta.x != currentSizeX) || (chartRect.sizeDelta.y != currentSizeY))
            {
                currentSizeX = chartRect.sizeDelta.x;
                currentSizeY = chartRect.sizeDelta.y;

                UpdateChart();
            }
        }

        public void SetUpChart()
        {
            xAxisImage = Instantiate(baseImage, chartRect);
            yAxisImage = Instantiate(baseImage, chartRect);

            xAxisImage.rectTransform.localPosition = Vector2.zero;
            yAxisImage.rectTransform.localPosition = Vector2.zero;

            currentSizeX = chartRect.sizeDelta.x;
            currentSizeY = chartRect.sizeDelta.y;

            xAxisImage.rectTransform.sizeDelta = new Vector2(currentSizeX, 3);
            yAxisImage.rectTransform.sizeDelta = new Vector2(3, currentSizeY);

            float halfWidth = currentSizeX * .5f;
            float halfHeight = currentSizeY * .5f;

            float stepWidth = halfWidth * xAxisData.stepPercent;
            float stepHeight = halfHeight * yAxisData.stepPercent;

            int xStepCount = (int)(1.0f / xAxisData.stepPercent);
            int yStepCount = (int)(1.0f / yAxisData.stepPercent);

            for (int i = -xStepCount; i <= xStepCount; i++)
            {
                if (i == 0) continue;

                var tick = Instantiate(baseImage, chartRect);

                tick.rectTransform.localPosition = new Vector2(i * stepWidth, 0);

                tick.rectTransform.sizeDelta = new Vector2(3, halfWidth * .02f);

                xAxisTicks.Add(i, tick);

                if ((i + 1) % 2 == 0)
                {
                    var text = Instantiate(baseText, chartRect);

                    text.text = (xAxisData.max * xAxisData.stepPercent * i).ToString();

                    text.rectTransform.localPosition = new Vector2(i * stepWidth, stepHeight * -.3f);

                    text.fontSize = halfWidth / 20.0f;

                    xAxisTexts.Add(i, text);
                }
            }

            for (int i = -yStepCount; i <= yStepCount; i++)
            {
                if (i == 0) continue;
                var tick = Instantiate(baseImage, chartRect);

                tick.rectTransform.localPosition = new Vector2(0, i * stepHeight);

                tick.rectTransform.sizeDelta = new Vector2(halfHeight * .02f, 3);

                yAxisTicks.Add(i, tick);

                if ((i + 1) % 2 == 0)
                {
                    var text = Instantiate(baseText, chartRect);

                    text.text = (yAxisData.max * yAxisData.stepPercent * i).ToString();

                    text.rectTransform.localPosition = new Vector2(stepHeight * -.3f, i * stepHeight);

                    text.fontSize = halfWidth / 20.0f;

                    yAxisTexts.Add(i, text);
                }
            }
        }

        public void UpdateChart()
        {
            xAxisImage.rectTransform.localPosition = Vector2.zero;
            yAxisImage.rectTransform.localPosition = Vector2.zero;

            xAxisImage.rectTransform.sizeDelta = new Vector2(currentSizeX, 3);
            yAxisImage.rectTransform.sizeDelta = new Vector2(3, currentSizeY);

            float halfWidth = currentSizeX * .5f;
            float halfHeight = currentSizeY * .5f;

            float stepWidth = halfWidth * xAxisData.stepPercent;
            float stepHeight = halfHeight * yAxisData.stepPercent;

            int xStepCount = (int)(xAxisData.max / (100 * xAxisData.stepPercent));
            int yStepCount = (int)(yAxisData.max / (100 * yAxisData.stepPercent));

            for (int i = -xStepCount; i <= xStepCount; i++)
            {
                if (i == 0) continue;

                xAxisTicks[i].rectTransform.localPosition = new Vector2(i * stepWidth, 0);

                xAxisTicks[i].rectTransform.sizeDelta = new Vector2(3, halfWidth * .02f);

                if ((i + 1) % 2 == 0)
                {
                    xAxisTexts[i].rectTransform.localPosition = new Vector2(i * stepWidth, stepHeight * -.3f);

                    xAxisTexts[i].fontSize = halfWidth / 20.0f;
                }
            }

            for (int i = -yStepCount; i <= yStepCount; i++)
            {
                if (i == 0) continue;

                yAxisTicks[i].rectTransform.localPosition = new Vector2(0, i * stepHeight);

                yAxisTicks[i].rectTransform.sizeDelta = new Vector2(halfHeight * .02f, 3);

                if ((i + 1) % 2 == 0)
                {
                    yAxisTexts[i].rectTransform.localPosition = new Vector2(stepHeight * -.3f, i * stepHeight);

                    yAxisTexts[i].fontSize = halfWidth / 20.0f;
                }
            }
        }

        public void PopulateChart(ChartPopulateType type, List<Vector2> values, Color color)
        {
            if (type == ChartPopulateType.Dots)
            {
                foreach (var item in values)
                {
                    if ((item.x <= xAxisData.max) && (item.y <= yAxisData.max))
                    {
                        float xPercent = (float)item.x / (float)xAxisData.max;

                        float xPos = (currentSizeX * .5f) * xPercent;

                        float yPercent = (float)item.y / (float)yAxisData.max;

                        float yPos = (currentSizeY * .5f) * yPercent;

                        DrawCircle(color, currentSizeX * .015f, new Vector2(xPos, yPos));
                    }
                }
            }
            else if (type == ChartPopulateType.Line)
            {
                chartLine.startColor = color;
                chartLine.endColor = color;

                chartLine.positionCount = values.Count;

                int i = 0;

                foreach (var item in values)
                {
                    if ((item.x <= xAxisData.max) && (item.y <= yAxisData.max))
                    {
                        float xPercent = (float)item.x / (float)xAxisData.max;

                        float xPos = (currentSizeX * .5f) * xPercent;

                        float yPercent = (float)item.y / (float)yAxisData.max;

                        float yPos = (currentSizeY * .5f) * yPercent;

                        chartLine.SetPosition(i++, new Vector3(xPos, yPos, 0));
                    }
                }
            }
        }


        public Image DrawCircle(Color color, float width, Vector2 position)
        {
            var dot = Instantiate(baseCircle, chartRect);

            dot.rectTransform.sizeDelta = new Vector2(width, width);
            dot.rectTransform.localPosition = position;

            dot.color = color;

            return dot;
        }
    }
}