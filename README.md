# GammaTuner
## About
- GammaTuner allows you to manually edit your gamma table via an interactive graph.
- Supports exporting/importing profiles and auto-switching when toggling between SDR/HDR.
- Can be set to run at startup.
- The app uses WindowsDisplayAPI, so it should support any GPU.
- This app was mostly designed to fix black crush by hand; there will be a bit of a learning curve. Will results be precise? Not really. Can it still improve things to YOUR desire? Maybe.

![image](https://github.com/user-attachments/assets/3f526756-13e9-4f7b-9792-3f22b85d18cc)

## Why this app?
Black crush is a common issue for many monitors. Common fixes include changing settings such as gamma, contrast, brightness, or monitor settings like "shadow detail", but these often have undesired side effects (e.g. raising the black level so you can't get pure blacks anymore). This app allows you to set the brightness at any point in the gamma table, so you can target and fix the exact brightness levels you're having trouble with. 

Other methods such as color profiles also exist, but they can be unfriendly to obtain, create, or apply. Not everyone has calibration hardware. 

Additionally, most existing methods require you to manually undo them if you switch between SDR/HDR and don't want them in one of the modes, which is not convenient. This app allows you to use a separate gamma profile for SDR and HDR, as well as automatically switch between them.

## How to use
- Have a display beside you with your desired/better accuracy than the display you want to tune (your primary display).
- Modify the gamma chart (shadow detail on the graph is mostly between X=0 to X=50). Alternate between the "Apply Default Gamma" and "Apply Gamma" buttons to quickly A/B-compare your custom gamma and default gamma.
- These YT videos are good for testing black crush (open them on both monitors):
- [HDR Black Crush Test](https://www.youtube.com/watch?v=wn517192hO4)
- [Darkest Hour Shadow Detail Test (HDR)](https://www.youtube.com/watch?v=z092wdyrZZQ)
- [Kyoto, Japan - 21:9 Ultrawide 4K HDR - Gion at Night - Cinematic Short](https://www.youtube.com/watch?v=FJLAnvSCieA)
- Aside from the videos, find some (very dark) test images and open them on both monitors. If you don't have them, you can take HDR screenshots with WIN+SHIFT+PrntScrn (Xbox Game Bar), NVIDIA Shadowplay, or some other tools.
- - Here's a selection of test images from various games: [https://drive.google.com/file/d/1WhJdO8Fv5SEKEFAtqgdD1i7SjCMx_rQw/view?usp=sharing](https://drive.google.com/file/d/1WhJdO8Fv5SEKEFAtqgdD1i7SjCMx_rQw/view?usp=sharing)
- Viewing HDR images: Windows Photos might already be able to display HDR Screenshots (.jxr), but I prefer the tool "HDR + WCG Image Viewer."

## Known Issues
- Windows Advanced Color Management (ACM) will make the app think it's always in HDR, which breaks the profile auto-switcher.

## Credits
- [NVCP Toggle](https://github.com/mcgrizzz/NVCP_Toggle) for code inspiration.
- [ScottPlot](https://github.com/ScottPlot/ScottPlot)
- [WindowsDisplayAPI](https://github.com/falahati/WindowsDisplayAPI)

## Disclaimers
This app is experimental. I take no responsibility if it renders your system unusable. 
