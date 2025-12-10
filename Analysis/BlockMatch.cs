using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class BlockMatch(BlockData block, bool xFlip, bool yFlip) : IMatch<BlockData>
    {
        public BlockData Data { get; set; } = block;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;

        public bool Primary { get; set; }

        public int Id { get; set; }

        public static Dictionary<int, BlockMatch> FindMatches(List<BlockData> blocks, Dictionary<int, TileMatch> tiles)
        {
            Dictionary<int, List<BlockMatch>> matches = [];

            for (int index = 0; index < blocks.Count; index++)
            {
                BlockData block = blocks[index];
                if (!block.Used) continue;

                List<BlockMatch> blockMatches = [];

                Utils.ForEachFlipWhere(
                    (xFlip, yFlip) => block.Equals(block, xFlip, yFlip, tiles, tiles),
                    (xFlip, yFlip) => blockMatches.Add(new BlockMatch(block, xFlip, yFlip))
                );

                matches[index] = blockMatches;
            }

            for (int index1 = 0; index1 < blocks.Count - 1; index1++)
            {
                BlockData block1 = blocks[index1];
                if (!block1.Used) continue;

                for (int index2 = index1 + 1; index2 < blocks.Count; index2++)
                {
                    BlockData block2 = blocks[index2];
                    if (!block2.Used) continue;

                    List<BlockMatch> block1matches = matches[index1];
                    List<BlockMatch> block2matches = matches[index2];

                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => block1.Equals(block2, xFlip, yFlip, tiles, tiles),
                        (xFlip, yFlip) =>
                        {
                            block1matches.Add(new BlockMatch(block2, xFlip, yFlip));
                            block2matches.Add(new BlockMatch(block1, xFlip, yFlip));
                        });
                }
            }

            return matches.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.OrderBy(block => blocks.IndexOf(block.Data)).First());
        }

        public static void MarkPrimary(
            Dictionary<int, BlockMatch> matches1, Dictionary<int, BlockMatch> matches2,
            Dictionary<int, TileMatch> tiles1, Dictionary<int, TileMatch> tiles2)
        {
            List<BlockData> act1 = Utils.CreateShortlist<BlockMatch, BlockData>(matches1);
            List<BlockData> act2 = Utils.CreateShortlist<BlockMatch, BlockData>(matches2);

            foreach (BlockData block1 in act1)
            {
                foreach (BlockData block2 in act2)
                    Utils.ForEachFlipWhere(
                        (xFlip, yFlip) => block1.Equals(block2, xFlip, yFlip, tiles1, tiles2),
                        (xFlip, yFlip) =>
                        {
                            foreach (BlockMatch block in matches2.Values.Where(block => block.Data == block2))
                            {
                                block.XFlip ^= xFlip;
                                block.YFlip ^= yFlip;
                                block.Data = block1;
                                block.Data.Used = false;
                            }

                            block1.Primary = true;
                        });

                act2.RemoveAll(block => !block.Used);
            }
        }

        public static void UpdateTileRefs(List<BlockData> blocks, Dictionary<int, TileMatch> matches)
        {
            foreach (BlockData block in blocks)
                foreach (TileRef tileRef in block.Definition)
                {
                    TileMatch match = matches[tileRef.Id];
                    tileRef.Id = match.Id;
                    tileRef.XFlip ^= match.XFlip;
                    tileRef.YFlip ^= match.YFlip;
                }
        }
    }

}
