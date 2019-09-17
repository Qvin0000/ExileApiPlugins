# ExileApiPlugins

## For developers:
# All plugins compilation:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers
* Clone this repo to HUD\ExileApi\Plugins\Source folder (HUD is the root directory for ExileApi and PoeHelper folders, can be any name)
* Add plugin projects you want to compile to main solution: RMB on solution->Add->Existing project (maybe create a folder in solution for them)
* Build it. Plugins should be automatically builded to HUD\PoeHelper\Plugins\Compiled (each to own folder). Make sure they really copied to the right folder after compilation (check dll creation time or delete dll before compilation)


## Creating own plugins:
# Plugin project setup:
* Setup solution as described here: https://github.com/Qvin0000/ExileApi/blob/master/README.md#for-developers (probably you can skip this and add references to dlls (ExileCore.dll instead Core(ExileApi) project) and other dlls.
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

Also you can add PoeHelper\SharpDX.Mathematics.dll (this gives you Vector2/3), optionally to ImGui.NET.dll(if you gonna create custom menu). 


Now you can make plugins.
