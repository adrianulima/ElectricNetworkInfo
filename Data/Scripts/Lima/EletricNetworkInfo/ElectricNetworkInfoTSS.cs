using System;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Lima
{
  [MyTextSurfaceScript("Touch_ElectricNetworkInfo", "Electric Network Info")]
  public class ElectricNetworkInfoTSS : MyTSSCommon
  {
    public override ScriptUpdate NeedsUpdate => ScriptUpdate.Update10;

    private IMyCubeBlock _block;
    private IMyTerminalBlock _terminalBlock;
    private IMyTextSurface _surface;

    private ElectricNetworkInfoApp _app;

    bool _init = false;
    int ticks = 0;

    public ElectricNetworkInfoTSS(IMyTextSurface surface, IMyCubeBlock block, Vector2 size) : base(surface, block, size)
    {
      _block = block;
      _surface = surface;
      _terminalBlock = (IMyTerminalBlock)block;

    }

    public void Init()
    {
      if (!GameSession.Instance.Api.IsReady)
        return;

      if (_init)
        return;
      _init = true;

      var electricManager = GameSession.Instance.GetElectricManagerForBlock(_block);
      if (electricManager == null)
        return;

      _app = new ElectricNetworkInfoApp(electricManager, SaveConfigAction);
      _app.InitApp(this.Block, this.Surface);
      _app.CreateElements();
      _app.Theme.Scale = Math.Min(Math.Max(Math.Min(this.Surface.SurfaceSize.X, this.Surface.SurfaceSize.Y) / 512, 0.4f), 2);
      _app.Cursor.Scale = _app.Theme.Scale;

      var appContent = GameSession.Instance.BlockHandler.LoadAppContent(_block, _surface.Name);
      if (appContent != null)
        _app.ApplySettings(appContent.GetValueOrDefault());

      GameSession.Instance.NetBlockHandler.MessageReceivedEvent += OnBlockContentReceived;
      _terminalBlock.OnMarkForClose += BlockMarkedForClose;
    }

    private void SaveConfigAction()
    {
      var appContent = new AppContent()
      {
        SurfaceName = _surface.Name,
        Layout = _app.WindowBarButtons.CurrentLayout,
        ChartIntervalIndex = _app.OverviewPanel.ChartPanel.ChartIntervalIndex,
        BatteryChartEnabled = _app.OverviewPanel.ChartPanel.BatteryOutputAsProduction,
        ChartDataColors = _app.OverviewPanel.ChartPanel.DataColors,
        ThemeScale = _app.Theme.Scale
      };

      var blockContent = GameSession.Instance.BlockHandler.SaveAppContent(_block, appContent);
      if (MyAPIGateway.Multiplayer.MultiplayerActive && blockContent != null)
      {
        blockContent.NetworkId = MyAPIGateway.Session.Player.SteamUserId;
        GameSession.Instance.NetBlockHandler.Broadcast(blockContent);
      }
    }

    private void OnBlockContentReceived(BlockStorageContent blockContent)
    {
      if (blockContent.BlockId != _block.EntityId)
        return;

      var appContent = blockContent.GetAppContent(_surface.Name);
      if (appContent != null)
        _app.ApplySettings(appContent.GetValueOrDefault());
    }

    public override void Dispose()
    {
      base.Dispose();

      if (_init || _app != null)
      {
        GameSession.Instance.RemoveManagerFromBlock(_block);
        _app?.Dispose();
        _terminalBlock.OnMarkForClose -= BlockMarkedForClose;
        GameSession.Instance.NetBlockHandler.MessageReceivedEvent -= OnBlockContentReceived;
      }
    }

    private void BlockMarkedForClose(IMyEntity ent)
    {
      Dispose();
    }

    private MySprite GetMessageSprite(string message)
    {
      return new MySprite()
      {
        Type = SpriteType.TEXT,
        Data = message,
        RotationOrScale = 0.7f,
        Color = _surface.ScriptForegroundColor,
        Alignment = TextAlignment.CENTER,
        Size = _surface.SurfaceSize
      };
    }

    private void UpdateScale()
    {
      var ctrl = MyAPIGateway.Input.IsAnyCtrlKeyPressed();
      if (!ctrl || !_app.Screen.IsOnScreen || MyAPIGateway.Gui.IsCursorVisible)
        return;

      var plus = MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Add) || MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.OemPlus);
      var minus = false;
      if (!plus)
        minus = MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.Subtract) || MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.OemMinus);

      if (plus || minus)
      {
        var sign = plus ? 1 : -1;
        var minScale = Math.Min(Math.Max(Math.Min(this.Surface.SurfaceSize.X, this.Surface.SurfaceSize.Y) / 512, 0.4f), 1.5f);
        _app.Theme.Scale = MathHelper.Min(1.5f, MathHelper.Max(minScale, _app.Theme.Scale + sign * 0.1f));
        _app.Cursor.Scale = _app.Theme.Scale;
        SaveConfigAction();
      }
      else if (MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.NumPad0) || MyAPIGateway.Input.IsKeyPress(VRage.Input.MyKeys.D0))
      {
        _app.Theme.Scale = Math.Min(Math.Max(Math.Min(this.Surface.SurfaceSize.X, this.Surface.SurfaceSize.Y) / 512, 0.4f), 2);
        _app.Cursor.Scale = _app.Theme.Scale;
        SaveConfigAction();
      }
    }

    public override void Run()
    {
      try
      {
        var initMessage = !_init && ticks++ < (6 * 2);// 2 seconds

        if (initMessage || !Utils.IsOwnerOrFactionShare(_block, MyAPIGateway.Session.Player))
        {
          base.Run();
          using (var frame = m_surface.DrawFrame())
          {
            frame.Add(GetMessageSprite(initMessage ? "Use middle mouse to click." : "Electric Network Info\nThis Block is not shared with you!"));
            frame.Dispose();
          }
          return;
        }

        if (!_init)
          Init();

        if (_app == null)
          return;

        UpdateScale();

        base.Run();
        using (var frame = m_surface.DrawFrame())
        {
          _app.ForceUpdate();
          frame.AddRange(_app.GetSprites());
          frame.Dispose();
        }
      }
      catch (Exception e)
      {
        _app?.Dispose();
        _app = null;
        MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

        if (MyAPIGateway.Session?.Player != null)
          MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} ]", 5000, MyFontEnum.Red);
      }
    }
  }
}