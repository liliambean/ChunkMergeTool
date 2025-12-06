namespace ChunkMergeTool
{
    internal class BlockReport
    {
        public List<BlockConfirmMatch>? ConfirmMatches { get; set; }

        public BlockReport(List<BlockConfirmMatch> blockConfirm)
        {
            ConfirmMatches = blockConfirm;
        }

        public BlockReport()
        {
        }
    }

}