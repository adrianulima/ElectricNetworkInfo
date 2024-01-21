using System.Collections.Generic;
using VRageMath;
using Lima.API;
using System.Linq;
using VRage.Game.GUI.TextPanel;

namespace Lima
{
  public class EntityListView : View
  {
    private int _odd = 0;

    private ScrollView _scrollView;
    private List<View> _views = new List<View>();
    private List<EntityItem> _entities = new List<EntityItem>();

    public float ScrollWheelStep
    {
      get { return _scrollView.ScrollWheelStep; }
      set { _scrollView.ScrollWheelStep = value; }
    }

    public string Title;
    public int Cols;

    public EntityListView(string title, int cols = 2) : base(ViewDirection.Column)
    {
      Title = title;
      Cols = cols;

      CreateElements();
    }

    public void SetScrollViewBgColor(Color color)
    {
      _scrollView.BgColor = color;
    }

    private void CreateElements()
    {
      var titleLabel = new Label(Title, 0.4f, TextAlignment.LEFT);
      titleLabel.Alignment = TextAlignment.CENTER;
      AddChild(titleLabel);

      _scrollView = new ScrollView(ViewDirection.Column);
      _scrollView.Padding = new Vector4(2, 2, 2, 0);
      _scrollView.Gap = 2;
      _scrollView.ScrollWheelStep = 36;
      AddChild(_scrollView);
    }

    public void Dispose()
    {
      _views.Clear();
      _entities.Clear();
    }

    public void RemoveAllChildren(EntityItemPooler pooler)
    {
      foreach (var v in _views)
      {
        foreach (var ch in v.Children)
          v.RemoveChild(ch);
        _scrollView.RemoveChild(v);
      }

      foreach (var entt in _entities)
        pooler.PutEntityItem(entt);

      _views.Clear();
      _entities.Clear();
      _odd = 0;
    }

    public void FillLastView()
    {
      if (_views.Count == 0) return;

      var view = _views.Last<View>();
      var childCount = view.Children.Count;
      if (childCount < Cols)
      {
        var fill = new View();
        var n = Cols - childCount;
        if (n > 1)
          fill.Pixels = Vector2.UnitX * ((n - 1) * view.Gap);
        view.AddChild(fill);
        fill.Flex = new Vector2(n, 0);
      }
    }

    public void AddItem(EntityItem item)
    {
      _entities.Add(item);
      if (_odd % Cols != 0)
      {
        var view = _views.Last<View>();
        view.AddChild(item);
      }
      else
      {
        var view = new View(ViewDirection.Row);
        view.Gap = 2;
        view.AddChild(item);
        view.Flex = new Vector2(1, 0);
        view.Pixels = new Vector2(0, 34);
        _scrollView.AddChild(view);
        _views.Add(view);
      }
      _odd++;
    }

    public void ResetScroll()
    {
      _scrollView.Scroll = 0;
    }
  }
}