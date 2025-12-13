namespace ChunkMergeTool.Analysis
{
    internal class IdMatch(int id, bool xFlip, bool yFlip)
    {
        public int Id { get; set; } = id;

        public bool XFlip { get; set; } = xFlip;

        public bool YFlip { get; set; } = yFlip;
    }

}
