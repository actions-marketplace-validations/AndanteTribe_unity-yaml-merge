namespace UnityYamlMerge.Core;

public static class ThrowHelper
{
    public static void ThrowIfMissingEnvironmentVariables(VersionSource versionSource, string unityVersion, string projectPath)
    {
        var hasError = false;
        if (versionSource == VersionSource.Invalid)
        {
            Console.Error.WriteLine($"{EnvironmentVariables.UnityVersionSource} is not set or invalid. Set `{EnvironmentVariables.UnityVersionSource}` to one of the following values: `project`, `latest-lts`, or `manual`.");
            hasError = true;
        }
        if (versionSource == VersionSource.Manual && string.IsNullOrEmpty(unityVersion))
        {
            Console.Error.WriteLine($"{EnvironmentVariables.UnityVersion} is not set. Set `{EnvironmentVariables.UnityVersion}` to the desired Unity version when `{EnvironmentVariables.UnityVersionSource}` is set to `manual`.");
            hasError = true;
        }
        if (string.IsNullOrEmpty(projectPath))
        {
            Console.Error.WriteLine($"{EnvironmentVariables.ProjectPath} is not set.");
            hasError = true;
        }
        else
        {
            if (!Directory.Exists(projectPath))
            {
                Console.Error.WriteLine($"{EnvironmentVariables.ProjectPath} '{projectPath}' does not exist.");
                hasError = true;
            }
            if (versionSource == VersionSource.Project && !File.Exists(Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt")))
            {
                Console.Error.WriteLine($"{EnvironmentVariables.ProjectPath} '{projectPath}' is missing ProjectVersion.txt.");
                hasError = true;
            }
        }

        if (hasError)
        {
            Environment.Exit(1);
        }
    }

    public static void ThrowIfInvalidArguments(IReadOnlyList<MergeRequest> requests)
    {
        var hasError = 0;
        Parallel.ForEach(requests, request =>
        {
            if (!File.Exists(request.Base))
            {
                Console.Error.WriteLine($"Base file '{request.Base}' does not exist.");
                Interlocked.Exchange(ref hasError, 1);
            }
            if (!File.Exists(request.Ours))
            {
                Console.Error.WriteLine($"Ours file '{request.Ours}' does not exist.");
                Interlocked.Exchange(ref hasError, 1);
            }
            if (!File.Exists(request.Theirs))
            {
                Console.Error.WriteLine($"Theirs file '{request.Theirs}' does not exist.");
                Interlocked.Exchange(ref hasError, 1);
            }
        });

        if (hasError != 0)
        {
            Environment.Exit(1);
        }
    }

    public static void ThrowFailGetVersion()
    {
        Console.Error.WriteLine("Failed to get Unity version from project.");
        Environment.Exit(1);
    }

    public static void ThrowInvalidUnityVersion(string unityVersion)
    {
        Console.Error.WriteLine("Invalid Unity version: " + unityVersion);
        Environment.Exit(1);
    }

    public static void ThrowException(Exception e)
    {
        if (e is OperationCanceledException)
        {
            throw e;
        }
        Console.Error.WriteLine(e.Message);
        Environment.Exit(1);
    }

    public static void ThrowFailPullDockerImage(string dockerImage)
    {
        Console.Error.WriteLine("Failed to pull Docker image: " + dockerImage);
        Environment.Exit(1);
    }
}