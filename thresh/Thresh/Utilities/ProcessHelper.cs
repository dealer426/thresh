using System.Diagnostics;
using System.Text;

namespace Thresh.Utilities;

/// <summary>
/// Utility class for executing external processes, particularly WSL commands
/// </summary>
public static class ProcessHelper
{
    private const int DefaultTimeoutSeconds = 30;

    /// <summary>
    /// Execute a command and return the result
    /// </summary>
    public static async Task<ProcessResult> ExecuteAsync(params string[] command)
    {
        return await ExecuteAsync(DefaultTimeoutSeconds, command);
    }

    /// <summary>
    /// Execute a command with a timeout and return the result
    /// </summary>
    public static async Task<ProcessResult> ExecuteAsync(int timeoutSeconds, params string[] command)
    {
        return await ExecuteAsync(null, timeoutSeconds, command);
    }

    /// <summary>
    /// Execute a command with a timeout, custom environment variables, and return the result
    /// </summary>
    public static async Task<ProcessResult> ExecuteAsync(Dictionary<string, string>? environmentVariables, int timeoutSeconds, params string[] command)
    {
        if (command.Length == 0)
            return new ProcessResult(false, -1, [], "No command specified");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command[0],
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Add arguments
            for (int i = 1; i < command.Length; i++)
            {
                startInfo.ArgumentList.Add(command[i]);
            }

            // Add custom environment variables (inherits parent environment by default)
            if (environmentVariables != null)
            {
                foreach (var (key, value) in environmentVariables)
                {
                    startInfo.Environment[key] = value;
                }
            }

            using var process = new Process { StartInfo = startInfo };
            
            var outputLines = new List<string>();
            var errorLines = new List<string>();

            // Capture output asynchronously
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    lock (outputLines) { outputLines.Add(e.Data); }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    lock (errorLines) { errorLines.Add(e.Data); }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for completion with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                return new ProcessResult(false, -1, outputLines, $"Process timed out after {timeoutSeconds} seconds");
            }

            var exitCode = process.ExitCode;
            var allOutput = outputLines.Concat(errorLines).ToList();
            
            return new ProcessResult(exitCode == 0, exitCode, allOutput, 
                errorLines.Count > 0 ? string.Join("\n", errorLines) : null);
        }
        catch (Exception ex)
        {
            return new ProcessResult(false, -1, [], $"Process execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Execute a command with real-time output streaming
    /// </summary>
    public static async Task<ProcessResult> ExecuteWithStreamingAsync(
        Action<string>? onOutputLine, 
        int timeoutSeconds, 
        params string[] command)
    {
        return await ExecuteWithStreamingAsync(onOutputLine, null, timeoutSeconds, command);
    }

    /// <summary>
    /// Execute a command with real-time output streaming and custom environment variables
    /// </summary>
    public static async Task<ProcessResult> ExecuteWithStreamingAsync(
        Action<string>? onOutputLine,
        Dictionary<string, string>? environmentVariables,
        int timeoutSeconds, 
        params string[] command)
    {
        if (command.Length == 0)
            return new ProcessResult(false, -1, [], "No command specified");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command[0],
                RedirectStandardOutput = true,
               RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Add arguments
            for (int i = 1; i < command.Length; i++)
            {
                startInfo.ArgumentList.Add(command[i]);
            }

            // Add custom environment variables (inherits parent environment by default)
            if (environmentVariables != null)
            {
                foreach (var (key, value) in environmentVariables)
                {
                    startInfo.Environment[key] = value;
                }
            }

            using var process = new Process { StartInfo = startInfo };
            
            var outputLines = new List<string>();
            var errorLines = new List<string>();

            // Capture output asynchronously with callback
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    lock (outputLines) { outputLines.Add(e.Data); }
                    onOutputLine?.Invoke(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    lock (errorLines) { errorLines.Add(e.Data); }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait for completion with timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(entireProcessTree: true);
                return new ProcessResult(false, -1, outputLines, $"Process timed out after {timeoutSeconds} seconds");
            }

            var exitCode = process.ExitCode;
            
            return new ProcessResult(exitCode == 0, exitCode, outputLines, 
                errorLines.Count > 0 ? string.Join("\n", errorLines) : null);
        }
        catch (Exception ex)
        {
            return new ProcessResult(false, -1, [], $"Process execution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a command is available on the system
    /// </summary>
    public static async Task<bool> IsCommandAvailableAsync(string command)
    {
        try
        {
            var isWindows = OperatingSystem.IsWindows();
            var checkCommand = isWindows 
                ? new[] { "where", command }
                : new[] { "which", command };

            var result = await ExecuteAsync(5, checkCommand);
            return result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Result of a process execution
    /// </summary>
    public class ProcessResult
    {
        public bool IsSuccess { get; }
        public int ExitCode { get; }
        public List<string> Output { get; }
        public string? Error { get; }

        public ProcessResult(bool isSuccess, int exitCode, List<string> output, string? error)
        {
            IsSuccess = isSuccess;
            ExitCode = exitCode;
            Output = output;
            Error = error;
        }

        public string GetOutputAsString()
        {
            return string.Join("\n", Output);
        }

        public bool HasOutput()
        {
            return Output.Count > 0 && Output.Any(line => !string.IsNullOrWhiteSpace(line));
        }

        public override string ToString()
        {
            return $"ProcessResult{{Success={IsSuccess}, ExitCode={ExitCode}, OutputLines={Output.Count}, Error='{Error}'}}";
        }
    }
}
