namespace ChunkMergeTool
{
    internal class LayoutRow(IList<byte> chunks)
    {
        public IList<byte> Chunks { get; set; } = chunks;
    }

}