namespace ChunkMergeTool.LevelData
{
    internal class TileData(IList<byte> bytes) : IData
    {
        public IList<byte> Bytes { get; set; } = bytes;

        public bool Primary { get; set; }

        public bool Pinned { get; set; }

        public bool Used { get; set; }

        public bool Equals(TileData tile, bool xFlip, bool yFlip)
        {
            IList<int> lookup;

            if (!xFlip && !yFlip) lookup =
            [
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B,
                0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1A, 0x1B,
                0x1C, 0x1D, 0x1E, 0x1F,
            ];
            else if (xFlip && !yFlip) lookup =
            [
                0x03, 0x02, 0x01, 0x00,
                0x07, 0x06, 0x05, 0x04,
                0x0B, 0x0A, 0x09, 0x08,
                0x0F, 0x0E, 0x0D, 0x0C,
                0x13, 0x12, 0x11, 0x10,
                0x17, 0x16, 0x15, 0x14,
                0x1B, 0x1A, 0x19, 0x18,
                0x1F, 0x1E, 0x1D, 0x1C,
            ];
            else if (!xFlip && yFlip) lookup =
            [
                0x1C, 0x1D, 0x1E, 0x1F,
                0x18, 0x19, 0x1A, 0x1B,
                0x14, 0x15, 0x16, 0x17,
                0x10, 0x11, 0x12, 0x13,
                0x0C, 0x0D, 0x0E, 0x0F,
                0x08, 0x09, 0x0A, 0x0B,
                0x04, 0x05, 0x06, 0x07,
                0x00, 0x01, 0x02, 0x03,
            ];
            else lookup =
            [
                0x1F, 0x1E, 0x1D, 0x1C,
                0x1B, 0x1A, 0x19, 0x18,
                0x17, 0x16, 0x15, 0x14,
                0x13, 0x12, 0x11, 0x10,
                0x0F, 0x0E, 0x0D, 0x0C,
                0x0B, 0x0A, 0x09, 0x08,
                0x07, 0x06, 0x05, 0x04,
                0x03, 0x02, 0x01, 0x00,
            ];

            for (int index = 0; index < 0x20; index++)
            {
                int byte1 = this.Bytes[index];
                int byte2 = tile.Bytes[lookup[index]];

                if (xFlip)
                    byte2 = ((byte2 & 0x0F) << 4) | ((byte2 & 0xF0) >> 4);

                if (byte1 != byte2)
                    return false;
            }

            return true;
        }

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
