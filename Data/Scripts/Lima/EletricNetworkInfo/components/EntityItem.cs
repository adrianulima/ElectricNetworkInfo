using System;
using VRageMath;
using Lima.API;
using VRage.Game.GUI.TextPanel;

namespace Lima2
{
  public class EntityItem : FancyView
  {
    public string Title;
    public int Count;
    public float Value;
    public float MaxValue;

    private FancyView _titleView;
    private FancyLabel _titleLabel;
    private FancyLabel _countLabel;
    private FancyProgressBar _progressBar;

    private readonly Color _mainColor;

    public EntityItem(string title, Color mainColor) : base(ViewDirection.Column, new Color(mainColor * 0.3f, 1))
    {
      Title = title;
      _mainColor = mainColor;
      SetStyles();
      CreateElements();
    }

    private void SetStyles()
    {
      SetPadding(new Vector4(2));
    }

    private void CreateElements()
    {
      _titleView = new FancyView(ViewDirection.Row);
      _titleView.SetScale(new Vector2(1, 0));
      _titleView.SetPixels(new Vector2(0, 14));
      AddChild(_titleView);

      _titleLabel = new FancyLabel(Title, 0.4f, TextAlignment.LEFT);
      _titleView.AddChild(_titleLabel);

      _countLabel = new FancyLabel("0", 0.4f, TextAlignment.RIGHT);
      _titleView.AddChild(_countLabel);

      _progressBar = new FancyProgressBar(0, MaxValue, false);
      _progressBar.SetScale(new Vector2(1, 0));
      _progressBar.SetPixels(new Vector2(0, 16));
      _progressBar.SetLabelScale(0.35f);
      AddChild(_progressBar);
    }

    public void UpdateValues()
    {
      _titleLabel.SetText(Title);
      _countLabel.SetText(Count.ToString());

      _titleLabel.SetTextColor(_mainColor);
      _countLabel.SetTextColor(_mainColor);

      var sv = ElectricNetworkInfoApp.PowerFormat(Value);

      _progressBar.SetLabel(sv);
      _progressBar.SetValue(Value);
      _progressBar.SetMaxValue(MaxValue);
    }
  }
}