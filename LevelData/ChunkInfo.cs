using ChunkMergeTool.Analysis;

namespace ChunkMergeTool.LevelData
{
    internal class ChunkInfo(List<BlockRef> definition)
    {
        public List<BlockRef> Definition { get; set; } = definition;

        public bool Used { get; set; }

        public MatchKind MatchKind { get; set; }

        public byte Match { get; set; }

        public bool Confirmed { get; set; }

        public IEnumerable<int> Words => Definition.Select(blockRef => blockRef.Word);
    }

}