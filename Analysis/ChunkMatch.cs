using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkMatch(ChunkData chunk) : IMatch<ChunkData>
    {
        public ChunkData Data { get; set; } = chunk;

        public bool Primary { get; set; }

        public static Dictionary<int, ChunkMatch> FindMatches(List<ChunkData> chunks, Dictionary<int, BlockMatch> blocks)
        {
            Dictionary<int, ChunkMatch> matches = [];

            return matches;
        }

        public static void MarkPrimary(Dictionary<int, ChunkMatch> matches1, Dictionary<int, ChunkMatch> matches2)
        {
        }
    }
}
