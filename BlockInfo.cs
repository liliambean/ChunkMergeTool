namespace ChunkMergeTool
{
    internal class BlockInfo(List<TileRef> definition)
    {
        public List<TileRef> Definition { get; set; } = definition;

        public int Solidity { get; set; }

        public IEnumerable<int> Words => Definition.Select(tileRef => tileRef.Word);
    }

}