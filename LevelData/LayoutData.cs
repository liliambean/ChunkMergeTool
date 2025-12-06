namespace ChunkMergeTool.LevelData
{
    internal class LayoutData(List<LayoutRow> foreground, List<LayoutRow> background)
    {
        public List<LayoutRow> Foreground { get; set; } = foreground;

        public List<LayoutRow> Background { get; set; } = background;

        public IEnumerable<LayoutRow> Rows => Foreground.Concat(Background);

        public static LayoutData Load(string filename)
        {
            var file = File.OpenRead(Path.Combine(Utils.WorkingDir, filename));
            var widthFG = Utils.ReadWord(file);
            var widthBG = Utils.ReadWord(file);
            var heightFG = Utils.ReadWord(file);
            var heightBG = Utils.ReadWord(file);
            var ptrsFG = new List<int>(0x20);
            var ptrsBG = new List<int>(0x20);

            for (var index = 0; index < 0x20; index++)
            {
                ptrsFG.Add(Utils.ReadWord(file));
                ptrsBG.Add(Utils.ReadWord(file));
            }

            var foreground = LayoutRow.Load(file, widthFG, heightFG, ptrsFG);
            var background = LayoutRow.Load(file, widthBG, heightBG, ptrsBG);
            return new LayoutData(foreground, background);
        }
    }

    internal class LayoutRow(IList<byte> chunks)
    {
        public IList<byte> Chunks { get; set; } = chunks;

        public static List<LayoutRow> Load(FileStream file, int bufferSize, int rowCount, IEnumerable<int> rowPtrs)
        {
            var rows = new List<LayoutRow>(rowCount);

            foreach (var ptr in rowPtrs)
            {
                if (rows.Count == rowCount)
                    break;

                var buffer = new byte[bufferSize];
                file.Seek(ptr - 0x8000, SeekOrigin.Begin);
                file.ReadExactly(buffer);
                rows.Add(new LayoutRow(buffer));
            }

            return rows;
        }
    }

}
