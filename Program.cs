using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;
using ChunkMergeTool.Reports;

namespace ChunkMergeTool
{
    internal class Program
    {
        private static void Main()
        {
            LayoutData layoutAct1 = LayoutData.Load(Utils.FileLayoutAct1);
            LayoutData layoutAct2 = LayoutData.Load(Utils.FileLayoutAct2);

            List<ChunkData> chunksAct1 = ChunkData.Load(Utils.FileChunksAct1);
            List<ChunkData> chunksAct2 = ChunkData.Load(Utils.FileChunksAct2);
            ChunkData.MarkUsed(layoutAct1, chunksAct1, Utils.EventChunkIDsAct1);
            ChunkData.MarkUsed(layoutAct2, chunksAct2, Utils.EventChunkIDsAct2);

            List<BlockData> blocksPrimary = BlockData.Load(Utils.FileBlocksPrimary);
            List<BlockData> blocksAct1 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct1)).ToList();
            List<BlockData> blocksAct2 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct2)).ToList();
            BlockData.MarkUsedAndLoadCollision(chunksAct1, blocksAct1, Utils.FileCollisionAct1);
            BlockData.MarkUsedAndLoadCollision(chunksAct2, blocksAct2, Utils.FileCollisionAct2);

            List<TileData> tilesPrimary = TileData.Load(Utils.FileTilesPrimary);
            List<TileData> tilesAct1 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct1)).ToList();
            List<TileData> tilesAct2 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct2)).ToList();
            TileData.MarkUsedAndPinned(blocksAct1, tilesAct1, Utils.AnimatedTileIDsAct1);
            TileData.MarkUsedAndPinned(blocksAct2, tilesAct2, Utils.AnimatedTileIDsAct2);

            Dictionary<int, TileMatch> tileMatchesAct1 = TileMatch.FindMatches(tilesAct1);
            Dictionary<int, TileMatch> tileMatchesAct2 = TileMatch.FindMatches(tilesAct2);
            TileMatch.Merge(tileMatchesAct1, tileMatchesAct2);

            Dictionary<int, BlockMatch> blockMatchesAct1 = BlockMatch.FindMatches(blocksAct1, tileMatchesAct1);
            Dictionary<int, BlockMatch> blockMatchesAct2 = BlockMatch.FindMatches(blocksAct2, tileMatchesAct2);
            BlockMatch.Merge(blockMatchesAct1, blockMatchesAct2);

            Dictionary<int, ChunkMatch> chunkMatchesAct1 = ChunkMatch.FindMatches(chunksAct1, blockMatchesAct1);
            Dictionary<int, ChunkMatch> chunkMatchesAct2 = ChunkMatch.FindMatches(chunksAct2, blockMatchesAct2);
            ChunkMatch.Merge(chunkMatchesAct1, chunkMatchesAct2);



            List<ChunkDataEx> chunksEx1 = ReportUtils.MarkDuplicateChunks(chunksAct1);
            List<ChunkDataEx> chunksEx2 = ReportUtils.MarkDuplicateChunks(chunksAct2);
            ReportUtils.BlankUnusedChunks(chunksEx1);
            ReportUtils.BlankUnusedChunks(chunksEx2);

            List<BlockMapping?>? blockMappings = ReportUtils.AnalyzeChunks(chunksEx1, chunksEx2, blocksPrimary.Count);
            if (blockMappings == null)
            {
                Console.WriteLine("Completed with errors; a report has been created.");
                return;
            }

            List<BlockConfirmMatch>? blockConfirm = ReportUtils.AnalyzeBlocks(blockMappings);
            if (blockConfirm == null)
            {
                Console.WriteLine("Completed with errors; a report has been created.");
                return;
            }

            if (!ReportUtils.AnalyzeTiles(blockConfirm, blocksAct1, blocksAct2, tilesAct1, tilesAct2))
            {
                Console.WriteLine("Tile mismatch error");
                return;
            }
        }
    }

}
