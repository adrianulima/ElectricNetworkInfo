using VRageMath;
using Lima.API;
using VRage.Game.GUI.TextPanel;

namespace Lima
{
  public class LegendItem : View
  {
    public string Title;
    public float Value;

    private Color _color;
    private Label _titleLabel;
    private View _square;

    public LegendItem(string title, Color color, float value = 0) : base(ViewDirection.Row)
    {
      Title = title;
      Value = value;
      _color = color;

      CreateElements();
    }

    public void UpdateTitleColor(Color color)
    {
      _titleLabel.TextColor = color;
    }

    private void CreateElements()
    {
      Gap = 4;
      Alignment = ViewAlignment.Center;

      _square = new View();
      _square.BgColor = _color;
      _square.Pixels = new Vector2(8, 8);
      _square.Flex = new Vector2(0, 0);
      AddChild(_square);

      _titleLabel = new Label(Title, 0.4f, TextAlignment.CENTER);
      _titleLabel.Alignment = TextAlignment.LEFT;
      AddChild(_titleLabel);
    }

    public void UpdateColor(Color color)
    {
      _color = color;
      _square.BgColor = _color;
    }

    public void UpdateValues()
    {
      var sv = ElectricNetworkInfoApp.PowerFormat(Value);
      _titleLabel.Text = sv;
    }
  }
}