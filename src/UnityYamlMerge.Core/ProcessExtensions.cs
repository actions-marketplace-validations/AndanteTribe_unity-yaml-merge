using System.Collections.Concurrent;
using System.Diagnostics;

namespace UnityYamlMerge.Core;

public static class ProcessExtensions
{
    extension(ProcessStartInfo)
    {
        public static ProcessStartInfo Create(string fileName)
        {
            return new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
        }
    }

    extension(Process)
    {
        public static async ValueTask<int> StartAsync(ProcessStartInfo startInfo, ConcurrentQueue<string>? output = null, CancellationToken cancellationToken = default)
        {
            using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start " + startInfo.FileName + " process.");
            var stdoutCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var stderrCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var _ = cancellationToken.UnsafeRegister(static args =>
            {
                var (process, stdoutCompletionSource, stderrCompletionSource) = ((Process, TaskCompletionSource, TaskCompletionSource))args!;
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore exceptions when killing the process
                }
                stdoutCompletionSource.TrySetCanceled();
                stderrCompletionSource.TrySetCanceled();
            }, (process, stdoutCompletionSource, stderrCompletionSource));

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    stdoutCompletionSource.TrySetResult();
                    return;
                }

                if (output == null)
                {
                    Console.WriteLine(e.Data);
                }
                else
                {
                    output.Enqueue(e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    stderrCompletionSource.TrySetResult();
                    return;
                }
                Console.Error.WriteLine(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await Task.WhenAll(stdoutCompletionSource.Task, stderrCompletionSource.Task, process.WaitForExitAsync(cancellationToken));
            return process.ExitCode;
        }
    }
}