using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SCGames.Common.Utilities.ProgressHandling;
using SCGames.MonoGame.Components.ScreenManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCGames.MonoGame.MSBuildContentBuilder.Demo.Screens;

internal class LoadingScreen(ScreenContext context) : ScreenBase
{
    private const string fontAssetName = "Fonts/Roboto/Roboto";

    private readonly StringBuilder helpTextBuilder = new();
    private readonly SpriteBatch spriteBatch = new(context.GraphicsDevice);

    private SpriteFont font;

    public override void Draw(GameTime gameTime)
    {
        context.GraphicsDevice.Clear(Color.DarkBlue);

        helpTextBuilder.Clear();
        helpTextBuilder.AppendLine("LOADING SCREEN");
        helpTextBuilder.AppendLine();
        helpTextBuilder.AppendLine("TILDE: Toggle console (enter 'help' for command list)");
        helpTextBuilder.AppendLine($"Load Progress: {context.NextScreenLoadingProgress.Value}");
        helpTextBuilder.AppendLine($"Load Progress Description: {context.NextScreenLoadingProgressDescription}");

        spriteBatch.Begin();
        spriteBatch.DrawString(font, helpTextBuilder, new(10, 10), Color.White);
        spriteBatch.End();
    }

    protected override async Task InitializeCoreAsync(IProgressHandler progress, CancellationToken cancellationToken = default)
    {
        font = context.Content.Load<SpriteFont>(fontAssetName);
    }
}
