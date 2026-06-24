namespace UnityYamlMerge.Core;

public readonly record struct MergeRequest
{
    public readonly string Base;
    public readonly string Ours;
    public readonly string Theirs;
    public readonly string Output;

    public MergeRequest(string @base, string ours, string theirs, string output)
    {
        Base = @base;
        Ours = ours;
        Theirs = theirs;
        Output = output;

        if (string.IsNullOrEmpty(@base))
        {
            throw new ArgumentException("Base file path cannot be null or empty.", nameof(@base));
        }
        if (string.IsNullOrEmpty(ours))
        {
            throw new ArgumentException("Ours file path cannot be null or empty.", nameof(ours));
        }
        if (string.IsNullOrEmpty(theirs))
        {
            throw new ArgumentException("Theirs file path cannot be null or empty.", nameof(theirs));
        }
        if (string.IsNullOrEmpty(output))
        {
            throw new ArgumentException("Output file path cannot be null or empty.", nameof(output));
        }

        // ours == output is allowed, because we can overwrite our own file with the merged result.
        if (@base == ours || @base == theirs || @base == output)
        {
            throw new ArgumentException("Base file path cannot be the same as any other file path.", nameof(@base));
        }
        if (ours == theirs)
        {
            throw new ArgumentException("Ours file path cannot be the same as Theirs file path.", nameof(ours));
        }
        if (theirs == output)
        {
            throw new ArgumentException("Theirs file path cannot be the same as Output file path.", nameof(theirs));
        }
    }

    public static MergeRequest Parse(string csv)
    {
        var destination = (Span<Range>)stackalloc Range[4];
        var length = csv.AsSpan().Split(destination, ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var mergeRequest = length != 4
            ? default
            : new MergeRequest(
                @base: csv.AsSpan(destination[0]).ToString(),
                ours: csv.AsSpan(destination[1]).ToString(),
                theirs: csv.AsSpan(destination[2]).ToString(),
                output: csv.AsSpan(destination[3]).ToString());
        return mergeRequest;
    }
}