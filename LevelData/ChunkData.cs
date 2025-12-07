using ChunkMergeTool.Analysis;

namespace ChunkMergeTool.LevelData
{
    internal class ChunkData(List<BlockRef> definition)
    {
        public List<BlockRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public MatchKind MatchKind { get; set; }

        public byte Match { get; set; }

        public bool Confirmed { get; set; }

        public IEnumerable<int> Words => Definition.Select(blockRef => blockRef.Word);

        public static List<ChunkData> Load(string filename)
        {
            string compressed = $"{filename}.bin";
            string uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            List<ChunkData> list = [];

            while (file.Position != file.Length)
            {
                List<BlockRef> definition = new(0x40);
                for (int index = 0; index < 0x40; index++)
                {
                    int word = Utils.ReadWord(file);
                    definition.Add(new BlockRef(word));
                }

                list.Add(new ChunkData(definition));
            }

            return list;
        }

        public static void MarkUsedIfExistsInLayout(List<ChunkData> chunks, LayoutData layout)
        {
            foreach (LayoutRow row in layout.Rows)
                foreach (byte chunk in row.Chunks)
                    chunks[chunk].Used = true;
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
