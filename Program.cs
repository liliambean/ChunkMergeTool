using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;

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
            TileData.MarkUsedAndPinned(blocksAct1, tilesAct1, Utils.PinnedTilesPrimary, Utils.PinnedTilesAct1);
            TileData.MarkUsedAndPinned(blocksAct2, tilesAct2, Utils.PinnedTilesPrimary, Utils.PinnedTilesAct2);



            Dictionary<int, TileMatch> tileMatchesAct1 = TileMatch.FindMatches(tilesAct1);
            Dictionary<int, TileMatch> tileMatchesAct2 = TileMatch.FindMatches(tilesAct2);
            TileMatch.MarkPrimary(tileMatchesAct1, tileMatchesAct2);

            (List<TileData> Primary, List<TileData> Act1, List<TileData> Act2) Tiles
                = Utils.GenerateLists<TileMatch, TileData>(tileMatchesAct1, tileMatchesAct2);

            TileData.EnsurePinned(Tiles.Primary, 0);
            TileData.EnsurePinned(Tiles.Act1, Tiles.Primary.Count);
            TileData.EnsurePinned(Tiles.Act2, Tiles.Primary.Count);

            Utils.EnsureIds(new List<TileData>([..Tiles.Primary, ..Tiles.Act1]), tileMatchesAct1);
            Utils.EnsureIds(new List<TileData>([..Tiles.Primary, ..Tiles.Act2]), tileMatchesAct2);



            Dictionary<int, BlockMatch> blockMatchesAct1 = BlockMatch.FindMatches(blocksAct1, tileMatchesAct1);
            Dictionary<int, BlockMatch> blockMatchesAct2 = BlockMatch.FindMatches(blocksAct2, tileMatchesAct2);
            BlockMatch.MarkPrimary(blockMatchesAct1, blockMatchesAct2, tileMatchesAct1, tileMatchesAct2);

            (List<BlockData> Primary, List<BlockData> Act1, List<BlockData> Act2) Blocks
                = Utils.GenerateLists<BlockMatch, BlockData>(blockMatchesAct1, blockMatchesAct2);

            BlockMatch.UpdateTileRefs(Blocks.Primary, tileMatchesAct1);
            BlockMatch.UpdateTileRefs(Blocks.Act1, tileMatchesAct1);
            BlockMatch.UpdateTileRefs(Blocks.Act2, tileMatchesAct2);

            Utils.EnsureIds(new List<BlockData>([..Blocks.Primary, ..Blocks.Act1]), blockMatchesAct1);
            Utils.EnsureIds(new List<BlockData>([..Blocks.Primary, ..Blocks.Act2]), blockMatchesAct2);



            Dictionary<int, ChunkMatch> chunkMatchesAct1 = ChunkMatch.FindMatches(chunksAct1, blockMatchesAct1);
            Dictionary<int, ChunkMatch> chunkMatchesAct2 = ChunkMatch.FindMatches(chunksAct2, blockMatchesAct2);
            ChunkMatch.MarkPrimary(chunkMatchesAct1, chunkMatchesAct2, blockMatchesAct1, blockMatchesAct2);

            (List<ChunkData> Primary, List<ChunkData> Act1, List<ChunkData> Act2) Chunks
                = Utils.GenerateLists<ChunkMatch, ChunkData>(chunkMatchesAct1, chunkMatchesAct2);

            ChunkMatch.UpdateBlockRefs(Chunks.Primary, blockMatchesAct1);
            ChunkMatch.UpdateBlockRefs(Chunks.Act1, blockMatchesAct1);
            ChunkMatch.UpdateBlockRefs(Chunks.Act2, blockMatchesAct2);

            Utils.EnsureIds(new List<ChunkData>([..Chunks.Primary, ..Chunks.Act1]), chunkMatchesAct1);
            Utils.EnsureIds(new List<ChunkData>([..Chunks.Primary, ..Chunks.Act2]), chunkMatchesAct2);



            Utils.UpdateChunkRefs(layoutAct1, chunkMatchesAct1);
            Utils.UpdateChunkRefs(layoutAct2, chunkMatchesAct2);

            return;
        }
    }

}
