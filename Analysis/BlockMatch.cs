using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class BlockMatch(BlockData block, bool xFlip, bool yFlip)
    {
        public BlockData Block { get; set; } = block;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public static Dictionary<int, BlockMatch> FindMatches(List<BlockData> blocks, Dictionary<int, TileMatch> tiles)
        {
            Dictionary<int, BlockMatch> matches = [];

            return matches;
        }

        public static void Merge(Dictionary<int, BlockMatch> blocks1, Dictionary<int, BlockMatch> blocks2)
        {
        }
    }

}
