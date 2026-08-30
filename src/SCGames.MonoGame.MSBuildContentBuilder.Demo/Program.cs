using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace SCGames.MonoGame.MSBuildContentBuilder.Demo;

public class Program : Game
{
    private const string modelAssetName = "Models/suzanne";
    private const string fontAssetName = "Fonts/Roboto/Roboto";
    private readonly SpriteBatch spriteBatch;

    private SpriteFont font;
    private Model model;

    private Program()
    {
        // Standard MonoGame stuff - window setup & graphics device initialisation:
        Window.Title = "MSBuild Content Builder Demo App";
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

        Content.RootDirectory = "Content";
        spriteBatch = new(graphicsDeviceManager.GraphicsDevice);
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
        font = Content.Load<SpriteFont>(fontAssetName);

        model = Content.Load<Model>(modelAssetName);
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects.Cast<BasicEffect>())
            {
                effect.TextureEnabled = false;
                effect.EnableDefaultLighting();
                effect.World = Matrix.CreateRotationX((float)-Math.PI / 2);
            }
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        //GraphicsDevice.BlendState = BlendState.Opaque;

        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (var effect in mesh.Effects)
            {
                var basicEffect = (BasicEffect)effect;
                basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4, GraphicsDevice.Viewport.AspectRatio, 0.1f, 4);
                basicEffect.View = Matrix.CreateLookAt(new(0, 0, 3), Vector3.Zero, Vector3.UnitY);
            }

            mesh.Draw();
        }

        spriteBatch.Begin();
        spriteBatch.DrawString(font, "Example Text", new(10, 10), Color.White);
        spriteBatch.End();
    }
}