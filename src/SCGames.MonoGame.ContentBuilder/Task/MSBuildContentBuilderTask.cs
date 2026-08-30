using Microsoft.Build.Framework;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace SCGames.MonoGame.MSBuildContentBuilder.Task;

/// <summary>
/// <para>
/// MSBuild task that invokes a <see cref="CsxContentBuilder"/>.
/// </para>
/// <para>
/// <strong>NB: At present, doesn't really work.</strong> Content builder fails to find the various runtime deps,
/// ultimately because the app base path is the dotnet SDK (where MSBuild lives), and I haven't yet been able to 
/// figure out how to apply configuration in such a way that they can all be found robustly.
/// </para>
/// </summary>
public class MSBuildContentBuilderTask : Microsoft.Build.Utilities.Task
{
    /// <summary>
    /// Gets or sets the MonoGame content builder script files to be processed.
    /// </summary>
    [Required]
    public required ITaskItem[] ScriptFiles { get; set; }

    /// <summary>
    /// Gets or sets the working directory that the content builder should use.
    /// </summary>
    [Required]
    public required string WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets the source directory that the content builder should use.
    /// </summary>
    [Required]
    public required string SourceDirectory { get; set; }

    /// <summary>
    /// Gets or sets the intermediate directory that the content builder should use.
    /// </summary>
    [Required]
    public required string IntermediateDirectory { get; set; }

    /// <summary>
    /// Gets or sets the output directory that the content builder should use.
    /// </summary>
    [Required]
    public required string OutputDirectory { get; set; }

    /// <summary>
    /// Gets or sets the graphics profile to build content for.
    /// </summary>
    [Required]
    public required string GraphicsProfile { get; set; }

    /// <summary>
    /// Gets or sets the target platform to build content for.
    /// </summary>
    [Required]
    public required string TargetPlatform { get; set; }

    //// TODO: public string ScriptReferences { get; set; } = string.Empty;

    //// TODO: public string ScriptImports { get; set; } = string.Empty;

    /// <summary>
    /// Executes the content builder task, processing all scripts.
    /// </summary>
    /// <returns>A value indicating whether processing succeeded.</returns>
    public override bool Execute()
    {
        ////System.Diagnostics.Debugger.Launch();

        var scriptFilePaths = ScriptFiles.Select(f => f.GetMetadata("FullPath")).ToArray();

        CsxContentBuilder builder = new(scriptFilePaths)
        {
            Logger = new MSBuildContentBuildLogger(Log),
        };

        return builder.Run(new ContentBuilderParams()
        {
            Mode = ContentBuilderMode.Builder,
            WorkingDirectory = WorkingDirectory,
            SourceDirectory = SourceDirectory,
            IntermediateDirectory = IntermediateDirectory,
            OutputDirectory = OutputDirectory,
            GraphicsProfile = Enum.Parse<GraphicsProfile>(GraphicsProfile),
            Platform = Enum.Parse<TargetPlatform>(TargetPlatform),
        });
    }
}
