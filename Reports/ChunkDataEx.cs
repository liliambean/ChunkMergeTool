using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Reports
{
    internal class ChunkDataEx(List<BlockRef> definition) : ChunkData(definition)
    {
        public MatchKind MatchKind { get; set; }

        public byte Match { get; set; }

        public bool Confirmed { get; set; }
    }

}
