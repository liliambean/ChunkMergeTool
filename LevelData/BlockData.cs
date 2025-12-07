namespace ChunkMergeTool.LevelData
{
    internal class BlockData(List<TileRef> definition)
    {
        public List<TileRef> Definition { get; set; } = definition;

        public int Collision { get; set; }

        public IEnumerable<int> Words => Definition.Select(tileRef => tileRef.Word);

        public bool Used { get; set; }

        public static List<BlockData> Load(string filename)
        {
            string compressed = $"{filename}.bin";
            string uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            List<BlockData> list = [];

            while (file.Position != file.Length)
            {
                List<TileRef> definition = new(4);
                for (int index = 0; index < 4; index++)
                {
                    int word = Utils.ReadWord(file);
                    definition.Add(new TileRef(word));
                }

                list.Add(new BlockData(definition));
            }

            return list;
        }

        public static void MarkUsedAndLoadCollision(List<BlockData> blocks, List<ChunkData> chunks, string filename)
        {
            foreach (ChunkData chunk in chunks)
                foreach (BlockRef block in chunk.Definition)
                    blocks[block.Id].Used = true;

            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));

            foreach (BlockData block in blocks)
                block.Collision = Utils.ReadWord(file);
        }
    }

    internal class TileRef(int word)
    {
        public int Id { get; set; } = word & 0x7FF;

        public bool XFlip { get; set; } = (word & 0x800) != 0;

        public bool YFlip { get; set; } = (word & 0x1000) != 0;

        public byte Palette { get; set; } = (byte)((word & 0x6000) >> 13);

        public bool Priority { get; set; } = (word & 0x8000) != 0;

        public int Word =>
            (Priority ? 0x8000 : 0) |
            (Palette << 13) |
            (YFlip ? 0x800 : 0) |
            (XFlip ? 0x400 : 0) |
            Id;
    }

}
