using VRageMath;
using Lima.API;
using System;

namespace Lima
{
  public class SettingsView : ScrollView
  {
    private int _chartIndex = 0;

    private Chart _chart;
    private Label[] _slidersLabels;
    private Slider[] _sliders;
    private LegendItem[] _legends;

    private View _containerView;
    private View _legendsView;
    private ChartView _chartView;
    private Button _cancelButton;
    private Button _confirmButton;

    private bool _hasChanges = false;
    public Action OnChangeConfig;

    public SettingsView(ChartView chartView, Action onChangeConfig)
    {
      _chartView = chartView;
      OnChangeConfig = onChangeConfig;

      Direction = ViewDirection.Column;
    }

    public void CreateElements()
    {
      Padding = new Vector4(2);

      var chartColorsLabel = new Label("Chart Colors", 0.4f);
      chartColorsLabel.AutoBreakLine = true;
      AddChild(chartColorsLabel);

      var intervalSwitcher = new Switch(new string[] { "Consumption", "Max Consum.", "Production", "Capacity" }, _chartIndex, (int v) =>
      {
        _chartIndex = v;
        UpdateSliders();
      });
      AddChild(intervalSwitcher);

      _containerView = new View(ViewDirection.Row);
      _containerView.BorderColor = App.Theme.GetMainColorDarker(2);
      _containerView.Border = new Vector4(2, 2, 2, 0);
      _containerView.Padding = new Vector4(4);
      _containerView.Pixels = new Vector2(0, 48 * 4);
      _containerView.Flex = new Vector2(1, 0);
      _containerView.Gap = 8;
      AddChild(_containerView);

      var slidersView = new View(ViewDirection.Column);
      _containerView.AddChild(slidersView);

      _slidersLabels = new Label[4];
      _sliders = new Slider[4];
      _legends = new LegendItem[4];

      var slider1Wrapper = new View();
      _slidersLabels[0] = new Label("Red");
      slider1Wrapper.AddChild(_slidersLabels[0]);
      _sliders[0] = new Slider(0, 255, OnChangecolor);
      _sliders[0].IsInteger = true;
      slider1Wrapper.AddChild(_sliders[0]);
      slidersView.AddChild(slider1Wrapper);

      var slider2Wrapper = new View();
      _slidersLabels[1] = new Label("Green");
      slider2Wrapper.AddChild(_slidersLabels[1]);
      _sliders[1] = new Slider(0, 255, OnChangecolor);
      _sliders[1].IsInteger = true;
      slider2Wrapper.AddChild(_sliders[1]);
      slidersView.AddChild(slider2Wrapper);

      var slider3Wrapper = new View();
      _slidersLabels[2] = new Label("Blue");
      slider3Wrapper.AddChild(_slidersLabels[2]);
      _sliders[2] = new Slider(0, 255, OnChangecolor);
      _sliders[2].IsInteger = true;
      slider3Wrapper.AddChild(_sliders[2]);
      slidersView.AddChild(slider3Wrapper);

      var slider4Wrapper = new View();
      _slidersLabels[3] = new Label("Alpha");
      slider4Wrapper.AddChild(_slidersLabels[3]);
      _sliders[3] = new Slider(0, 255, OnChangecolor);
      _sliders[3].IsInteger = true;
      slider4Wrapper.AddChild(_sliders[3]);
      slidersView.AddChild(slider4Wrapper);

      var chartWrapper = new View();
      _containerView.AddChild(chartWrapper);

      _chart = new Chart(10);
      _chart.DataSets.Add(new float[] { 8, 8, 8, 8, 8, 8, 9, 9, 9, 9 });
      _chart.DataSets.Add(new float[] { 7, 7, 7, 7, 7, 6, 6, 7, 8, 8 });
      _chart.DataSets.Add(new float[] { 6, 6, 6, 5, 5, 5, 6, 7, 6, 6 });
      _chart.DataSets.Add(new float[] { 4, 3, 3, 3, 3, 4, 6, 6, 7, 7 });
      _chart.DataColors.Add(_chartView.DataColors[0]);
      _chart.DataColors.Add(_chartView.DataColors[1]);
      _chart.DataColors.Add(_chartView.DataColors[2]);
      _chart.DataColors.Add(_chartView.DataColors[3]);
      _chart.GridHorizontalLines = 5;
      _chart.GridVerticalLines = 5;
      chartWrapper.AddChild(_chart);

      _legendsView = new View(ViewDirection.Row);
      _legendsView.Alignment = ViewAlignment.Center;
      _legendsView.Padding = new Vector4(8, 0, 2, 0);
      _legendsView.Flex = new Vector2(1, 0);
      _legendsView.Pixels = new Vector2(0, 20);
      _legendsView.BgColor = App.Theme.GetMainColorDarker(2);
      AddChild(_legendsView);

      _legends[0] = new LegendItem("Consumption", _chartView.DataColors[3]);
      _legendsView.AddChild(_legends[0]);
      _legends[1] = new LegendItem("Max Consum.", _chartView.DataColors[2]);
      _legendsView.AddChild(_legends[1]);
      _legends[2] = new LegendItem("Production", _chartView.DataColors[1]);
      _legendsView.AddChild(_legends[2]);
      _legends[3] = new LegendItem("Capacity", _chartView.DataColors[0]);
      _legendsView.AddChild(_legends[3]);

      var buttonsContainer = new View(ViewDirection.Row);
      buttonsContainer.Margin = new Vector4(0, 4, 0, 4);
      buttonsContainer.Gap = 4;
      buttonsContainer.Anchor = ViewAnchor.SpaceBetween;
      AddChild(buttonsContainer);

      var resetButton = new Button("Reset Colors", OnClickReset);
      resetButton.Flex = new Vector2(0.3f, 0);
      buttonsContainer.AddChild(resetButton);

      var cancelConfirmView = new View(ViewDirection.Row);
      cancelConfirmView.Gap = 4;
      cancelConfirmView.Flex = new Vector2(0.6f, 0);
      buttonsContainer.AddChild(cancelConfirmView);

      _cancelButton = new Button("Cancel Changes", OnClickCancel);
      cancelConfirmView.AddChild(_cancelButton);

      _confirmButton = new Button("Confirm Changes", OnClickConfirm);
      _confirmButton.Enabled = _hasChanges;
      cancelConfirmView.AddChild(_confirmButton);

      UpdateButtons();
      UpdateSliders();
    }

    private void OnClickReset()
    {
      var changed = false;
      for (int i = 0; i < 4; i++)
      {
        changed = changed || _chartView.DataColors[i] != _chartView.DefaultColors[i];
        _chart.DataColors[i] = _chartView.DefaultColors[i];
        _legends[i].UpdateColor(_chartView.DefaultColors[3 - i]);
      }

      _hasChanges = changed;
      UpdateButtons();
      UpdateSliders();
    }

    public void OnClickCancel()
    {
      for (int i = 0; i < 4; i++)
      {
        _chart.DataColors[i] = _chartView.DataColors[i];
        _legends[i].UpdateColor(_chartView.DataColors[3 - i]);
      }

      _hasChanges = false;
      UpdateButtons();
      UpdateSliders();
    }

    private void OnClickConfirm()
    {
      if (_hasChanges)
      {
        for (int i = 0; i < 4; i++)
          _chartView.DataColors[i] = _chart.DataColors[i];

        _chartView.ApplyColors();
        OnChangeConfig();
      }

      _hasChanges = false;
      UpdateButtons();
    }

    public void UpdateAppThemeColors()
    {
      _containerView.BorderColor = App.Theme.GetMainColorDarker(2);
      _legendsView.BgColor = App.Theme.GetMainColorDarker(2);
    }

    private void OnChangecolor(float value)
    {
      var color = new Color((int)_sliders[0].Value, (int)_sliders[1].Value, (int)_sliders[2].Value, (int)_sliders[3].Value);
      _chart.DataColors[3 - _chartIndex] = color;
      _legends[_chartIndex].UpdateColor(color);

      _hasChanges = true;
      UpdateButtons();
      UpdateSliderLabels();
    }

    private void UpdateSliders()
    {
      var color = _chart.DataColors[3 - _chartIndex];

      _sliders[0].Value = (float)color.R;
      _sliders[1].Value = (float)color.G;
      _sliders[2].Value = (float)color.B;
      _sliders[3].Value = (float)color.A;

      UpdateSliderLabels();
    }

    private void UpdateSliderLabels()
    {
      _slidersLabels[0].Text = $"Red: {(int)_sliders[0].Value}";
      _slidersLabels[1].Text = $"Green: {(int)_sliders[1].Value}";
      _slidersLabels[2].Text = $"Blue: {(int)_sliders[2].Value}";
      _slidersLabels[3].Text = $"Alpha: {(int)_sliders[3].Value}";

      UpdateAppThemeColors();
    }

    private void UpdateButtons()
    {
      _cancelButton.Enabled = _hasChanges;
      _confirmButton.Enabled = _hasChanges;
    }
  }
}