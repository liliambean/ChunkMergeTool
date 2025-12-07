namespace ChunkMergeTool.Reports
{
    internal class BlockReport
    {
        public List<BlockConfirmMatch> ConfirmMatches { get; set; }

        public BlockReport(List<BlockConfirmMatch> blockConfirm)
        {
            ConfirmMatches = blockConfirm;
        }

#pragma warning disable CS8618
        public BlockReport()
        {
        }
#pragma warning restore CS8618
    }

}
