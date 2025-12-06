namespace ChunkMergeTool.LevelData
{
    internal class TileData(IList<byte> bytes)
    {
        public IList<byte> Bytes { get; set; } = bytes;

        public static List<TileData> Load(string filename)
        {
            var compressed = $"{filename}.bin";
            var uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: true, extract: true);

            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            var list = new List<TileData>();

            while (file.Position != file.Length)
            {
                var bytes = new byte[0x20];
                file.ReadExactly(bytes);
                list.Add(new TileData(bytes));
            }

            return list;
        }
    }

}
