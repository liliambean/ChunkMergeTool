namespace ChunkMergeTool.Analysis
{
    internal interface IMatch<TData>
    {
        public int Id { get; }

        TData Data { get; }

        public bool XFlip { get; }

        public bool YFlip { get; }
    }

}
