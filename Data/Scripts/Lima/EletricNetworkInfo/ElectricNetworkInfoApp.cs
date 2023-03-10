using Lima.API;
using VRage.Utils;
using System.Text;
using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Lima
{
  public class ElectricNetworkInfoApp : TouchApp
  {
    private ElectricNetworkManager _electricMan;

    public View MainView;
    public WindowButtons WindowBarButtons;
    public OverviewPanel OverviewPanel;
    public EntitiesPanel EntitiesPanel;
    public SettingsView SettingsView;
    public HelpView HelpView;

    private bool _helpOpen = false;
    private bool _settingsOpen = false;

    public Action SaveConfigAction;

    public ElectricNetworkInfoApp(IMyCubeBlock block, IMyTextSurface surface, ElectricNetworkManager electricManager, Action saveConfigAction) : base(block, surface)
    {
      _electricMan = electricManager;
      _electricMan.UpdateEvent += UpdateValues;

      SaveConfigAction = saveConfigAction;

      var windowBar = new WindowBar("Electric Network Info");
      AddChild(windowBar);

      WindowBarButtons = new WindowButtons(OnChangeConfig, OnChangePage);
      windowBar.AddChild(WindowBarButtons);

      MainView = new View();
      AddChild(MainView);

      OverviewPanel = new OverviewPanel(OnChangeConfig);
      MainView.AddChild(OverviewPanel);
      OverviewPanel.CreateElements(_electricMan.History);

      EntitiesPanel = new EntitiesPanel();
      MainView.AddChild(EntitiesPanel);
      EntitiesPanel.CreateElements();

      HelpView = new HelpView();
      AddChild(HelpView);
      HelpView.CreateElements();
      HelpView.Enabled = false;

      SettingsView = new SettingsView(OverviewPanel.ChartPanel, OnChangeConfig);
      AddChild(SettingsView);
      SettingsView.CreateElements();
      SettingsView.Enabled = false;
    }

    public void OnChangePage(string page)
    {
      if (page == "help")
      {
        _helpOpen = !_helpOpen;
        _settingsOpen = false;
      }
      else if (page == "settings")
      {
        _settingsOpen = !_settingsOpen;
        _helpOpen = false;
        SettingsView.UpdateAppThemeColors();
        SettingsView.OnClickCancel();
      }
      else
      {
        _settingsOpen = false;
        _helpOpen = false;
      }

      MainView.Enabled = !_helpOpen && !_settingsOpen;
      SettingsView.Enabled = !_helpOpen && _settingsOpen;
      HelpView.Enabled = _helpOpen && !_settingsOpen;

      WindowBarButtons.LayoutButton.Enabled = MainView.Enabled;
    }

    public void OnChangeConfig()
    {
      SaveConfigAction();
      UpdateLayout();
    }

    public void ApplySettings(AppContent content)
    {
      var themeScale = content.ThemeScale ?? 0;
      Theme.Scale = themeScale > 0 ? themeScale : 1;
      Cursor.Scale = Theme.Scale;
      WindowBarButtons.CurrentLayout = content.Layout;
      OverviewPanel.ApplySettings(content, _electricMan);

      UpdateLayout();
      UpdateValues();
    }

    public void UpdateLayout()
    {
      switch (WindowBarButtons.CurrentLayout)
      {
        default:
        case 0:
          MainView.Direction = ViewDirection.Column;
          OverviewPanel.Enabled = true;
          EntitiesPanel.Enabled = true;
          break;
        case 1:
          MainView.Direction = ViewDirection.Row;
          OverviewPanel.Enabled = true;
          EntitiesPanel.Enabled = true;
          break;
        case 2:
          MainView.Direction = ViewDirection.Column;
          OverviewPanel.Enabled = true;
          EntitiesPanel.Enabled = false;
          break;
        case 3:
          MainView.Direction = ViewDirection.Column;
          OverviewPanel.Enabled = false;
          EntitiesPanel.Enabled = true;
          break;
      }

      if (EntitiesPanel.Enabled)
        EntitiesPanel.ResetScrolls();
    }

    int _skipTicks = 1000;
    public void UpdateValues()
    {
      if (OverviewPanel.Enabled)
        OverviewPanel.UpdateValues(_electricMan);

      // Skip updates if not aiming the screen for more than 5s
      if (!Screen.IsOnScreen && ++_skipTicks < 5)
        return;
      _skipTicks = 0;

      if (EntitiesPanel.Enabled)
        EntitiesPanel.UpdateValues(_electricMan);
    }

    public void Dispose()
    {
      ForceDispose();
      OverviewPanel?.Dispose();
      EntitiesPanel?.Dispose();
      if (_electricMan != null)
        _electricMan.UpdateEvent -= UpdateValues;
    }

    public static string PowerFormat(float MW, string decimals = "0.##")
    {
      if (MW >= 1000000000000)
        return $"{MW.ToString("E2")} MW";
      if (MW >= 1000000000)
        return $"{(MW / 1000000000).ToString(decimals)} PW";
      if (MW >= 1000000)
        return $"{(MW / 1000000).ToString(decimals)} TW";
      if (MW >= 1000)
        return $"{(MW / 1000).ToString(decimals)} GW";
      if (MW >= 1)
        return $"{MW.ToString(decimals)} MW";
      if (MW >= 0.001)
        return $"{(MW * 1000f).ToString(decimals)} kW";
      return $"{(MW * 1000000f).ToString(decimals)} W";
    }

    private readonly static StringBuilder _str = new StringBuilder();
    public static string HoursFormat(float hours, string decimals = "0.##")
    {
      if (hours > 24 * 365)
        return "1 year +";

      _str.Clear();
      MyValueFormatter.AppendTimeInBestUnit(hours * 3600f, _str);
      return _str.ToString();
    }
  }
}