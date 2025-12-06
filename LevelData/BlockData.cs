namespace ChunkMergeTool.LevelData
{
    internal class BlockData(List<TileRef> definition)
    {
        public List<TileRef> Definition { get; set; } = definition;

        public int Solidity { get; set; }

        public IEnumerable<int> Words => Definition.Select(tileRef => tileRef.Word);

        public static List<BlockData> Load(string filename)
        {
            var compressed = $"{filename}.bin";
            var uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            var list = new List<BlockData>();

            while (file.Position != file.Length)
            {
                var definition = new List<TileRef>(4);
                for (var index = 0; index < 4; index++)
                {
                    var word = Utils.ReadWord(file);
                    definition.Add(new TileRef(word));
                }

                list.Add(new BlockData(definition));
            }

            return list;
        }

        public static void LoadCollisionIntoBlocks(string filename, List<BlockData> blocks)
        {
            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));

            foreach (var block in blocks)
                block.Solidity = Utils.ReadWord(file);
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
