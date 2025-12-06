namespace ChunkMergeTool.LevelData
{
    internal class BlockRef(int word)
    {
        public int Id { get; set; } = word & 0x3FF;

        public bool XFlip { get; set; } = (word & 0x400) != 0;

        public bool YFlip { get; set; } = (word & 0x800) != 0;

        public BlockSolidity ForegroundSolid { get; set; } = (BlockSolidity)((word & 0x3000) >> 12);

        public BlockSolidity BackgroundSolid { get; set; } = (BlockSolidity)((word & 0xC000) >> 14);

        public int Word =>
            (int)BackgroundSolid << 14 |
            (int)ForegroundSolid << 12 |
            (YFlip ? 0x800 : 0) |
            (XFlip ? 0x400 : 0) |
            Id;
    }

    internal enum BlockSolidity : byte
    {
        None,
        Top,
        Sides,
        All
    }

}