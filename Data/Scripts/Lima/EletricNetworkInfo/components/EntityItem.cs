using VRageMath;
using Lima.API;
using VRage.Game.GUI.TextPanel;

namespace Lima
{
  public class EntityItem : TouchView
  {
    public string Title;
    public int Count;
    public float Value;
    public float MaxValue;
    public string IconTexture;

    private TouchView _wrapperView;
    private TouchView _titleView;
    private TouchLabel _titleLabel;
    private TouchLabel _countLabel;
    private TouchProgressBar _progressBar;
    private Icon _icon;

    public EntityItem(string title, Color textColor) : base(ViewDirection.Row)
    {
      Title = title;
      SetStyles();
      CreateElements(textColor);
    }

    private void SetStyles()
    {
      Padding = new Vector4(2);
    }

    private void CreateElements(Color textColor)
    {
      _icon = new Icon("", new Vector2(30));
      _icon.Margin = new Vector4(0, 0, 2, 0);
      _icon.Bg = true;
      _icon.SpriteColor = new Color(148, 148, 148);
      AddChild(_icon);

      _wrapperView = new TouchView(ViewDirection.Column);
      _wrapperView.Flex = Vector2.One;
      AddChild(_wrapperView);

      _titleView = new TouchView(ViewDirection.Row);
      _titleView.Flex = new Vector2(1, 0);
      _titleView.Pixels = new Vector2(0, 14);
      _wrapperView.AddChild(_titleView);

      _titleLabel = new TouchLabel(Title, 0.4f, TextAlignment.LEFT);
      _titleLabel.TextColor = textColor;
      _titleView.AddChild(_titleLabel);

      _countLabel = new TouchLabel("0", 0.4f, TextAlignment.RIGHT);
      _countLabel.TextColor = textColor;
      _countLabel.Flex = new Vector2(0, 1);
      _countLabel.Pixels = new Vector2(10, 0);
      _titleView.AddChild(_countLabel);

      _progressBar = new TouchProgressBar(0, MaxValue);
      _progressBar.Flex = new Vector2(1, 0);
      _progressBar.Pixels = new Vector2(0, 16);
      _progressBar.Label.FontSize = 0.35f;
      _wrapperView.AddChild(_progressBar);
    }

    public void UpdateColors(Color textColor)
    {
      _titleLabel.TextColor = textColor;
      _countLabel.TextColor = textColor;
    }

    public void UpdateValues()
    {
      _titleLabel.Text = Title;

      var countText = Count.ToString();

      if (_countLabel.Text != countText)
      {
        _countLabel.Text = countText;
        var px = (App?.Theme.MeasureStringInPixels(countText, App.Theme.Font, 0.4f).X ?? 0) + 2;
        _countLabel.Pixels = new Vector2(px, 0);
      }

      var sv = ElectricNetworkInfoApp.PowerFormat(Value);
      _progressBar.Label.Text = sv;
      _progressBar.MaxValue = MaxValue;
      _progressBar.Value = Value;
      _icon.SpriteImage = IconTexture;
    }
  }
}