using ChunkMergeTool.Analysis;

namespace ChunkMergeTool.LevelData
{
    internal class ChunkInfo(List<BlockRef> definition)
    {
        public List<BlockRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public MatchKind MatchKind { get; set; }

        public byte Match { get; set; }

        public bool Confirmed { get; set; }

        public IEnumerable<int> Words => Definition.Select(blockRef => blockRef.Word);

        public static List<ChunkInfo> Load(string filename)
        {
            var compressed = $"{filename}.bin";
            var uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: false, extract: true);

            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            var list = new List<ChunkInfo>();

            while (file.Position != file.Length)
            {
                var definition = new List<BlockRef>(0x40);
                for (var index = 0; index < 0x40; index++)
                {
                    var word = Utils.ReadWord(file);
                    definition.Add(new BlockRef(word));
                }

                list.Add(new ChunkInfo(definition));
            }

            return list;
        }

        public static void MarkUsedIfExistsInLayout(List<ChunkInfo> chunks, LayoutInfo layout)
        {
            foreach (var row in layout.Rows)
                foreach (var chunk in row.Chunks)
                    chunks[chunk].Used = true;
        }
    }

}
