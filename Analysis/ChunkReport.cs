using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkReport
    {
        public List<List<string>>? ConfirmMatches { get; set; }

        public List<List<string>>? DuplicatesAct1 { get; set; }

        public List<List<string>>? DuplicatesAct2 { get; set; }

        public List<ChunkIgnoreMatch>? IgnoreMatches { get; set; }

        public ChunkReport(List<ChunkInfo> chunksAct1, List<ChunkInfo> chunksAct2, Dictionary<int, List<int>?> chunkIgnore)
        {
            ConfirmMatches = [.. chunksAct1
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Pending)
                .Select(match => new List<string>
                {
                    match.index.ToString("X"),
                    match.chunk.Match.ToString("X")
                })];
            DuplicatesAct1 = CollectDuplicates(chunksAct1);
            DuplicatesAct2 = CollectDuplicates(chunksAct2);
            IgnoreMatches = [];

            if (chunkIgnore.Count == 0)
                IgnoreMatches.Add(new ChunkIgnoreMatch(0, [0]));

            else foreach (var index1 in chunkIgnore.Keys)
                {
                    var ignore = chunkIgnore[index1];
                    IgnoreMatches.Add(new ChunkIgnoreMatch(index1, ignore));
                }
        }

        public ChunkReport()
        {
        }

        private static List<List<string>> CollectDuplicates(List<ChunkInfo> chunks)
        {
            return [.. chunks
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Duplicate)
                .GroupBy(match => match.chunk.Match)
                .OrderBy(group => group.Key)
                .Select(group => group
                    .Select(chunk => chunk.index)
                    .Prepend(group.Key)
                    .Select(index => index.ToString("X"))
                    .ToList())];
        }
    }

}