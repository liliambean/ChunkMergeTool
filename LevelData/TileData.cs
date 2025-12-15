namespace ChunkMergeTool.LevelData
{
    internal class TileData(byte[] bytes) : IData
    {
        public byte[] Bytes { get; set; } = bytes;

        public bool Used { get; set; }

        public int PinnedId { get; set; }

        public PinnedKind Pinned { get; set; }

        public static List<TileData> Load(string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);
            Utils.ProcessKosFile(compressed, uncompressed, moduled: true, extract: true);

            List<TileData> list = [];
            using (FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed)))
            {
                while (file.Position != file.Length)
                {
                    byte[] bytes = new byte[Utils.TileSize];
                    file.ReadExactly(bytes);
                    list.Add(new TileData(bytes));
                }
            }

            return list;
        }

        public static void Save(List<TileData> tiles, string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);

            using (FileStream file = File.Open(Path.Combine(Utils.WorkingDir, uncompressed), FileMode.Create))
            {
                foreach (TileData tile in tiles)
                    file.Write(tile.Bytes);
            }

            Utils.ProcessKosFile(uncompressed, compressed, moduled: true, extract: false);
        }

        public static void MarkUsedAndPinned(
            List<BlockData> blocks, List<TileData> tiles, Range rangeObject, Range rangePrimary, Range rangeAct)
        {
            foreach (BlockData block in blocks.Where(block => block.Used))
                foreach (TileRef tile in block.Definition)
                    tiles[tile.Id].Used = true;

            foreach (Range range in new[] { rangeObject, rangePrimary })
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
    }

    internal enum PinnedKind
    {
        None = 0,
        Primary = 1,
        Act = 2,
    }

}
