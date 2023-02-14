using VRageMath;
using Lima.API;
using System.Linq;

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

      ConsumptionList = new EntityListView("CONSUMERS", 3);
      ConsumptionList.SetScrollViewBgColor(bgColor);
      ConsumptionList.Scale = new Vector2(3, 1);
      AddChild(ConsumptionList);

      ProductionList = new EntityListView("PRODUCERS", 1);
      ProductionList.SetScrollViewBgColor(bgColor);
      ProductionList.Scale = new Vector2(1, 1);
      AddChild(ProductionList);
    }

    public void UpdateValues(ElectricNetworkManager electricMan)
    {
      var bgColor = App.Theme.GetMainColorDarker(2);

      ProductionList.SetScrollViewBgColor(bgColor);
      ProductionList.RemoveAllChildren(_pooler);

      var productionList = electricMan.ProductionBlocks.ToList();
      productionList.Sort((pair1, pair2) => pair2.Value.Y.CompareTo(pair1.Value.Y));
      foreach (var item in productionList)
      {
        var entity = _pooler.GetEntityItem(item.Key, App.Theme.WhiteColor);
        entity.BgColor = App.Theme.GetMainColorDarker(4);
        entity.Count = (int)item.Value.X;
        entity.Value = item.Value.Y;
        entity.MaxValue = electricMan.CurrentPowerStats.Production + electricMan.CurrentPowerStats.BatteryOutput;
        ProductionList.AddItem(entity);
        entity.UpdateValues();
      }
      ProductionList.FillLastView();
      ProductionList.ScrollWheelStep = 36 * this.App?.Theme.Scale ?? 1;

      ConsumptionList.SetScrollViewBgColor(bgColor);
      ConsumptionList.RemoveAllChildren(_pooler);

      var consumptionList = electricMan.ConsumptionBlocks.ToList();
      consumptionList.Sort((pair1, pair2) => pair2.Value.Y.CompareTo(pair1.Value.Y));
      foreach (var item in consumptionList)
      {
        var entity = _pooler.GetEntityItem(item.Key, App.Theme.WhiteColor);
        entity.BgColor = App.Theme.GetMainColorDarker(4);
        entity.Count = (int)item.Value.X;
        entity.Value = item.Value.Y;
        entity.MaxValue = electricMan.CurrentPowerStats.Consumption;
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