using System;
using System.Collections.Generic;
using Lima.API;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Lima
{
  [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
  public class GameSession : MySessionComponentBase
  {
    public TouchUiKit Api { get; private set; }
    public static GameSession Instance;

    private List<ElectricNetworkManager> _electricManagers;
    public BlockStorageHandler BlockHandler;
    public GridStorageHandler GridHandler;
    public NetworkHandler<BlockStorageContent> NetBlockHandler;
    public NetworkHandler<GridStorageContent> NetGridHandler;

    public override void LoadData()
    {
      // For server and clients
      BlockHandler = new BlockStorageHandler();
      GridHandler = new GridStorageHandler();
      NetBlockHandler = new NetworkHandler<BlockStorageContent>(041414);
      NetBlockHandler.Init();
      NetGridHandler = new NetworkHandler<GridStorageContent>(011414);
      NetGridHandler.Init();
      NetGridHandler.AutoBroadcastEnabled = false;

      Instance = this;

      if (MyAPIGateway.Utilities.IsDedicated)
      {
        // Only for server
        NetBlockHandler.MessageReceivedEvent += NetwrokBlockReceivedServer;
        NetGridHandler.MessageReceivedEvent += NetwrokGridReceivedServer;
        return;
      }

      // Only for clients
      Api = new TouchUiKit();
      Api.Load();
    }

    private void NetwrokBlockReceivedServer(BlockStorageContent blockContent)
    {
      var block = MyAPIGateway.Entities.GetEntityById(blockContent.BlockId) as IMyCubeBlock;
      if (block != null)
        BlockHandler.SaveBlockContent(block, blockContent);
    }

    private void NetwrokGridReceivedServer(GridStorageContent gridContent)
    {
      var grid = MyAPIGateway.Entities.GetEntityById(gridContent.GridId) as IMyCubeGrid;
      if (grid != null)
        GridHandler.SaveGridContent(grid, gridContent);
    }

    public ElectricNetworkManager GetElectricManagerForBlock(IMyCubeBlock lcdBlock)
    {
      if (MyAPIGateway.Utilities.IsDedicated)
        return null;

      if (_electricManagers != null)
      {
        foreach (var manager in _electricManagers)
          if (manager.AddBlockIfSameGrid(lcdBlock))
            return manager;
      }
      else
        _electricManagers = new List<ElectricNetworkManager>();

      var newManager = new ElectricNetworkManager(lcdBlock);
      _electricManagers.Add(newManager);
      return newManager;
    }

    public void RemoveManagerFromBlock(IMyCubeBlock lcdBlock)
    {
      if (_electricManagers != null)
      {
        foreach (var manager in _electricManagers)
          if (manager.RemoveBlockAndCount(lcdBlock))
          {
            RemoveManager(manager);
            return;
          }
      }
    }

    public void RemoveManager(ElectricNetworkManager manager)
    {
      manager.Dispose();
      if (_electricManagers != null)
        _electricManagers.Remove(manager);
    }

    protected override void UnloadData()
    {
      if (_electricManagers != null)
        foreach (var manager in _electricManagers)
          manager.Dispose();

      if (NetBlockHandler != null)
        NetBlockHandler.MessageReceivedEvent -= NetwrokBlockReceivedServer;
      if (NetGridHandler != null)
        NetGridHandler.MessageReceivedEvent -= NetwrokGridReceivedServer;
      NetBlockHandler?.Dispose();
      NetGridHandler?.Dispose();
      Api?.Unload();
      Instance = null;
      _electricManagers = null;
    }

    bool wasPaused = false;
    double prevTime = 0;
    public override void UpdateAfterSimulation()
    {
      if (MyAPIGateway.Utilities.IsDedicated)
        return;

      if (wasPaused)
      {
        prevTime = MyAPIGateway.Session?.ElapsedPlayTime.TotalSeconds ?? 0;
        wasPaused = false;
      }

      // This make the code ruturns if less than one second
      var secs = MyAPIGateway.Session?.ElapsedPlayTime.TotalSeconds ?? 0;
      var diff = prevTime == 0 ? 0 : secs - prevTime - 1;
      if (diff < 0)
        return;
      prevTime = secs - diff;

      try
      {
        if (_electricManagers != null)
          foreach (var manager in _electricManagers)
            manager.Update();
      }
      catch (Exception e)
      {
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

        if (MyAPIGateway.Session?.Player != null)
          MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} ]", 5000, MyFontEnum.Red);
      }
    }

    public override void UpdatingStopped()
    {
      base.UpdatingStopped();
      wasPaused = true;
    }

    public override MyObjectBuilder_SessionComponent GetObjectBuilder()
    {
      if (_electricManagers != null)
      {
        foreach (var manager in _electricManagers)
        {
          var gridTuple = manager.GenerateGridContent();
          GridHandler.SaveGridContent(gridTuple.Item1, gridTuple.Item2);
        }
      }
      return base.GetObjectBuilder();
    }
  }
}