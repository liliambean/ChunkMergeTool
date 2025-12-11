namespace ChunkMergeTool.Analysis
{
    internal interface IMatch<TData>
    {
        TData Data { get; set; }

        int Id { get; set; }
    }
}
