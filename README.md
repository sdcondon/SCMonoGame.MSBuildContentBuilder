# SCMonoGame.MSBuildContentBuilder

## Usage

* Build the package project
* Reference the package in your game.
* Add one or more `.mbcbs` (MonoGame content build script) to your project, populated with code to populate the MonoGame content collection.
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
