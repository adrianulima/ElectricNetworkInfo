using System;
using VRageMath;
using Lima.API;
using VRage.Game.GUI.TextPanel;

namespace Lima
{
  public class StatusView : View
  {
    public string Title;
    public float Value;
    public float MaxValue;

    private Label _titleLabel;
    private ProgressBar _progressBar;

    public StatusView(string title, float maxValue = 100, float value = 0) : base(ViewDirection.Column)
    {
      Title = title;
      Value = value;
      MaxValue = maxValue;

      CreateElements();
    }

    private void CreateElements()
    {
      _titleLabel = new Label(Title, 0.4f, TextAlignment.CENTER);
      AddChild(_titleLabel);

      _progressBar = new ProgressBar(0, MaxValue);
      _progressBar.Label.FontSize = 0.4f;
      _progressBar.Label.Alignment = TextAlignment.RIGHT;
      AddChild(_progressBar);
    }

    public void UpdateValues()
    {
      var sv = ElectricNetworkInfoApp.PowerFormat(Value);
      var smv = ElectricNetworkInfoApp.PowerFormat(MaxValue);

      _progressBar.Label.Text = $"{sv} / {smv}";
      _progressBar.Value = Value;
      _progressBar.MaxValue = MaxValue;
    }
  }
}