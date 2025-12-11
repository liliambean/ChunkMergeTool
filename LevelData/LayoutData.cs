namespace ChunkMergeTool.LevelData
{
    internal class LayoutData(List<byte[]> foreground, List<byte[]> background)
    {
        public IEnumerable<byte[]> Rows => foreground.Concat(background);

        public static LayoutData Load(string filename)
        {
            List<byte[]> foreground;
            List<byte[]> background;

            using (FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename)))
            {
                int widthFG = Utils.ReadWord(file);
                int widthBG = Utils.ReadWord(file);
                int heightFG = Utils.ReadWord(file);
                int heightBG = Utils.ReadWord(file);
                List<int> ptrsFG = new(0x20);
                List<int> ptrsBG = new(0x20);

                for (int index = 0; index < 0x20; index++)
                {
                    ptrsFG.Add(Utils.ReadWord(file));
                    ptrsBG.Add(Utils.ReadWord(file));
                }

                foreground = ReadLayoutRow(file, widthFG, heightFG, ptrsFG);
                background = ReadLayoutRow(file, widthBG, heightBG, ptrsBG);
            }

            return new LayoutData(foreground, background);
        }

        public static void Save(LayoutData layout, string filename)
        {
            using FileStream file = File.Open(Path.Combine(Utils.WorkingDir, filename), FileMode.Open);
            file.Seek(0x88, SeekOrigin.Begin);
            file.Write(layout.Rows.SelectMany(row => row).ToArray());
        }

        private static List<byte[]> ReadLayoutRow(FileStream file, int rowLength, int rowCount, IEnumerable<int> rowPtrs)
        {
            List<byte[]> rows = new(rowCount);

            foreach (int ptr in rowPtrs)
            {
                if (rows.Count == rowCount)
                    break;

                byte[] bytes = new byte[rowLength];
                file.Seek(ptr - 0x8000, SeekOrigin.Begin);
                file.ReadExactly(bytes);
                rows.Add(bytes);
            }

            return rows;
        }
    }

}
