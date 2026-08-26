using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SCGames.Common.Utilities.ProgressHandling;
using SCGames.MonoGame.Components.InputHandling;
using SCGames.MonoGame.Components.InputHandling.Signals;
using SCGames.MonoGame.Components.ScreenManagement;
using SCGames.MonoGame.ComponentServices.Basic3d.Primitives;
using SCGames.MonoGame.ComponentServices.Cameras;
using SCGames.MonoGame.GameServices;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCGames.MonoGame.MSBuildContentBuilder.Demo.Screens;

internal class MainScreen : ScreenBase, IDisposable
{
    private const string modelAssetName = "Models/suzanne";
    private const string fontAssetName = "Fonts/Roboto/Roboto";

    private static int NextInstanceId = 1;

    private readonly ScreenContext context;
    private readonly int instanceId;
    private readonly Color bgColor;
    private readonly InputMonitor inputMonitor;
    private readonly IInputSignal<bool> screenSwitchSignal;
    private readonly IInputSignal<bool> resetCameraSignal;
    private readonly SpriteBatch spriteBatch;
    private readonly StringBuilder helpTextBuilder = new();
    private readonly BasicEffect linesEffect;
    private readonly MutableColoredLineList lines;
    private readonly ControllableOrbitCameraAligned camera;

    private SpriteFont font;
    private Model model;

    public MainScreen(ScreenContext context)
    {
        this.context = context;
        instanceId = NextInstanceId++;
        bgColor = new(Random.Shared.NextSingle() * .5f, Random.Shared.NextSingle() * .5f, Random.Shared.NextSingle() * .5f);
        inputMonitor = context.Services.GetRequiredService<InputMonitor>();
        screenSwitchSignal = WasJustReleasedSignal.Create(inputMonitor, Keys.Space, this);
        resetCameraSignal = WasJustPressedSignal.Create(inputMonitor, Keys.D0, this);

        spriteBatch = new SpriteBatch(context.GraphicsDevice);
        lines = new(context.GraphicsDevice)
        {
            new(),
            new()
        };
        linesEffect = new(context.GraphicsDevice)
        {
            VertexColorEnabled = true,
        };

        camera = new ControllableOrbitCameraAligned(context.GraphicsDevice, Math.PI / 4, .01, 10, 2.5)
        {
            PanUp = IsPressedSignal.Create(inputMonitor, Keys.W, this),
            PanDown = IsPressedSignal.Create(inputMonitor, Keys.S, this),
            PanLeft = IsPressedSignal.Create(inputMonitor, Keys.A, this),
            PanRight = IsPressedSignal.Create(inputMonitor, Keys.D, this),
            Pan = MouseDragSignal.Create(inputMonitor, MouseButtons.Middle, new(-.01f, .01f), this),
            Zoom = MouseScrollWheelDeltaSignal.Create(inputMonitor, this),
            RotationSpeedBase = 0.6f,
            ZoomMinDistance = 1f,
            ZoomBase = 0.9f,
            MaxZoomLevel = 10,
            MinZoomLevel = -6,
        };
    }

    protected override async Task InitializeCoreAsync(IProgressHandler progress, CancellationToken cancellationToken = default)
    {
        var loadTime = TimeSpan.FromSeconds(Random.Shared.Next(1, 4));
        Trace($"Instance loading (with {loadTime.Seconds}s delay to simulate significant content load time).");
        Stopwatch loadStopwatch = Stopwatch.StartNew();

        // Simulate a slow-loading screen
        const int loadIncrements = 20;
        for (var i = 0; i < loadIncrements; i++)
        {
            await Task.Delay(loadTime / loadIncrements, cancellationToken).ConfigureAwait(false);
            progress.Report(new Progress((float)i / loadIncrements, $"{i * 100 / loadIncrements}% complete"));

            ////if (Random.Shared.Next(20) == 0)
            ////{
            ////    throw new InvalidOperationException("Demo go BOOM!");
            ////}
        }

        font = context.Content.Load<SpriteFont>(fontAssetName);

        model = context.Content.Load<Model>(modelAssetName);
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
            {
                effect.TextureEnabled = false;
                effect.EnableDefaultLighting();
                effect.World = Matrix.CreateRotationX((float)-Math.PI / 2);
            }
        }

        Trace($"Instance #{instanceId} loaded in {loadStopwatch.ElapsedMilliseconds}ms.");
    }

    public override void Update(GameTime gameTime)
    {
        camera.Update(gameTime);

        if (resetCameraSignal.Value)
        {
            camera.PanTo(0, 0, 10f);
        }

        if (!inputMonitor.IsMouseCapturedByOtherThan(this))
        {
            if (inputMonitor.WasJustPressed(MouseButtons.Left))
            {
                var ray = camera.CastRayToScreenPoint(inputMonitor.CurrentMouseState.Position, context.GraphicsDevice);
                VertexPositionColor p1 = new(ray.Position, Color.Red);
                VertexPositionColor p2 = new(ray.Position + ray.Direction * 5f, Color.Red);
                lines.Add(new(p1, p2));
            }

            if (inputMonitor.WasJustPressed(MouseButtons.Right))
            {
                lines.Clear();
            }
        }

        if (screenSwitchSignal.Value)
        {
            context.SwitchTo<MainScreen>();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        context.GraphicsDevice.Clear(bgColor);

        context.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        context.GraphicsDevice.BlendState = BlendState.Opaque;

        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                var basicEffect = (BasicEffect)effect;
                basicEffect.Projection = camera.Projection;
                basicEffect.View = camera.View;
            }

            mesh.Draw();
        }

        linesEffect.Projection = camera.Projection;
        linesEffect.View = camera.View;
        linesEffect.CurrentTechnique.Passes[0].Apply();
        lines.Draw();

        helpTextBuilder.Clear();
        helpTextBuilder.AppendLine($"MAIN SCREEN, INSTANCE #{instanceId}");
        helpTextBuilder.AppendLine();
        helpTextBuilder.AppendLine("TILDE: Toggle console (enter 'help' for command list)");
        helpTextBuilder.AppendLine();
        helpTextBuilder.AppendLine("LMB: Cast ray to mouse cursor position");
        helpTextBuilder.AppendLine("RMB: Clear all rays");
        helpTextBuilder.AppendLine("MMB+Mouse Movement: Rotate camera");
        if (!inputMonitor.IsKeyboardCapturedByOtherThan(this))
        {
            helpTextBuilder.AppendLine("W/S/A/D: Rotate camera");
            helpTextBuilder.AppendLine("0: Reset camera");
            helpTextBuilder.AppendLine();
            helpTextBuilder.AppendLine("SPACE: Switch to new copy of this main screen");
        }

        spriteBatch.Begin();
        spriteBatch.DrawString(font, helpTextBuilder, new(10, 10), Color.White);
        spriteBatch.End();
    }

    public void Dispose()
    {
        context.Content.UnloadAsset(modelAssetName);
        GC.SuppressFinalize(this);
        Trace($"Instance #{instanceId} disposed.");
    }
}
