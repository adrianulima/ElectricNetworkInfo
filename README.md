# Electric Network Info

Electric Network Info is an app for LCD screens in [Space Engineers](https://www.spaceengineersgame.com/) that provides real-time monitoring of your grid's electric production, consumption, battery storage, and detailed information per block. The app saves a history of the last 30 minutes of data and presents this information as a chart with a configurable interval, helping you optimize your power management.

![image](https://github.com/user-attachments/assets/173da7b6-4ba2-4357-907a-6093d1b4a6a6)

## How to Install

1. Visit the [Steam Workshop page](https://steamcommunity.com/sharedfiles/filedetails/?id=2917216762) for Electric Network Info and subscribe to the mod and its dependency, the [TouchScreenAPI](https://steamcommunity.com/sharedfiles/filedetails/?id=2668820525).
2. Launch Space Engineers and navigate to the save game settings.
3. Activate both the Electric Network Info mod and the TouchScreenAPI mod in your active mods list.
4. Open any LCD Block's Control Panel.
5. Change the `Content` property to `Script`.
6. Select "Electric Network Info" from the list.

## Features

- **Consumption:** Displays current consumption and potential maximum consumption if all blocks require maximum input.
- **Production:** Shows current production and potential maximum output if all producers are operating at full capacity.
- **Battery Output:** Indicates current power production of batteries and their maximum output.
- **Chart Time Span:** Configurable time span for the graph, e.g., "30s" represents data over the last 30 seconds.
- **Battery Checkbox:** When checked, battery output is counted as production in the graph.
- **Battery Storage:** Displays the estimated time until the battery is depleted. A red border indicates system overload and the need for increased power production.
- **Detailed Consumption and Production:** Lists consumers and producers in order from highest to lowest power usage or output.
- **Window Bar Buttons:**
  - **Help Button:** Opens the help panel.
  - **Gears Button:** Opens settings to customize how information is displayed.

## Screens and Touch

The app adapts to nearly all LCDs in the vanilla game and DLCs, including cockpits. However, due to small buttons and labels, readability may be challenging on smaller screens. For best results on modded LCDs, use the Screen Calibration app to optimize the display.

The touch screen feature is powered by the [TouchScreenAPI mod](https://github.com/adrianulima/TouchScreenAPI), which provides both the cursor functionality and UI elements accessible to any modder. For further assistance, feel free to reach out via GitHub, Steam, or Discord (@adrianolima).

## Multiplayer and Servers

Electric Network Info works seamlessly in single-player, multiplayer, and server environments. As a TSS (LCD script), it runs mostly on the client side, with clients responsible for drawing and monitoring the grids. Data is sent to the server solely for persistence across sessions. Access to the app is restricted to players with permission to interact with the block; sharing with a faction allows faction members to use it as well.

## Changing the Scale

Adjust the app's scale using the following shortcuts:

- **Ctrl + Plus:** Increase scale.
- **Ctrl + Minus:** Decrease scale.
- **Ctrl + 0:** Reset to default scale.

This feature is particularly useful for optimizing visibility on smaller screens.

## Performance and Limitations

Currently, the app runs at 6 FPS due to game-imposed limitations on LCD screen texture updates. While a potential workaround exists to increase the refresh rate to 30 FPS, the initial release prioritizes performance stability. Future versions may include enhancements to improve refresh rates.

## Credits

- Developed by [Adriano Lima](https://github.com/adrianulima)
- Special thanks to the Space Engineers modding community for their continuous support and feedback.

> [!IMPORTANT]
> This mod is not affiliated with or endorsed by Keen Software House. It is a fan-made project developed independently for Space Engineers.

---

For any issues or suggestions, please contact me on [GitHub](https://github.com/adrianulima), [Steam](https://steamcommunity.com/id/adrianulima), or Discord (@adrianolima).
