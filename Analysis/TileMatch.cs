using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class TileMatch(int id, TileData tile, bool xFlip, bool yFlip) : IMatch<TileData>
    {
        public int Id { get; set; } = id;

        public TileData Data { get; set; } = tile;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public static Dictionary<int, List<TileMatch>> FindDuplicatesInAct(List<TileData> tiles)
        {
            Dictionary<int, List<TileMatch>> matches = [];

            for (int index = 0; index < tiles.Count; index++)
            {
                TileData tile = tiles[index];
                if (!tile.Used) continue;

                List<TileMatch> tileMatches = [];

                Utils.ForEachFlipWhere(
                    (xFlip, yFlip) => tile.Equals(tile, xFlip, yFlip),
                    (xFlip, yFlip) => tileMatches.Add(new TileMatch(index, tile, xFlip, yFlip))
                );

                matches[index] = tileMatches;
            }

            for (int index1 = 0; index1 < tiles.Count - 1; index1++)
            {
                TileData tile1 = tiles[index1];
                if (!tile1.Used) continue;

                for (int index2 = index1 + 1; index2 < tiles.Count; index2++)
                {
                    TileData tile2 = tiles[index2];
                    if (!tile2.Used) continue;

                    List<TileMatch> tile1matches = matches[index1];
                    List<TileMatch> tile2matches = matches[index2];

                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => tile1.Equals(tile2, xFlip, yFlip),
                        (xFlip, yFlip) =>
                        {
                            tile1matches.Add(new TileMatch(index2, tile2, xFlip, yFlip));
                            if (tile2.Pinned != PinnedKind.None) return;
                            tile2matches.Add(new TileMatch(index1, tile1, xFlip, yFlip));
                        });
                }
            }

            return matches;
        }

        public static (List<TileData>, List<TileData>, List<TileData>) FindDuplicatesAcrossActs(
            Dictionary<int, List<TileMatch>> matches1, Dictionary<int, List<TileMatch>> matches2)
        {
            List<TileData> act1 = Utils.CreateShortlist<TileMatch, TileData>(matches1);
            List<TileData> act2 = Utils.CreateShortlist<TileMatch, TileData>(matches2);
            List<TileData> primary = act1.Where(tile => tile.Pinned == PinnedKind.Primary).ToList();

            foreach (TileData tile1 in act1.Where(tile => tile.Pinned == PinnedKind.None))
            {
                bool isMatch = false;

                foreach (TileData tile2 in act2.Where(tile => tile.Pinned == PinnedKind.None))
                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => tile1.Equals(tile2, xFlip, yFlip),
                        (xFlip, yFlip) =>
                        {
                            foreach (TileMatch match in matches2.SelectMany(entry => entry.Value).Where(match => match.Data == tile2))
                            {
                                match.Data = tile1;
                                match.XFlip ^= xFlip;
                                match.YFlip ^= yFlip;
                            }

                            isMatch = true;
                            tile2.Used = false;
                        });

                act2.RemoveAll(tile => !tile.Used);
                if (isMatch) primary.Add(tile1);
            }

            act1.RemoveAll(primary.Contains);
            act2.RemoveAll(tile => tile.Pinned == PinnedKind.Primary);
            return (primary, act1, act2);
        }
    }

}
