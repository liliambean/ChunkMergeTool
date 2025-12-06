namespace ChunkMergeTool.LevelData
{
    internal static class TileInfo
    {
        public static List<IList<byte>> Load(string filename)
        {
            var compressed = $"{filename}.bin";
            var uncompressed = $"{filename} unc.bin";
            Utils.ProcessKosFile(compressed, uncompressed, moduled: true, extract: true);

            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, uncompressed));
            var list = new List<IList<byte>>();

            while (file.Position != file.Length)
            {
                var bytes = new byte[0x20];
                file.ReadExactly(bytes);
                list.Add(bytes);
            }

            return list;
        }
    }

}
