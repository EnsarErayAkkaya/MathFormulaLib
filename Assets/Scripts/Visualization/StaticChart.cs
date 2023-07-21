using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using static UnityEditor.Progress;
using System.Linq;

namespace EEA.Visualizer
{    
    public class StaticChart : ChartBase
    {
        protected override void OnUpdate()
        {
            if ((chartRect.rect.width != currentSizeX) || (chartRect.rect.height != currentSizeY))
            {
                OnChartSizeChanged();
            }
        }

        // SETUP CHART
        public override void SetUpChart()
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

        // CALLED WHEN CHART SIZE CHANGED
        public override void OnChartSizeChanged()
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

        public override void PopulateChart(PopulationConfig populationConfig)
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
            else if (populationConfig.type == ChartPopulateType.Column)
            {
                chartLine.startColor = populationConfig.color;
                chartLine.endColor = populationConfig.color;

                List<Vector2> result = new List<Vector2>();

                int xAxisIndex = GetAxisIndexByKey(currentPopulationConfig.xAxisKey);

                foreach (var item in populationConfig.values)
                {
                    Vector2 pos = GetPopulatedItemPos(currentPopulationConfig.xAxisKey, currentPopulationConfig.yAxisKey, item);

                    if (chartRect.rect.Contains(pos))
                    {
                        Image column = Instantiate(baseImage, chartRect);
                        column.rectTransform.pivot = new Vector2(.5f, 0);
                        column.rectTransform.sizeDelta = new Vector2(stepDists[xAxisIndex] * .4f, pos.y);
                        column.rectTransform.localPosition = new Vector3(pos.x, 0) + (Vector3)AxisOriantationOffset(axisDatas[xAxisIndex]);
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

        public override void UpdateChartPopulation(PopulationConfig populationConfig)
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

        protected override void CreateAxis(int axisIndex)
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
    }
}