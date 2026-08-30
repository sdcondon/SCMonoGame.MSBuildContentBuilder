using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace SCGames.MonoGame.MSBuildContentBuilder;

/// <summary>
/// <para>
/// An implementation of <see cref="ContentBuilder"/> that invokes a given set of C# scripts
/// to build content.
/// </para>
/// <para>
/// Scripts are provided a global that is the <see cref="ContentCollection"/>
/// instance - and can thus include lines such as "Include&lt;WildcardRule&gt;(..);".
/// </para>
/// </summary>
public class CsxContentBuilder(string[] scriptFilePaths) : ContentBuilder
{
    /// <inheritdoc />
    public override IContentCollection GetContentCollection()
    {
        var scriptOptions = ScriptOptions
            .Default
            .AddReferences(
            [
                typeof(object).Assembly,
                typeof(ContentCollection).Assembly,
                // TODO: ..and any others specified via a build prop.
            ])
            .AddImports(
            [
                "Microsoft.Xna.Framework.Content.Pipeline",
                "Microsoft.Xna.Framework.Content.Pipeline.Processors",
                "MonoGame.Framework.Content.Pipeline.Builder",
                // TODO: ..and any others specified via a build prop.
            ]);

        // NB: We need to register the assembly that contains the script global type (ContentCollection).
        // Else the script will load a separate copy of the assembly into its own load context, which will
        // mean that the script and this host will be using "different" types, and it'll fail.
        using InteractiveAssemblyLoader scriptAssemblyLoader = new();
        scriptAssemblyLoader.RegisterDependency(typeof(ContentCollection).Assembly);

        ContentCollection contentCollection = new();

        foreach (var scriptFilePath in scriptFilePaths)
        {
            Logger.Log(LogLevel.Info, $"Executing MonoGame Content Builder Script '{scriptFilePath}'");

            CSharpScript
                .Create(File.ReadAllText(scriptFilePath), scriptOptions, typeof(ContentCollection), scriptAssemblyLoader)
                .RunAsync(contentCollection, HandleScriptException)
                .GetAwaiter()
                .GetResult();
        }

        return contentCollection;
    }

    private bool HandleScriptException(Exception exception)
    {
        Logger.Log(LogLevel.Error, exception.ToString());
        return false;
    }
}
