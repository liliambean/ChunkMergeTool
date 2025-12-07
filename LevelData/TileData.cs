namespace ChunkMergeTool.LevelData
{
    internal class TileData(IList<byte> bytes)
    {
        public IList<byte> Bytes { get; set; } = bytes;

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
    }

}
