using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal interface IMatch<TData> where TData: IData
    {
        TData Data { get; set; }

        int Id { get; set; }
    }
}
