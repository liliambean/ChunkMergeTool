using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Reports
{
    internal class ChunkReport
    {
        public List<List<string>> ConfirmMatches { get; set; }

        public List<List<string>> DuplicatesAct1 { get; set; }

        public List<List<string>> DuplicatesAct2 { get; set; }

        public List<ChunkIgnoreMatch> IgnoreMatches { get; set; }

        public ChunkReport(List<ChunkDataEx> chunksAct1, List<ChunkDataEx> chunksAct2, Dictionary<int, List<int>?> chunkIgnore)
        {
            ConfirmMatches = ReportUtils.GetChunkMatches(chunksAct1);
            DuplicatesAct1 = ReportUtils.GetChunkDuplicates(chunksAct1);
            DuplicatesAct2 = ReportUtils.GetChunkDuplicates(chunksAct2);
            IgnoreMatches = ReportUtils.GetChunkIgnores(chunkIgnore);
        }

#pragma warning disable CS8618
        public ChunkReport()
        {
        }
#pragma warning restore CS8618
    }

}
