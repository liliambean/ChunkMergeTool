namespace ChunkMergeTool.Analysis
{
    internal class ChunkIgnoreMatch
    {
        public string? Chunk1 { get; set; }

        public List<string>? Chunk2 { get; set; }

        public ChunkIgnoreMatch(int index1, List<int>? ignore)
        {
            Chunk1 = index1.ToString("X2");
            Chunk2 = ignore?.Select(index2 => index2.ToString("X2")).ToList();
        }

        public ChunkIgnoreMatch()
        {
        }
    }

}
