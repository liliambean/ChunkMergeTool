using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class BlockMatch(BlockData block, bool xFlip, bool yFlip) : IMatch<BlockData>
    {
        public BlockData Data { get; set; } = block;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public bool Primary { get; set; }

        public static Dictionary<int, BlockMatch> FindMatches(List<BlockData> blocks, Dictionary<int, TileMatch> tiles)
        {
            Dictionary<int, BlockMatch> matches = [];

            return matches;
        }

        public static void MarkPrimary(Dictionary<int, BlockMatch> matches1, Dictionary<int, BlockMatch> matches2)
        {
        }

        public static void UpdateTileRefs(List<BlockData> blocks, Dictionary<int, BlockMatch> matches)
        {
        }
    }

}
