using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SCGames.Common.GameServices.Diagnostics;
using SCGames.Common.GameServices.Modularity;
using SCGames.Common.GameServices.Modularity.Diagnostics;
using SCGames.Common.GameServices.Persistence;
using SCGames.MonoGame.Components.Diagnostics;
using SCGames.MonoGame.Components.InputHandling;
using SCGames.MonoGame.Components.ScreenManagement;
using SCGames.MonoGame.Components.ScreenManagement.Diagnostics;
using SCGames.MonoGame.GameServices.ContentManagement;
using SCGames.MonoGame.GameServices.ContentManagement.Diagnostics;
using SCGames.MonoGame.GameServices.Modularity;
using SCGames.MonoGame.MSBuildContentBuilder.Demo.Screens;
using System;
using System.IO;
using System.Linq;

namespace SCGames.MonoGame.MSBuildContentBuilder.Demo;

public class Program : Game
{
    private Program()
    {
        // Standard MonoGame stuff - window setup & graphics device initialisation:
        Window.Title = "SCMonoGame Demo App";
        Window.AllowUserResizing = true;
        IsMouseVisible = true;

        // NB: Under DX12, the "default" adapter isn't necessarily one we want to grab
        // current display mode from. So check for the first one with a monitor handle instead.
        var displayAdapter = GraphicsAdapter.Adapters.First(a => a.SupportedDisplayModes.Any());
        GraphicsDeviceManager graphicsDeviceManager = new(this)
        {
            PreferredBackBufferWidth = displayAdapter.CurrentDisplayMode.Width,
            PreferredBackBufferHeight = displayAdapter.CurrentDisplayMode.Height,
            IsFullScreen = true
        };
        graphicsDeviceManager.ApplyChanges();

        // The library provides a custom ContentManager type, which adds a bunch of features to
        // the base class. Note that we register it as a service by its concrete type as well -
        // makes it easier to retrieve as its concrete type. Though of course you could also just
        // grab the game's Content property and cast it).
        ModularContentManager modularContentManager = new(Services, "Content");
        Content = modularContentManager;
        Services.AddService(modularContentManager);
    }

    /// <summary>
    /// The program entry point.
    /// </summary>
    public static void Main()
    {
        using var game = new Program();
        game.Run();
    }

    protected override void Initialize()
    {
#if WINDOWSDX12
        // On WindowsDX12, the Graphics Monitor commands need to queue up some work on the
        // main update thread (can only access the game's Window on the thread that created
        // it). Can use sync task manager to do this. Its not needed on DESKTOPVK:
        Components.Add(new SynchronisedTaskManager(this));
#endif

        // Set up CLI service first, to capture any traces from
        // setup of other components. We also register some commands
        // for querying app state.
        CommandLineInterface cli = new();
        cli.AddGraphicsMonitorCommands(this);
        cli.AddPerformanceMonitorCommands(this);
        cli.AddModDiagnosticCommands();
        cli.AddContentManagerCommands(this);
        cli.AddScreenManagerCommands(this);
        Services.AddService(cli);

        // Initialize mod registry
        AppDataManager appData = new();
        ModRegistry modRegistry = ModRegistry.Create(
        [
            Path.Combine(appData.UserDirectory.FullName, "Mods")
        ]);

        // Initialize all discovered mods with a basic MonoGameModContext.
        // In general, the idea is to create an app-specific context type derived
        // from MonoGameModContext that includes public methods that mods can use
        // to set themselves up in the context of the game (usually, events to register
        // handlers for). Here though, we don't bother with any particular sub-type -
        // our mod doesn't actually do anything.
        MonoGameModContext modContext = new((ModularContentManager)Content);
        foreach (var mod in modRegistry.Mods)
        {
            mod.Initialize(modContext);
        }

        // Set up the input monitor. Note that this is both a component (it needs to be updated every tick)
        // and a service (it is intended as something that various components can make use of):
        InputMonitor inputMonitor = new(this);
        Services.AddService(inputMonitor);
        Components.Add(inputMonitor);

        // Set up the CLI panel component. Do so *after* the input monitor and *before* the screen manager,
        // so that, without needing to manually set UpdateOrders, the order of operations is as follows:
        // the input monitor resets input capture state, then the panel captures it if it is accepting input,
        // thus taking priority over input into screens. NB: CliPanel sets its own DrawOrder to int.MaxValue
        // as it instantiates, so that the panel is drawn on top by default.
        Components.Add(new CliPanel(this, "Fonts/Roboto/Roboto"));

        // Finally, set up the screen manager component, and tell it to show the main screen.
        // This component (or rather, the screens that it hosts), does the bulk of the "real"
        // work as the game progresses:
        var screenManager = new ScreenManager(this, typeof(LoadingScreen))
        {
            ScreenInitializationGracePeriod = TimeSpan.FromSeconds(1),
        };
        screenManager.Show<MainScreen>();
        Components.Add(screenManager);

        // Don't forget to call base.Initialize so that all the components we added above get initialized.
        base.Initialize();
    }
}