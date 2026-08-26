using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Diagnostics;

namespace SCGames.MonoGame.MSBuildContentBuilder.Task;

/// <summary>
/// Derivation of <see cref="ContentBuildLogger"/> that writes log messages out to an
/// MSBuild <see cref="TaskLoggingHelper"/>.
/// </summary>
/// <param name="log">The MSBuild logging helper to write content build log messages to.</param>
internal class MSBuildContentBuildLogger(TaskLoggingHelper log) : ContentBuildLogger
{
    private readonly TaskLoggingHelper _log = log;
    private readonly Stack<string> _relativePaths = new();
    private readonly Stopwatch _stopWatch = Stopwatch.StartNew();
    private int _indentCount;

    public override void Log(LogLevel level, string message)
    {
        if (level >= LoggerLogLevel)
        {
            string paths = _relativePaths.Count > 0 ? (string.Join(" > ", _relativePaths.Reverse()) + ": ") : "";
            string indent = new(' ', _indentCount * 2);
            string elapsed = LoggerLogLevel <= LogLevel.Debug ? $"{_stopWatch.Elapsed:hh\\:mm\\:ss\\.fff} " : "";

            string[] messageLines = message.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            message = $"{elapsed}{paths}{string.Join("\n", messageLines.Select(l => $"{indent}{l}"))}";

            switch (level)
            {
                case LogLevel.Error:
                    _log.LogError(message);
                    break;

                case LogLevel.Warning:
                    _log.LogWarning(message);
                    break;

                case LogLevel.Info:
                    _log.LogMessage(MessageImportance.High, message);
                    break;

                default:
                    _log.LogMessage(message);
                    break;
            }
        }
    }

    public override void PushFile(string filename)
    {
        string fullPath = Path.GetFullPath(filename);
        _relativePaths.Push(Path.GetRelativePath(base.LoggerRootDirectory, fullPath));
    }

    public override void PopFile()
    {
        _relativePaths.Pop();
    }

    public override void Indent()
    {
        _indentCount++;
    }

    public override void Unindent()
    {
        _indentCount = Math.Max(0, _indentCount - 1);
    }
}
