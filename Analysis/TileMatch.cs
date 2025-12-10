using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class TileMatch(TileData tile, bool xFlip, bool yFlip) : IMatch<TileData>
    {
        public TileData Data { get; set; } = tile;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public static Dictionary<int, TileMatch> FindMatches(List<TileData> tiles)
        {
            Dictionary<int, List<TileMatch>> matches = [];

            for (int index = 0; index < tiles.Count; index++)
            {
                TileData tile = tiles[index];
                if (!tile.Used) continue;
                List<TileMatch> matchesForTile = [];

                Utils.ForEachFlipWhere(
                    (xFlip, yFlip) => tile.Equals(tile, xFlip, yFlip),
                    (xFlip, yFlip) => matchesForTile.Add(new TileMatch(tile, xFlip, yFlip))
                );

                matches[index] = matchesForTile;
            }

            for (int index1 = 0; index1 < tiles.Count - 1; index1++)
            {
                TileData tile1 = tiles[index1];
                if (!tile1.Used) continue;

                for (int index2 = index1 + 1; index2 < tiles.Count; index2++)
                {
                    TileData tile2 = tiles[index2];
                    if (!tile2.Used) continue;
                    List<TileMatch> matchesForTile1 = matches[index1];

                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => tile1.Equals(tile2, xFlip, yFlip),
                        (xFlip, yFlip) =>
                        {
                            matchesForTile1.Add(new TileMatch(tile2, xFlip, yFlip));
                            if (tile2.Pinned != PinnedKind.None) return;
                            matches[index2].Add(new TileMatch(tile1, xFlip, yFlip));
                        });
                }
            }

            return matches.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.OrderBy(tile => tiles.IndexOf(tile.Data)).First());
        }

        public static void MarkPrimary(Dictionary<int, TileMatch> matches1, Dictionary<int, TileMatch> matches2)
        {
            List<TileData> act1 = Utils.CreateShortlist<TileMatch, TileData>(matches1);
            List<TileData> act2 = Utils.CreateShortlist<TileMatch, TileData>(matches2);

            foreach (TileData tile1 in act1.Where(tile => tile.Pinned != PinnedKind.Act))
            {
                foreach (TileData tile2 in act2.Where(tile => tile.Pinned != PinnedKind.Act))
                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => tile1.Equals(tile2, xFlip, yFlip),
                        (xFlip, yFlip) =>
                        {
                            foreach (TileMatch tile in matches2.Values.Where(tile => tile.Data == tile2))
                            {
                                tile.XFlip ^= xFlip;
                                tile.YFlip ^= yFlip;
                                tile.Data = tile1;
                                tile.Data.Used = false;
                            }

                            tile1.Primary = true;
                        });

                act2.RemoveAll(tile => !tile.Used);
            }
        }
    }

}
