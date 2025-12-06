namespace ChunkMergeTool.LevelData
{
    internal class TileRef(int word)
    {
        public int Id { get; set; } = word & 0x7FF;

        public bool XFlip { get; set; } = (word & 0x800) != 0;

        public bool YFlip { get; set; } = (word & 0x1000) != 0;

        public byte Palette { get; set; } = (byte)((word & 0x6000) >> 13);

        public bool Priority { get; set; } = (word & 0x8000) != 0;

        public int Word =>
            (Priority ? 0x8000 : 0) |
            (Palette << 13) |
            (YFlip ? 0x800 : 0) |
            (XFlip ? 0x400 : 0) |
            Id;
    }

}
