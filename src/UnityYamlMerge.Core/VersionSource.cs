namespace UnityYamlMerge.Core;

public enum VersionSource : sbyte
{
    Invalid = -1,
    Project = 0,
    LatestLts = 1,
    Manual = 2,
}