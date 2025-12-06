namespace ChunkMergeTool.LevelData
{
    internal class BlockInfo(List<TileRef> definition)
    {
        public List<TileRef> Definition { get; set; } = definition;

        public int Solidity { get; set; }

        public IEnumerable<int> Words => Definition.Select(tileRef => tileRef.Word);

        public static List<BlockInfo> Load(string filename)
        {
            var compressed = $"{filename}.bin";
            var uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            var list = new List<BlockInfo>();

            while (file.Position != file.Length)
            {
                var definition = new List<TileRef>(4);
                for (var index = 0; index < 4; index++)
                {
                    var word = Utils.ReadWord(file);
                    definition.Add(new TileRef(word));
                }

                list.Add(new BlockInfo(definition));
            }

            return list;
        }

        public static void LoadCollisionIntoBlocks(string filename, List<BlockInfo> blocks)
        {
            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));

            foreach (var block in blocks)
                block.Solidity = Utils.ReadWord(file);
        }
    }

}
