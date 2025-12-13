namespace ChunkMergeTool.Analysis
{
    internal interface IMatch<TData>
    {
        TData Data { get; }

        public bool XFlip { get; }

        public bool YFlip { get; }
    }

}
