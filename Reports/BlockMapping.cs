namespace ChunkMergeTool.Reports
{
    internal class BlockMapping
    {
        public int Id { get; set; }

        public byte Chunk1 { get; set; }

        public byte Chunk2 { get; set; }

        public bool Common { get; set; }

        public BlockMapping(int id)
        {
            Id = id;
            Common = true;
        }

        public BlockMapping(int id, byte chunk1, byte chunk2)
        {
            Id = id;
            Chunk1 = chunk1;
            Chunk2 = chunk2;
        }
    }

}
