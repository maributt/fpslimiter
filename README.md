
# FPS Limiter

A [Dalamud](https://github.com/goatcorp/Dalamud) plugin which allows you to cap your FPS using commands.

  

## Usage

To set your FPS cap to either: *None*, *Refresh Rate 1/1*, *Refresh Rate 1/2* **...** *Refresh Rate 1/4*
`/fps <0..4>`

You can remember it by thinking of the number as the FPS cap divider, e.g:
 `/fps 2` - Sets your FPS cap to Refresh Rate 1/**2**

You may also return to your previous FPS cap by using `/fps previous` or by typing the command for a same FPS cap twice (mimics the effect of using `/afk`)

## Demo

![Demonstration](https://cdn.discordapp.com/attachments/599442186056237086/792866438528368701/demo.gif)

## Planned features
- Add a way to set your FPS to frame accurate caps: `/fps 0..200..`
