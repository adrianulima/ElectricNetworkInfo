using VRageMath;
using Lima.API;
using System.Linq;
using Sandbox.ModAPI;

namespace Lima
{
  public class EntitiesPanel : TouchView
  {
    public EntitiesPanel()
    {
    }

    public EntityListView ProductionList { get; private set; }
    public EntityListView ConsumptionList { get; private set; }

    private EntityItemPooler _pooler = new EntityItemPooler(30);

    public void CreateElements()
    {
      Direction = ViewDirection.Row;
      Padding = new Vector4(4);
      Gap = 4;

      var bgColor = App.Theme.GetMainColorDarker(1);

      ConsumptionList = new EntityListView("CONSUMERS", 2);
      ConsumptionList.SetScrollViewBgColor(bgColor);
      ConsumptionList.Scale = new Vector2(2, 1);
      AddChild(ConsumptionList);

      ProductionList = new EntityListView("PRODUCERS", 1);
      ProductionList.SetScrollViewBgColor(bgColor);
      ProductionList.Scale = new Vector2(1, 1);
      AddChild(ProductionList);
    }

    public void UpdateValues(ElectricNetworkManager electricMan)
    {

      var cols = MathHelper.FloorToInt(GetSize().X / 160);
      if (cols < 2)
        cols = 2;
      ConsumptionList.Cols = cols - 1;
      ConsumptionList.Scale = new Vector2(ConsumptionList.Cols, 1);

      var bgColor = App.Theme.GetMainColorDarker(2);
      var entityColor = App.Theme.GetMainColorDarker(4);
      ProductionList.SetScrollViewBgColor(bgColor);
      ProductionList.RemoveAllChildren(_pooler);

      var productionList = electricMan.ProductionBlocks.ToList();
      productionList.Sort((pair1, pair2) => pair2.Value.Item2.CompareTo(pair1.Value.Item2));
      foreach (var item in productionList)
      {
        var entity = _pooler.GetEntityItem(item.Key, App.Theme.WhiteColor);
        entity.BgColor = entityColor;
        entity.Count = item.Value.Item1;
        entity.Value = item.Value.Item2;
        entity.MaxValue = electricMan.CurrentPowerStats.Production + electricMan.CurrentPowerStats.BatteryOutput;
        entity.IconTexture = GameSession.Instance.Api.GetBlockIconSprite(item.Value.Item3);
        ProductionList.AddItem(entity);
        entity.UpdateValues();
      }
      ProductionList.FillLastView();
      ProductionList.ScrollWheelStep = 36 * this.App?.Theme.Scale ?? 1;

      ConsumptionList.SetScrollViewBgColor(bgColor);
      ConsumptionList.RemoveAllChildren(_pooler);

      var consumptionList = electricMan.ConsumptionBlocks.ToList();
      consumptionList.Sort((pair1, pair2) => pair2.Value.Item2.CompareTo(pair1.Value.Item2));
      foreach (var item in consumptionList)
      {
        var entity = _pooler.GetEntityItem(item.Key, App.Theme.WhiteColor);
        entity.BgColor = entityColor;
        entity.Count = item.Value.Item1;
        entity.Value = item.Value.Item2;
        entity.MaxValue = electricMan.CurrentPowerStats.Consumption;
        entity.IconTexture = GameSession.Instance.Api.GetBlockIconSprite(item.Value.Item3);
        ConsumptionList.AddItem(entity);
        entity.UpdateValues();
      }
      ConsumptionList.FillLastView();
      ConsumptionList.ScrollWheelStep = 36 * this.App?.Theme.Scale ?? 1;
    }

    public void Dispose()
    {
      ProductionList.Dispose();
      ConsumptionList.Dispose();
    }
  }
}