using UnityYamlMerge.Core;

// arguments: <base1> <ours1> <theirs1> <output1> [<base2> <ours2> <theirs2> <output2> ...]
if (args.Length == 0 || args.Length % 4 != 0)
{
    Console.Error.WriteLine("Usage: UnityYamlMerge <base> <ours> <theirs> <output> [...]");
    Console.Error.WriteLine("Arguments must be provided in sets of 4.");
    Environment.Exit(1);
}

var requests = new List<MergeRequest>(args.Length / 4);
for (var i = 0; i < args.Length; i += 4)
{
    requests.Add(new MergeRequest(args[i], args[i + 1], args[i + 2], args[i + 3]));
}

await YamlMergeProcessor.StartAsync(requests);
