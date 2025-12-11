namespace ChunkMergeTool.LevelData
{
    internal class BlockData(List<TileRef> definition)
    {
        public List<TileRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public int Collision { get; set; }

        public byte[] Bytes => Definition.Select(tileRef => tileRef.Word).ToBytes().ToArray();

        public static List<BlockData> Load(string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            List<BlockData> list = [];
            using (FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed)))
            {
                while (file.Position != file.Length)
                {
                    List<TileRef> definition = new(4);
                    for (int index = 0; index < Utils.BlockSize; index++)
                    {
                        int word = Utils.ReadWord(file);
                        definition.Add(new TileRef(word));
                    }

                    list.Add(new BlockData(definition));
                }
            }

            return list;
        }

        public static void Save(List<BlockData> blocks, string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);

            using (FileStream file = File.Open(Path.Combine(Utils.WorkingDir, uncompressed), FileMode.Create))
            {
                foreach (BlockData block in blocks)
                    file.Write(block.Bytes);
            }

            Utils.ProcessKosFile(uncompressed, compressed, moduled: false, extract: false);
        }

        public static void SaveCollision(List<BlockData> blocks, string filename)
        {
            using FileStream file = File.Open(Path.Combine(Utils.WorkingDir, filename), FileMode.Create);
            file.Write(blocks.Select(block => block.Collision).ToBytes().ToArray());
            file.SetLength(0x600);
        }

        public static void MarkUsedAndLoadCollision(List<ChunkData> chunks, List<BlockData> blocks, string filename)
        {
            foreach (ChunkData chunk in chunks)
                foreach (BlockRef block in chunk.Definition)
                    blocks[block.Id].Used = true;

            using FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));

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
            (YFlip ? 0x1000 : 0) |
            (XFlip ? 0x800 : 0) |
            Id;
    }

}
