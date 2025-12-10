namespace ChunkMergeTool.LevelData
{
    using LayoutRow = List<byte>;

    internal class LayoutData(List<LayoutRow> foreground, List<LayoutRow> background)
    {
        public List<LayoutRow> Foreground { get; set; } = foreground;

        public List<LayoutRow> Background { get; set; } = background;

        public IEnumerable<LayoutRow> Rows => Foreground.Concat(Background);

        public static LayoutData Load(string filename)
        {
            FileStream file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));
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

            List<LayoutRow> foreground = ReadLayoutRow(file, widthFG, heightFG, ptrsFG);
            List<LayoutRow> background = ReadLayoutRow(file, widthBG, heightBG, ptrsBG);
            return new LayoutData(foreground, background);
        }

        private static List<LayoutRow> ReadLayoutRow(FileStream file, int bufferSize, int rowCount, IEnumerable<int> rowPtrs)
        {
            List<LayoutRow> rows = new(rowCount);

            foreach (int ptr in rowPtrs)
            {
                if (rows.Count == rowCount)
                    break;

                byte[] buffer = new byte[bufferSize];
                file.Seek(ptr - 0x8000, SeekOrigin.Begin);
                file.ReadExactly(buffer);
                rows.Add(buffer.ToList());
            }

            return rows;
        }
    }

}
