using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkReport
    {
        public List<List<string>> ConfirmMatches { get; set; }

        public List<List<string>> DuplicatesAct1 { get; set; }

        public List<List<string>> DuplicatesAct2 { get; set; }

        public List<ChunkIgnoreMatch> IgnoreMatches { get; set; }

        public ChunkReport(List<ChunkData> chunksAct1, List<ChunkData> chunksAct2, Dictionary<int, List<int>?> chunkIgnore)
        {
            ConfirmMatches = chunksAct1
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Pending)
                .Select(match => new List<string>
                {
                    match.index.ToString("X2"),
                    match.chunk.Match.ToString("X2")
                })
                .ToList();
            DuplicatesAct1 = CollectDuplicates(chunksAct1);
            DuplicatesAct2 = CollectDuplicates(chunksAct2);
            IgnoreMatches = [];

            if (chunkIgnore.Count == 0)
                IgnoreMatches.Add(new ChunkIgnoreMatch(0, [0]));

            else foreach (int index1 in chunkIgnore.Keys)
            {
                List<int>? ignore = chunkIgnore[index1];
                IgnoreMatches.Add(new ChunkIgnoreMatch(index1, ignore));
            }
        }

#pragma warning disable CS8618
        public ChunkReport()
        {
        }
#pragma warning restore CS8618

        private static List<List<string>> CollectDuplicates(List<ChunkData> chunks)
        {
            return chunks
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Duplicate)
                .GroupBy(match => match.chunk.Match)
                .OrderBy(group => group.Key)
                .Select(group => group
                    .Select(chunk => chunk.index)
                    .Prepend(group.Key)
                    .Select(index => index.ToString("X2"))
                    .ToList())
                .ToList();
        }
    }

}
