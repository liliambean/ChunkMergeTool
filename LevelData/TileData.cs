namespace ChunkMergeTool.LevelData
{
    internal class TileData(IList<byte> bytes)
    {
        public IList<byte> Bytes { get; set; } = bytes;

        public bool Pinned { get; set; }

        public bool Used { get; set; }

        public static List<TileData> Load(string filename)
        {
            string compressed = $"{filename}.bin";
            string uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: true, extract: true);

            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            List<TileData> list = [];

            while (file.Position != file.Length)
            {
                byte[] bytes = new byte[0x20];
                file.ReadExactly(bytes);
                list.Add(new TileData(bytes));
            }

            return list;
        }

        public static void MarkUsedAndPinned(List<BlockData> blocks, List<TileData> tiles, List<(int, int)> pinnedIds)
        {
            foreach (BlockData block in blocks)
                foreach (TileRef tile in block.Definition)
                    tiles[tile.Id].Used = true;

            foreach ((int start, int end) in pinnedIds)
                for (int id = start; id <= end; id++)
                    tiles[id].Pinned = true;
        }
    }

}
