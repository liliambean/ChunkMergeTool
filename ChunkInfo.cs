namespace ChunkMergeTool
{
    internal class ChunkInfo(List<BlockRef> definition)
    {
        public List<BlockRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public MatchType MatchType { get; set; }

        public byte Match { get; set; }

        public bool Confirmed { get; set; }

        public IEnumerable<int> Words => Definition.Select(blockRef => blockRef.Word);
    }

}