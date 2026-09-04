# SCMonoGame.MSBuildContentBuilder

A MonoGame 3.8.5 content builder that runs during the MSBuild process. Finds C# script files in the project to populate the content collection.

## Usage

* Build the package project. (I've no current plans to publish this on NuGet - since I've no real desire to add another package that I need to maintain).
* Reference the package in your game.
* Add one or more `.mgcbs` (MonoGame content build script) files to your project, containing C# code to populate the MonoGame content collection.
  The content collection is provided to the script as the global, meaning your script might look something like:
  ```
  Include("Models/suzanne.fbx", new FbxImporter(), new ModelProcessor()
  {
	  ColorKeyEnabled = true,
	  DefaultEffect = MaterialProcessorDefaultEffect.BasicEffect,
    ..
  });

  Include<WildcardRule>("Shaders/*.fx", new EffectImporter(), new EffectProcessor());
  ```
* If needed, add MSBuild build properties to your project to be passed along to the content build - see the [.targets file](src/SCMonoGame.MSBuildContentBuilder/_PackageFiles/build/SCMonoGame.MSBuildContentBuilder.targets) for details.
