namespace ChunkMergeTool.LevelData
{
    internal class TileData(IList<byte> bytes) : IData
    {
        public IList<byte> Bytes { get; set; } = bytes;

        public bool Primary { get; set; }

        public bool Used { get; set; }

        public int PinnedId { get; set; }

        public PinnedKind Pinned { get; set; }

        public static List<TileData> Load(string filename)
        {
            string compressed = $"{filename}.bin";
            string uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: true, extract: true);

            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            List<TileData> list = [];

            while (file.Position != file.Length)
            {
                byte[] bytes = new byte[Utils.TileSize];
                file.ReadExactly(bytes);
                list.Add(new TileData(bytes));
            }

            return list;
        }

        public static void MarkUsedAndPinned(List<BlockData> blocks, List<TileData> tiles, List<Range> rangePrimary, Range rangeAct)
        {
            foreach (BlockData block in blocks)
                foreach (TileRef tile in block.Definition)
                    tiles[tile.Id].Used = true;

            foreach (Range range in rangePrimary)
                for (int id = range.Start.Value; id <= range.End.Value; id++)
                {
                    TileData tile = tiles[id];
                    tile.Pinned = PinnedKind.Primary;
                    tile.PinnedId = id;
                    tile.Used = true;
                }

            for (int id = rangeAct.Start.Value; id <= rangeAct.End.Value; id++)
            {
                TileData tile = tiles[id];
                tile.Pinned = PinnedKind.Act;
                tile.PinnedId = id;
            }
        }

        public static void EnsurePinned(List<TileData> tiles, int firstId)
        {
            List<TileData> pinned = tiles.Where(tile => tile.Pinned != PinnedKind.None).ToList();

            foreach (TileData tile in pinned)
                tiles.Remove(tile);

            foreach (TileData tile in pinned)
            {
                int index = tile.PinnedId - firstId;
                tiles.Insert(index, tile);
            }
        }
    }

    internal enum PinnedKind
    {
        None = 0,
        Primary = 1,
        Act = 2,
    }

}
