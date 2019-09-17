# ExileApiPlugins

## For developers:
# All plugins compilation:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers
* Clone this repo to HUD\ExileApi\Plugins\Source folder (HUD is the root directory for ExileApi and PoeHelper folders, can be any name)
* Add plugin projects you want to compile to main solution: RMB on solution->Add->Existing project (maybe create a folder in solution for them)
* Build it. Plugins should be automatically builded to HUD\PoeHelper\Plugins\Compiled (each to own folder). Make sure they really copied to the right folder after compilation (check dll creation time or delete dll before compilation)


## Creating own plugins:
# Plugin project setup:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers
* Create a new project: 

Solution->Add->New project

Type: "Class Library (.Net framework)"

ProjectName: MyPlugin

Location: fix project location to HUD\ExileApi\Plugins\Source.  

Framework: 4.8 (Important!). (if you don't have it- install from https://dotnet.microsoft.com/download/thank-you/net48-developer-pack)

Note: You can create a folder in solution like CustomPlugins

* Create folder for ur plugin in: HUD\PoeHelper\Plugins\Compiled\MyPlugin
* Go to Project->Properties->Build. Set output path (should be FULL path!). For example C:\\HUD\PoeHelper\Plugins\Compiled\MyPlugin
* Add reference to Core(ExileApi) project: Project.References->Add reference->Projects->Core. 

Also add Project.References->Add reference->Assemblies->System.Numerics to be able to draw something.

Also you can add PoeHelper\SharpDX.Mathematics.dll (this gives you Vector2, RectangleF, Color), optionally to ImGui.NET.dll(if you gonna create custom menu). 

* Important! Select all ur newly added references in Project.References, go to Properties window and check CopyLocal: false.

Now you can make plugins.


# Plugin example:
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

  
