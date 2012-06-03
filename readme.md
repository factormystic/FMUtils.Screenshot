    _________  ____   _ _   _ _       _____                              _           _   
    |  ___|  \/  | | | | | (_) |     /  ___|                            | |         | |  
    | |_  | .  . | | | | |_ _| |___  \ `--.  ___ _ __ ___  ___ _ __  ___| |__   ___ | |_ 
    |  _| | |\/| | | | | __| | / __|  `--. \/ __| '__/ _ \/ _ \ '_ \/ __| '_ \ / _ \| __|
    | |   | |  | | |_| | |_| | \__ \_/\__/ / (__| | |  __/  __/ | | \__ \ | | | (_) | |_ 
    \_|   \_|  |_/\___/ \__|_|_|___(_)____/ \___|_|  \___|\___|_| |_|___/_| |_|\___/ \__|


### A screenshot library for ProSnap
(but you can use it for whatever - see _license.txt_)


### Nuget
    Install-Package FMUtils.Screenshot


### State of the Code
- Mostly operational


### Usage
Create a screenshot by creating a new `Screenshot` object.
Of a window (by handle):

    var WindowScreenshot = new ComposedScreenshot(targetWindowHandle, ScreenshotMethod.DWM, true);

or, of a rect:

    var RectScreenshot = new ComposedScreenshot(new Rectangle(0,0,100,100));

The `Screenshot` class is only responsible for creating the raw screenshot object.
This creates an object with various useful metadata properties (screen location, window title, etc). `BaseScreenshotImage` is a bitmap with the image data.

If you want a slightly nicer endpoint, that it's accomplished with `ComposedScreenshot`.
This class has more advanced functionality such as maintaining a composition stack, rounding corners, and adding a shadow effect.

See **[ProSnap](https://github.com/factormystic/ProSnap)** for an example of how I built an end-user application out of it.


### Where you can help
**FMUtils.Screenshot** can create screenshots either by copying the raw screen image, or by creating a backing form, registering a 1:1 sized DWM thumbnail, and copying that.
The latter method has the pleasant side effect of making Aero-glass effects opaque.

Other screenshot applications use slightly more advanced methods, such as taking two screenshots with the backing form alternately black then white, the combination of which can be used to determine per-pixel opacity.
That would be a great feature for this library as well, so it's an open task for anywho who wants to take a crack.
