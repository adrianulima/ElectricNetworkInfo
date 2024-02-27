using System;
using VRageMath;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using Sandbox.Game.EntityComponents;
using VRage;
using SpaceEngineers.Game.ModAPI;
using ProtoBuf;
using VRage.Utils;

namespace Lima
{
  public class ElectricNetworkManager
  {
    [ProtoContract(UseProtoMembersOnly = true)]
    public struct PowerStats
    {
      [ProtoMember(1)]
      public float Consumption;
      [ProtoMember(2)]
      public float MaxConsumption;
      [ProtoMember(3)]
      public float Production;
      [ProtoMember(4)]
      public float MaxProduction;
      [ProtoMember(5)]
      public float BatteryOutput;
      [ProtoMember(6)]
      public float BatteryMaxOutput;
    }

    public struct BatteryStats
    {
      public float BatteryInput;
      public float BatteryMaxInput;
      public float BatteryCharge;
      public float BatteryMaxCharge;
      public float BatteryHoursLeft;
      public MyResourceStateEnum EnergyState;
    }

    private readonly List<IMyCubeBlock> _lcdBlocks;
    private MyDefinitionId _electricityId = MyResourceDistributorComponent.ElectricityId;

    public event Action UpdateEvent;
    public bool Disposed { get; private set; } = false;

    public PowerStats CurrentPowerStats = new PowerStats();
    public BatteryStats CurrentBatteryStats = new BatteryStats();

    public readonly PowerStatsHistory History = new PowerStatsHistory();

    private IMyCubeGrid _gridToHandleNextUpdate;
    private IMyGridGroupData _gridGroup;
    private readonly List<IMyCubeGrid> _grids = new List<IMyCubeGrid>();

    private readonly List<MyCubeBlock> _inputList = new List<MyCubeBlock>();
    private readonly List<MyCubeBlock> _outputList = new List<MyCubeBlock>();
    private readonly List<MyCubeBlock> _thrustersList = new List<MyCubeBlock>();

    public readonly Dictionary<string, MyTuple<int, float, MyCubeBlock>> ProductionBlocks = new Dictionary<string, MyTuple<int, float, MyCubeBlock>>();
    public readonly Dictionary<string, MyTuple<int, float, MyCubeBlock>> ConsumptionBlocks = new Dictionary<string, MyTuple<int, float, MyCubeBlock>>();

    private string _batteryName = "";

    public ElectricNetworkManager(IMyCubeBlock lcdBlock)
    {
      _lcdBlocks = new List<IMyCubeBlock>() { lcdBlock };

      HandleGridGroup(lcdBlock.CubeGrid);
      LoadGridContent(lcdBlock.CubeGrid);
    }

    public MyTuple<IMyCubeGrid, GridStorageContent> GenerateGridContent()
    {
      return new MyTuple<IMyCubeGrid, GridStorageContent>(_lcdBlocks[0].CubeGrid, new GridStorageContent()
      {
        GridId = _lcdBlocks[0].CubeGrid.EntityId,
        History_0 = History.Intervals[0].Item3,
        History_1 = History.Intervals[1].Item3,
        History_2 = History.Intervals[2].Item3,
        History_3 = History.Intervals[3].Item3,
        History_4 = History.Intervals[4].Item3
      });
    }

    public void LoadGridContent(IMyCubeGrid gridCube)
    {
      var content = GameSession.Instance.GridHandler.LoadGridContent(gridCube);
      if (content != null)
      {
        History.Intervals[0].Item3 = content.History_0;
        History.Intervals[1].Item3 = content.History_1;
        History.Intervals[2].Item3 = content.History_2;
        History.Intervals[3].Item3 = content.History_3;
        History.Intervals[4].Item3 = content.History_4;
      }
    }

    public void Dispose()
    {
      Clear();
      History.Dispose();
      _lcdBlocks.Clear();
      UpdateEvent = null;
      Disposed = true;
    }

    public void Clear()
    {
      _gridGroup.OnGridRemoved -= OnGridRemovedFromGroup;
      _gridGroup.OnGridAdded -= OnGridAddedToGroup;

      foreach (MyCubeGrid grid in _grids)
      {
        grid.OnBlockAdded -= OnBlockAddedToGrid;
        grid.OnBlockRemoved -= OnBlockRemovedFromGrid;
      }

      _grids.Clear();
      _inputList.Clear();
      _outputList.Clear();
      _thrustersList.Clear();
      ProductionBlocks.Clear();
      ConsumptionBlocks.Clear();
    }

    private void HandleGridGroup(IMyCubeGrid gridCube)
    {
      _gridGroup = MyAPIGateway.GridGroups.GetGridGroup(GridLinkTypeEnum.Electrical, gridCube);
      _gridGroup.OnGridAdded -= OnGridAddedToGroup;
      _gridGroup.OnGridRemoved -= OnGridRemovedFromGroup;
      _gridGroup.OnGridAdded += OnGridAddedToGroup;
      _gridGroup.OnGridRemoved += OnGridRemovedFromGroup;

      _gridGroup.GetGrids(_grids);
      foreach (MyCubeGrid grid in _grids)
        HandleGrid(grid);
    }

    private void HandleGrid(MyCubeGrid grid)
    {
      grid.OnBlockAdded -= OnBlockAddedToGrid;
      grid.OnBlockRemoved -= OnBlockRemovedFromGrid;
      grid.OnBlockAdded += OnBlockAddedToGrid;
      grid.OnBlockRemoved += OnBlockRemovedFromGrid;

      foreach (MyCubeBlock block in grid.GetFatBlocks())
        HandleBlock(block);
    }

    private void HandleBlock(MyCubeBlock block)
    {
      if (block is IMyBatteryBlock)
      {
        if (_batteryName == "" || block.BlockDefinition.BlockPairName == "BatteryBlock")
          _batteryName = block.DefinitionDisplayNameText;
        _inputList.Add(block);
      }
      else
      {
        var thruster = block as MyThrust;
        if (thruster != null && thruster.FuelDefinition.Id == _electricityId)
          _thrustersList.Add(block);
        else
        {
          MyResourceSinkComponent sink = block.Components?.Get<MyResourceSinkComponent>();
          if (sink != null && sink.AcceptedResources.IndexOf(_electricityId) != -1)
            _inputList.Add(block);
          else if (block is IMyGyro && block.IsFunctional)
            _inputList.Add(block);
        }
      }

      var source = block.Components?.Get<MyResourceSourceComponent>();
      if (source != null && source.ResourceTypes.IndexOf(_electricityId) != -1)
        _outputList.Add(block);
    }

    private void OnGridAddedToGroup(IMyGridGroupData group, IMyCubeGrid grid, IMyGridGroupData otherGroup)
    {
      var cubeGrid = grid as MyCubeGrid;
      if (cubeGrid != null)
      {
        _grids.Add(grid);
        HandleGrid(cubeGrid);
      }
    }

    private void OnGridRemovedFromGroup(IMyGridGroupData group, IMyCubeGrid grid, IMyGridGroupData otherGroup)
    {
      Clear();
      if (_lcdBlocks.Count > 0)
        _gridToHandleNextUpdate = _lcdBlocks[0].CubeGrid; // Next update so group is updated before checking
    }

    private void OnBlockAddedToGrid(IMySlimBlock slimBlock)
    {
      try
      {
        MyCubeBlock block = slimBlock.FatBlock as MyCubeBlock;
        if (block == null)
          return;
        HandleBlock(block);
      }
      catch (Exception e)
      {
        GameSession.Instance.RemoveManager(this);
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");
      }
    }

    private void OnBlockRemovedFromGrid(IMySlimBlock slimBlock)
    {
      try
      {
        MyCubeBlock block = slimBlock.FatBlock as MyCubeBlock;
        if (block == null)
          return;

        _inputList.Remove(block);
        _outputList.Remove(block);
        _thrustersList.Remove(block);
      }
      catch (Exception e)
      {
        GameSession.Instance.RemoveManager(this);
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");
      }
    }

    public bool AddBlockIfSameGrid(IMyCubeBlock lcdBlock)
    {
      if (_lcdBlocks[0].CubeGrid == lcdBlock.CubeGrid)
      {
        // if (!_lcdBlocks.Contains(lcdBlock)) // let be added multiple times because of multi surface blocks
        _lcdBlocks.Add(lcdBlock);
        return true;
      }
      return false;
    }

    public bool RemoveBlockAndCount(IMyCubeBlock lcdBlock)
    {
      if (_lcdBlocks.Contains(lcdBlock))
      {
        _lcdBlocks.Remove(lcdBlock);
        return _lcdBlocks.Count == 0;
      }
      return false;
    }

    private void UpdatePowerDict(Dictionary<string, MyTuple<int, float, MyCubeBlock>> dict, MyCubeBlock block, float power, string customKey = "")
    {
      var k = customKey != "" ? customKey : block.DefinitionDisplayNameText;
      if (block.CubeGrid.EntityId != _lcdBlocks[0].CubeGrid.EntityId)
        k = $" {k}";

      if (k == null)
        k = "Unknown";

      MyTuple<int, float, MyCubeBlock> count;
      if (dict.TryGetValue(k, out count))
        dict[k] = new MyTuple<int, float, MyCubeBlock>(count.Item1 + 1, count.Item2 + power, count.Item3);
      else
        dict.Add(k, new MyTuple<int, float, MyCubeBlock>(1, power, block));
    }

    private void UpdateDistributiorStatus()
    {
      var grid = _lcdBlocks[0].CubeGrid as MyCubeGrid;
      if (grid == null || _lcdBlocks[0].CubeGrid.ResourceDistributor == null)
        return;

      MyResourceDistributorComponent distributor = _lcdBlocks[0].CubeGrid.ResourceDistributor as MyResourceDistributorComponent;
      if (distributor != null)
      {
        CurrentBatteryStats.BatteryHoursLeft = distributor.RemainingFuelTimeByType(MyResourceDistributorComponent.ElectricityId, grid: grid);
        CurrentBatteryStats.EnergyState = distributor.ResourceStateByType(MyResourceDistributorComponent.ElectricityId, grid: grid);
      }
    }

    public void Update()
    {
      // TODO: implement a tag to check if there is any tss that still active (player not distant)

      if (_gridToHandleNextUpdate != null)
      {
        HandleGridGroup(_gridToHandleNextUpdate);
        _gridToHandleNextUpdate = null;
      }

      CurrentPowerStats = new PowerStats();
      CurrentBatteryStats = new BatteryStats();

      UpdateDistributiorStatus();

      ProductionBlocks.Clear();
      ConsumptionBlocks.Clear();

      foreach (MyCubeBlock block in _thrustersList)
      {
        MyThrust thrust = block as MyThrust;
        var cons = thrust.MinPowerConsumption + thrust.CurrentStrength * (thrust.MaxPowerConsumption - thrust.MinPowerConsumption);
        CurrentPowerStats.Consumption += cons;
        CurrentPowerStats.MaxConsumption += cons;

        UpdatePowerDict(ConsumptionBlocks, block, cons);
      }

      foreach (MyCubeBlock block in _inputList)
      {
        MyResourceSinkComponent sink = block.Components?.Get<MyResourceSinkComponent>();
        if (sink != null)
        {
          var cons = sink.CurrentInputByType(_electricityId);
          CurrentPowerStats.Consumption += cons;

          var customKey = "";
          IMyBatteryBlock battery = block as IMyBatteryBlock;
          if (battery != null)
          {
            if (battery.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Discharge)
              continue;
            customKey = $"{_batteryName} \"{battery.ChargeMode}\"";
            CurrentBatteryStats.BatteryCharge += battery.CurrentStoredPower;
            CurrentBatteryStats.BatteryMaxCharge += battery.MaxStoredPower;

            if (battery.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Recharge)
              CurrentPowerStats.MaxConsumption += sink.MaxRequiredInputByType(_electricityId);
            else
              CurrentPowerStats.MaxConsumption += cons;

            CurrentBatteryStats.BatteryInput = cons;
            CurrentBatteryStats.BatteryMaxInput = sink.MaxRequiredInputByType(_electricityId);
          }
          else
          {
            if (block is IMyBeacon)
              CurrentPowerStats.MaxConsumption += Math.Max(cons, sink.MaxRequiredInputByType(_electricityId) / 1000f);
            else if (block is IMyRadioAntenna)
              CurrentPowerStats.MaxConsumption += Math.Max(cons, sink.MaxRequiredInputByType(_electricityId) * 100f);
            else if (block is IMyMedicalRoom)
              CurrentPowerStats.MaxConsumption += Math.Max(cons, sink.MaxRequiredInputByType(_electricityId) / 100f);
            else
              CurrentPowerStats.MaxConsumption += Math.Max(cons, sink.MaxRequiredInputByType(_electricityId));
          }

          UpdatePowerDict(ConsumptionBlocks, block, cons, customKey);
        }
        else if (block is IMyGyro)
        {
          if ((block as IMyGyro).Enabled)
            UpdatePowerDict(ConsumptionBlocks, block, (block as MyGyro).RequiredPowerInput, "");
        }
      }

      foreach (MyCubeBlock block in _outputList)
      {
        var source = block.Components?.Get<MyResourceSourceComponent>();
        if (source != null)
        {
          var customKey = "";
          var prod = source.CurrentOutputByType(_electricityId);
          IMyBatteryBlock battery = block as IMyBatteryBlock;
          if (battery != null)
          {
            if (battery.ChargeMode == Sandbox.ModAPI.Ingame.ChargeMode.Recharge)
              continue;
            customKey = $"{_batteryName} \"{battery.ChargeMode}\"";
            CurrentPowerStats.BatteryOutput += prod;
            CurrentPowerStats.BatteryMaxOutput += source.MaxOutputByType(_electricityId);
          }
          else
          {
            CurrentPowerStats.Production += prod;
            CurrentPowerStats.MaxProduction += source.MaxOutputByType(_electricityId);
          }
          UpdatePowerDict(ProductionBlocks, block, prod, customKey);
        }
      }

      History.AddStats(CurrentPowerStats);

      if (MyAPIGateway.Multiplayer.MultiplayerActive && History.UpdatedLastIndex[4]) // every 60 seconds
      {
        var player = MyAPIGateway.Session.Player;
        var relation = (_lcdBlocks[0].OwnerId > 0 ? player.GetRelationTo(_lcdBlocks[0].OwnerId) : MyRelationsBetweenPlayerAndBlock.NoOwnership);
        if (relation == MyRelationsBetweenPlayerAndBlock.Owner)
        {
          var gridContent = GenerateGridContent().Item2;
          GameSession.Instance.NetGridHandler.Broadcast(gridContent);
        }
      }

      UpdateEvent?.Invoke();
    }
  }
}