using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkMatch(ChunkData chunk)
    {
        public ChunkData Chunk { get; set; } = chunk;

        public static Dictionary<int, ChunkMatch> FindMatches(List<ChunkData> chunks, Dictionary<int, BlockMatch> blocks)
        {
            Dictionary<int, ChunkMatch> matches = [];

            return matches;
        }

        public static void Merge(Dictionary<int, ChunkMatch> chunks1, Dictionary<int, ChunkMatch> chunks2)
        {
        }
    }
}
