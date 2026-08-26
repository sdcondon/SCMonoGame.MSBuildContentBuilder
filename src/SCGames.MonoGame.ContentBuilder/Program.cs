using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace SCGames.MonoGame.MSBuildContentBuilder;

public static class Program
{
    private static ContentBuilderParams defaultBuilderParams = new();

    public static int Main(string[] args)
    {
        System.Diagnostics.Debugger.Launch();

        if (!TryProcessCommandLine(args, out var builder, out var builderParams))
        {
            return 1;
        }

        return builder.Run(builderParams) ? 0 : 1;
    }

    private static bool TryProcessCommandLine(string[] args, [MaybeNullWhen(false)] out ScriptedContentBuilder builder, [MaybeNullWhen(false)] out ContentBuilderParams builderParams)
    {
        // NB: No support for running as a server, since that makes no sense for a builder that runs in MSBuild

        ScriptedContentBuilder? maybeBuilder = null;
        ContentBuilderParams maybeParams = new();

        RootCommand command = new("Content builder for MonoGame that reads C# scripts to establish the content to be built.");

        Option<string> filesOption = new(["--files", "-f"], () => string.Empty)
        {
            Description = "A semicolon-delimited list of the script files to process.",
        };
        command.Add(filesOption);

        Option<string> workingDirectoryOption = new(["--workingDir", "-d"], () => defaultBuilderParams.WorkingDirectory)
        {
            Description = "The working directory of the content builder.",
        };
        command.Add(workingDirectoryOption);

        Option<string> srcDirectoryOptions = new(["--src", "-s"], () => defaultBuilderParams.SourceDirectory)
        {
            Description = "The source asset directory.",
        };
        command.Add(srcDirectoryOptions);

        Option<string> outputDirectoryOption = new(["--output", "-o"], () => defaultBuilderParams.OutputDirectory)
        {
            Description = "The output content directory.",
        };
        command.Add(outputDirectoryOption);

        Option<string> intermediateDirectoryOption = new(["--intermediate", "-i"], () => defaultBuilderParams.IntermediateDirectory)
        {
            Description = "The intermediate content directory.",
        };
        command.Add(intermediateDirectoryOption);

        Option<TargetPlatform> platformOption = new(["--platform", "-p"], () => defaultBuilderParams.Platform)
        {
            Description = "The content target platform.",
        };
        command.Add(platformOption);

        Option<GraphicsProfile> graphicsProfileOption = new(["--graphics-profile", "-g"], () => defaultBuilderParams.GraphicsProfile)
        {
            Description = "The content graphics profile.",
        };
        command.Add(graphicsProfileOption);

        Option<bool> compressContentOption = new(["--compress"], () => defaultBuilderParams.CompressContent)
        {
            Description = "Compress the build content files.",
        };
        command.Add(compressContentOption);

        Option<LogLevel> logLevelOption = new(["--loglevel", "-l"], () => defaultBuilderParams.LogLevel)
        {
            Description = "The log level of messages that get outputed to the console.",
        };
        command.Add(logLevelOption);

        Option<bool> rebuildOption = new(["--rebuild"], () => defaultBuilderParams.Rebuild)
        {
            Description = "Should the builder rebuild all the assets and ignore the content cache.",
        };
        command.Add(rebuildOption);

        Option<bool> skipCleanOption = new(["--skip-clean"], () => defaultBuilderParams.SkipClean)
        {
            Description = "Should the builder skip cleaning up old content cache data after the build is finished.",
        };
        command.Add(skipCleanOption);

        command.SetHandler(cxt =>
        {
            var files = cxt.ParseResult.GetValueForOption(filesOption).Split(';');
            maybeBuilder = new(files);

            maybeParams.Mode = ContentBuilderMode.Builder;
            string workingDir = cxt.ParseResult.GetValueForOption(workingDirectoryOption)!;
            maybeParams.WorkingDirectory = workingDir;
            maybeParams.SourceDirectory = MakeRelative(workingDir, cxt.ParseResult.GetValueForOption(srcDirectoryOptions));
            maybeParams.OutputDirectory = MakeRelative(workingDir, cxt.ParseResult.GetValueForOption(outputDirectoryOption));
            maybeParams.IntermediateDirectory = MakeRelative(workingDir, cxt.ParseResult.GetValueForOption(intermediateDirectoryOption));
            maybeParams.Platform = cxt.ParseResult.GetValueForOption(platformOption);
            maybeParams.GraphicsProfile = cxt.ParseResult.GetValueForOption(graphicsProfileOption);
            maybeParams.CompressContent = cxt.ParseResult.GetValueForOption(compressContentOption);
            maybeParams.LogLevel = cxt.ParseResult.GetValueForOption(logLevelOption);
            maybeParams.Rebuild = cxt.ParseResult.GetValueForOption(rebuildOption);
            maybeParams.SkipClean = cxt.ParseResult.GetValueForOption(skipCleanOption);
        });

        ParseResult parseResult = command.Parse(args);
        if (parseResult.Errors.Count == 0)
        {
            parseResult.Invoke();
            builder = maybeBuilder!;
            builderParams = maybeParams;
            return true;
        }
        else
        {
            foreach (var parseError in parseResult.Errors)
            {
                Console.Error.WriteLine(parseError.Message);
            }

            builder = null;
            builderParams = null;
            return false;
        }
    }

    private static string MakeRelative(string workingDir, string path)
    {
        if (!Path.IsPathRooted(path))
        {
            path = FileHelper.NormalizeDirectorySeparators(path);
        }

        return Path.GetRelativePath(workingDir, path);
    }
}
