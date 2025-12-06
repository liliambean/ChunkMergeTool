namespace ChunkMergeTool.LevelData
{
    internal class LayoutInfo(List<LayoutRow> foreground, List<LayoutRow> background)
    {
        public List<LayoutRow> Foreground { get; set; } = foreground;

        public List<LayoutRow> Background { get; set; } = background;

        public IEnumerable<LayoutRow> Rows => Foreground.Concat(Background);
    }

    internal class LayoutRow(IList<byte> chunks)
    {
        public IList<byte> Chunks { get; set; } = chunks;
    }

}