using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class TileMatch(TileData tile, bool xFlip, bool yFlip)
    {
        public TileData Block { get; set; } = tile;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public static Dictionary<int, TileMatch> FindMatches(List<TileData> tiles)
        {
            Dictionary<int, TileMatch> matches = [];

            return matches;
        }

        public static void Merge(Dictionary<int, TileMatch> tiles1, Dictionary<int, TileMatch> tiles2)
        {
        }
    }

}
