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

            chunksAct1[0xDA].Used = true; // Pasted into layout when miniboss starts
            chunksAct2[0xA6].Used = true; // Alt death egg booster pasted into layout during cutscene
            chunksAct2[0xA7].Used = true; // Alt death egg booster pasted into layout during cutscene
            ChunkData.MarkUsedIfExistsInLayout(chunksAct1, layoutAct1);
            ChunkData.MarkUsedIfExistsInLayout(chunksAct2, layoutAct2);

            List<BlockData> blocksPrimary = BlockData.Load(Utils.FileBlocksPrimary);
            List<BlockData> blocksAct1 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct1)).ToList();
            List<BlockData> blocksAct2 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct2)).ToList();
            BlockData.LoadCollisionIntoBlocks(Utils.FileCollisionAct1, blocksAct1);
            BlockData.LoadCollisionIntoBlocks(Utils.FileCollisionAct2, blocksAct2);

            List<TileData> tilesPrimary = TileData.Load(Utils.FileTilesPrimary);
            List<TileData> tilesAct1 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct1)).ToList();
            List<TileData> tilesAct2 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct2)).ToList();

            ReportUtils.MarkDuplicateChunks(chunksAct1);
            ReportUtils.MarkDuplicateChunks(chunksAct2);
            ReportUtils.BlankUnusedChunks(chunksAct1);
            ReportUtils.BlankUnusedChunks(chunksAct2);

            List<BlockMapping?>? blockMappings = ReportUtils.AnalyzeChunks(chunksAct1, chunksAct2, blocksPrimary.Count);
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
