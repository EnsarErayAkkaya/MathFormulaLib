using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EEA.Visualizer
{
    [System.Serializable]
    public struct AxisData
    {
        public float max;
        public float stepPercent;
    }
    
    public class ChartVisualizer : MonoBehaviour
    {
        [SerializeField] private RectTransform chartRect;
        [SerializeField] private Image baseImage;
        [SerializeField] private TextMeshProUGUI baseText;

        private Image xAxisImage;
        private Image yAxisImage;

        private Dictionary<int, Image> xAxisTicks = new Dictionary<int, Image>();
        private Dictionary<int, Image> yAxisTicks = new Dictionary<int, Image>();
        private Dictionary<int, TextMeshProUGUI> xAxisTexts = new Dictionary<int, TextMeshProUGUI>();
        private Dictionary<int, TextMeshProUGUI> yAxisTexts = new Dictionary<int, TextMeshProUGUI>();

        private AxisData xAxis;
        private AxisData yAxis;

        private float currentSizeX;
        private float currentSizeY;

        private void Start()
        {
            AxisData a;

            a.max = 100;
            a.stepPercent = .05f;

            SetUpChart(a, a);
        }

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

        public void SetUpChart(AxisData xAxis, AxisData yAxis)
        {
            this.xAxis = xAxis;
            this.yAxis = yAxis;

            xAxisImage = Instantiate(baseImage, chartRect);
            yAxisImage = Instantiate(baseImage, chartRect);

            xAxisImage.rectTransform.localPosition = Vector2.zero;
            yAxisImage.rectTransform.localPosition = Vector2.zero;

            xAxisImage.rectTransform.sizeDelta = new Vector2(chartRect.sizeDelta.x, 3);
            yAxisImage.rectTransform.sizeDelta = new Vector2(3, chartRect.sizeDelta.y);

            float halfWidth = chartRect.sizeDelta.x * .5f;
            float halfHeight = chartRect.sizeDelta.y * .5f;

            float stepWidth = halfWidth * xAxis.stepPercent;
            float stepHeight = halfHeight * yAxis.stepPercent;

            int xStepCount = (int)(xAxis.max / (100 * xAxis.stepPercent));
            int yStepCount = (int)(xAxis.max / (100 * yAxis.stepPercent));

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

                    text.text = (xAxis.max * xAxis.stepPercent * i).ToString();

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

                    text.text = (yAxis.max * yAxis.stepPercent * i).ToString();

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

            xAxisImage.rectTransform.sizeDelta = new Vector2(chartRect.sizeDelta.x, 3);
            yAxisImage.rectTransform.sizeDelta = new Vector2(3, chartRect.sizeDelta.y);

            float halfWidth = chartRect.sizeDelta.x * .5f;
            float halfHeight = chartRect.sizeDelta.y * .5f;

            float stepWidth = halfWidth * xAxis.stepPercent;
            float stepHeight = halfHeight * yAxis.stepPercent;

            int xStepCount = (int)(xAxis.max / (100 * xAxis.stepPercent));
            int yStepCount = (int)(xAxis.max / (100 * yAxis.stepPercent));

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

        public void PopulateChart()
        {

        }
    }
}