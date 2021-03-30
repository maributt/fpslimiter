<h1 align="center">FPSLimiter</h1>
<p align="center">A <a href="https://github.com/goatcorp/Dalamud">Dalamud</a> plugin to manage your FPS cap with a command.<br>Type `/fps` in-game to learn more!</p><br>

<img src="https://user-images.githubusercontent.com/76499752/112993515-95750580-9169-11eb-9df9-58f5309fea19.gif" width="40%" align="right">
The goal of this plugin is to implement a simple command
in FFXIV allowing the player to set their framerate
to whichever cap they wish.

It only adds a single new command `/fps` and accepts
a single argument at most: the desired FPS cap.
Setting the FPS cap to anything below 5 will, instead
of setting the absolute FPS cap to the given number,
default to capping the framerate using in-game means...

This means if the player issues the command `/fps 0`
their framerate will be uncapped through the "Frame Rate"
section of the System Configuration menu instead
of being set to a maximum of 0.

<br>
<br>

## Planned features

- **Add** an alias for toggling on and off...
    - Limit frame rate when client is inactive
    - Limit FPS when away from keyboard
- **Reflect** the Refresh Rate cap changes in the System Configuration menu
