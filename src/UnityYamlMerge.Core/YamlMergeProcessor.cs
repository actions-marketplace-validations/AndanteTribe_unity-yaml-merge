namespace UnityYamlMerge.Core;

public static class YamlMergeProcessor
{
    public static async ValueTask StartAsync(IReadOnlyList<MergeRequest> requests, HttpClient? httpClient = null, CancellationToken cancellationToken = default)
    {
        ThrowHelper.ThrowIfInvalidArguments(requests);
        var (versionSource, unityVersion, projectPath) = EnvironmentVariables.Get();
        httpClient ??= new HttpClient { Timeout = Timeout.InfiniteTimeSpan };

        try
        {
            unityVersion = versionSource switch
            {
                VersionSource.Project => GetLocalUnityVersion(projectPath),
                VersionSource.LatestLts => await httpClient.GetLatestLtsVersionAsync(cancellationToken),
                _ => unityVersion,
            };
        }
        catch (Exception e)
        {
            ThrowHelper.ThrowException(e);
        }
        await ValidateUnityVersionAsync(versionSource, unityVersion, httpClient, cancellationToken);

        var dockerImage = "unityci/editor:" + unityVersion + "-base-3";

        try
        {
            if (!await DockerHelper.PullImageAsync(dockerImage, cancellationToken))
            {
                ThrowHelper.ThrowFailPullDockerImage(dockerImage);
            }

            await Parallel.ForEachAsync(requests, cancellationToken, async (request, ct) =>
            {
                var success = await DockerHelper.RunMergeAsync(dockerImage, projectPath, request, ct);
                if (!success)
                {
                    throw new InvalidOperationException($"Merge failed for request: {request}");
                }
            });
        }
        catch (Exception e)
        {
            ThrowHelper.ThrowException(e);
        }
    }

    private static string GetLocalUnityVersion(string projectPath)
    {
        const string prefix = "m_EditorVersion:";

        var versionFilePath = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
        var version = File.ReadLines(versionFilePath)
            .FirstOrDefault(static line => line.StartsWith(prefix, StringComparison.Ordinal))?.AsSpan(prefix.Length).Trim().ToString();
        if (string.IsNullOrEmpty(version))
        {
            ThrowHelper.ThrowFailGetVersion();
            return "";
        }
        return version;
    }

    private static async ValueTask ValidateUnityVersionAsync(VersionSource source, string unityVersion, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        if (source == VersionSource.LatestLts)
        {
            // If the version source is latest LTS, we assume the version is valid since it was retrieved from Unity's official source.
            return;
        }

        try
        {
            if (!await httpClient.CheckValidVersionAsync(unityVersion, cancellationToken: cancellationToken))
            {
                ThrowHelper.ThrowInvalidUnityVersion(unityVersion);
            }
        }
        catch (Exception e)
        {
            ThrowHelper.ThrowException(e);
        }
    }
}