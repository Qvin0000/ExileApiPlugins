# ExileApiPlugins
Plugins for https://github.com/Qvin0000/ExileApi



# For developers (this part in in progress, some parts could be not correct or not complete):
## All plugins compilation:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers
* Clone this repo to **HUD\ExileApi\Plugins\Source folder** (**HUD** is the root directory for **ExileApi** and **PoeHelper** folders, can be any name)
* Add plugin projects you want to compile to main solution: RMB on **solution->Add->Existing project** (maybe create a folder in solution for them)
* Build it. Plugins should be automatically builded to **HUD\PoeHelper\Plugins\Compiled** (each to own folder). Make sure they really copied to the right folder after compilation (check dll creation time or delete dll before compilation)


## Creating own plugins:
## Plugin project setup:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers
* Create a new project: 

Solution->Add->New project

Type: "**Class Library (.Net framework)**"

ProjectName: **MyPlugin**

Location: fix project location to HUD\ExileApi\Plugins\Source.  

Framework: **4.8** (Important!). (if you don't have it- install from https://dotnet.microsoft.com/download/thank-you/net48-developer-pack)

Note: You can create a folder in solution like CustomPlugins

* Create folder for ur plugin in: **HUD\PoeHelper\Plugins\Compiled\MyPlugin**
* Go to **Project->Properties->Build**. Set output path (should be FULL path!). For example **C:\\HUD\PoeHelper\Plugins\Compiled\MyPlugin**
* Add reference to **Core**(ExileApi) project: **Project.References->Add reference->Projects->Core**. 

Also add **Project.References->Add reference->Assemblies->System.Numerics** to be able to draw something.

Also you can add **PoeHelper\SharpDX.Mathematics.dll** (this gives you Vector2, RectangleF, Color), optionally to ImGui.NET.dll(if you gonna create custom menu). 

* Important! Select all ur newly added references in Project.References, go to Properties window and check CopyLocal: false.

Now you can make plugins.


## Plugin example:
Create Settings class:
```
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace MyPlugin
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Example check box", "This check box disable some functionality")]
        public ToggleNode MyCheckboxOption { get; set; } = new ToggleNode(true);
    }
}

```

Create plugin class MyPluginCore:
I recommend the plugin class name be uniq (not just Plugin or Core), in this case you will not have any problems with breakpoints is triggered in some other source files with the same name.
```
using ExileCore;
using SharpDX;

namespace MyPlugin
{
    public class MyPluginCore : BaseSettingsPlugin<Settings>
    {
        public override void Render()
        {
            DebugWindow.LogMsg("My plugin works!", 20f);
            Graphics.DrawBox(new RectangleF(10, 10, 100, 100), Color.Red);
        }
    }
}
```

# Api
Some basic stuff you may need:
```
var player = GameController.Player;
var playerPos = player.GridPos;

var life = player.GetComponent<Life>();
var myHp = life.CurHP;
```

## Base plugin functions (all funtions is optionally to override):



**Initialise** called on load once if plugin is enabled. Use it like initialization for all the stuff.
Return false if plugin can't find/load some resources, etc., and in that case OnLoad() will not be called.
```
public override bool Initialise()
{
    if(can't find/load some resource that required for a plugin)
        return false;
    return true;
}
```
**OnLoad()** Called once for each enabled plugin. Called even if plugin is disabled in options, so don't use for loading stuff.

**OnPluginDestroyForHotReload** called before plugin will be reloaded in memory (after recompilation, etc). You can abort own Threads or unsubscribe from some global events here.

**AreaChange(AreaInstance area)** called after player changed area.

**EntityAddedAny(Entity entity)** called once entity appear in range. Entity is cached and will not triggered again if entity go out of range then appear again.

**EntityAdded(Entity entity)** called once entity appear in range. Same as EntityAddedAny, but will trigger if Monster back to visible range. This is more commonly used.

**EntityRemoved(Entity entity)** Called when entity removed from cache (probably fo all entities on area change too, not sure).

**Job Tick()** Used for updating logic only.

**Render()** Main function for rendering. Perfectly do only rendering here, for logic update use Tick().

Logging:
This functions display messages on screen only (no logging to file). For file logging install package Serilog from solution packages manager.

**LogError()**

**LogMessage()**


## Not implemented functions atm:

**OnUnload()** Not implemented atm. Called on plugin close.

**OnPluginSelectedInMenu()** Called when plugins is selected in main menu (settings opened)

**EntityIgnored(Entity entity)** 


## Draw textures
I recommend to use texture atlas (tutorial later in this giude) if you have a lot of textures, this will greatly improve perfomance:
```
public override bool Initialise()
{
    var combine = Path.Combine(DirectoryFullName, "TestImage.png").Replace('\\', '/');
    Graphics.InitImage(combine, false);

    return true;
}

public override void Render()
{
   Graphics.DrawImage("TestImage.png", new RectangleF(100, 100, 1000, 100));
}
```
## Texture atlas optimization
If you have a lot of textures you can put them to texture atlas (all textures in one).
* Install **Free texture packer** http://free-tex-packer.com/download/
* Drag all your textures to program to generate an atlas (name nicelly ur image files, you will use their names in code)
* Use setting as on image

![alt text](https://github.com/Qvin0000/ExileApiPlugins/blob/master/TutorialTextures/atlas_generation.png?raw=true)

* **Export** it to **HUD\PoeHelper\Plugins\Compiled\MyPlugin\textures folder**, you should get **MyPluginAtlas.json** and **MyPluginAtlas.png**

Then just draw in code:

```
public class MyPluginCore : BaseSettingsPlugin<Settings>
{
    private AtlasTexture _iconArcingTexture;
    public override bool Initialise()
    {
        _iconArcingTexture = GetAtlasTexture("IconArcing");//IconArcing or IconArcing.png, doesn't matter, works both
        return true;
    }

    public override void Render()
    {
        Graphics.DrawImage(_iconArcingTexture, new RectangleF(10, 10, 1000, 1000));
    }
}
```

## Enabling multithreading in plugin
Plugin can work in own thread, but depends on "Threads count" parameter in hud core menu (if it is 0 there is no difference that you enabled multithreading in code). 

For enabling multithreading:
```
public override void OnLoad()
{
    CanUseMultiThreading = true;
}
        
public override Job Tick()
{
    if (Settings.MultiThreading)    //custom setting ToggleNode
    {
        return new Job(nameof(HealthBars), TickLogic);
        //return GameController.MultiThreadManager.AddJob(TickLogic, nameof(HealthBars));   //another way to enable multiprocessing
    }

    TickLogic();
    return null;
}

private void TickLogic()
{
    //Do your logic here (separate thread).
}
```
