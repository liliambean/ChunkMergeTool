namespace ChunkMergeTool.LevelData
{
    internal class ChunkData(List<BlockRef> definition)
    {
        public List<BlockRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public byte[] Bytes => Definition.Select(blockRef => blockRef.Word).ToBytes().ToArray();

        public static List<ChunkData> Load(string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            List<ChunkData> list = [];
            using (FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed)))
            {
                while (file.Position != file.Length)
                {
                    List<BlockRef> definition = new(Utils.ChunkSize);
                    for (int index = 0; index < Utils.ChunkSize; index++)
                    {
                        int word = Utils.ReadWord(file);
                        definition.Add(new BlockRef(word));
                    }

                    list.Add(new ChunkData(definition));
                }
            }

            return list;
        }

        public static void Save(List<ChunkData> chunks, string filename)
        {
            (string compressed, string uncompressed) = Utils.GetKosFileNames(filename);

            using (FileStream file = File.Open(Path.Combine(Utils.WorkingDir, uncompressed), FileMode.Create))
            {
                foreach (ChunkData chunk in chunks)
                    file.Write(chunk.Bytes);
            }

            Utils.ProcessKosFile(uncompressed, compressed, moduled: false, extract: false);
        }

        public static void MarkUsed(LayoutData layout, List<ChunkData> chunks, List<byte> usedIds)
        {
            foreach (byte[] layoutRow in layout.Rows)
                foreach (byte chunkId in layoutRow)
                    chunks[chunkId].Used = true;

            foreach (byte chunkId in usedIds)
                chunks[chunkId].Used = true;
        }
    }

    internal class BlockRef(int word)
    {
        public int Id { get; set; } = word & 0x3FF;

        public bool XFlip { get; set; } = (word & 0x400) != 0;

        public bool YFlip { get; set; } = (word & 0x800) != 0;

        public SolidKind SolidLayerA { get; set; } = (SolidKind)((word & 0x3000) >> 12);

        public SolidKind SolidLayerB { get; set; } = (SolidKind)((word & 0xC000) >> 14);

        public int Word =>
            (int)SolidLayerB << 14 |
            (int)SolidLayerA << 12 |
            (YFlip ? 0x800 : 0) |
            (XFlip ? 0x400 : 0) |
            Id;
    }

    internal enum SolidKind : byte
    {
        None,
        Top,
        Sides,
        All
    }

}
