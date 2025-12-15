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
            ChunkData.MarkPinned(chunksAct2, Utils.DeathEggChunkIDsFG);
            ChunkData.MarkPinned(chunksAct2, Utils.DeathEggChunkIDsBG);

            List<BlockData> blocksPrimary = BlockData.Load(Utils.FileBlocksPrimary);
            List<BlockData> blocksAct1 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct1)).ToList();
            List<BlockData> blocksAct2 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct2)).ToList();
            BlockData.MarkUsedAndPinned(chunksAct1, blocksAct1, (0, 0), Utils.FileCollisionAct1);
            BlockData.MarkUsedAndPinned(chunksAct2, blocksAct2, Utils.PinnedBlocksAct2, Utils.FileCollisionAct2);

            List<TileData> tilesPrimary = TileData.Load(Utils.FileTilesPrimary);
            List<TileData> tilesAct1 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct1)).ToList();
            List<TileData> tilesAct2 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct2)).ToList();
            TileData.MarkUsedAndPinned(blocksAct1, tilesAct1, Utils.PinnedTilesObjects, Utils.PinnedTilesPrimary, Utils.PinnedTilesAct1);
            TileData.MarkUsedAndPinned(blocksAct2, tilesAct2, Utils.PinnedTilesObjects, Utils.PinnedTilesPrimary, Utils.PinnedTilesAct2);



            Dictionary<int, List<TileMatch>> tileMatchesAct1 = TileMatch.FindDuplicatesInAct(tilesAct1);
            Dictionary<int, List<TileMatch>> tileMatchesAct2 = TileMatch.FindDuplicatesInAct(tilesAct2);

            (List<TileData> Primary, List<TileData> Act1, List<TileData> Act2) Tiles
                = TileMatch.FindDuplicatesAcrossActs(tileMatchesAct1, tileMatchesAct2);

            Utils.EnsurePinned(Tiles.Primary[0], Tiles.Primary, 0, padding: false);
            Utils.EnsurePinned(Tiles.Primary[0], Tiles.Act1, Tiles.Primary.Count, padding: false);
            Utils.EnsurePinned(Tiles.Primary[0], Tiles.Act2, Tiles.Primary.Count, padding: false);

            Dictionary<int, List<IdMatch>> tileIdsAct1 = Utils.GenerateIds(Tiles.Primary.Concat(Tiles.Act1).ToList(), tileMatchesAct1);
            Dictionary<int, List<IdMatch>> tileIdsAct2 = Utils.GenerateIds(Tiles.Primary.Concat(Tiles.Act2).ToList(), tileMatchesAct2);



            Dictionary<int, List<BlockMatch>> blockMatchesAct1 = BlockMatch.FindDuplicatesInAct(blocksAct1, tileIdsAct1);
            Dictionary<int, List<BlockMatch>> blockMatchesAct2 = BlockMatch.FindDuplicatesInAct(blocksAct2, tileIdsAct2);

            (List<BlockData> Primary, List<BlockData> Act1, List<BlockData> Act2) Blocks
                = BlockMatch.FindDuplicatesAcrossActs(blockMatchesAct1, blockMatchesAct2, tileIdsAct1, tileIdsAct2);

            Utils.EnsurePinned(Blocks.Primary[0], Blocks.Act2, Blocks.Primary.Count, padding: true);

            Utils.UpdateTileRefs(Blocks.Primary, tileIdsAct1);
            Utils.UpdateTileRefs(Blocks.Act1, tileIdsAct1);
            Utils.UpdateTileRefs(Blocks.Act2, tileIdsAct2);

            List<BlockData> collisionAct1 = Blocks.Primary.Concat(Blocks.Act1).ToList();
            List<BlockData> collisionAct2 = Blocks.Primary.Concat(Blocks.Act2).ToList();

            Dictionary<int, List<IdMatch>> blockIdsAct1 = Utils.GenerateIds(collisionAct1, blockMatchesAct1);
            Dictionary<int, List<IdMatch>> blockIdsAct2 = Utils.GenerateIds(collisionAct2, blockMatchesAct2);



            Dictionary<int, List<ChunkMatch>> chunkMatchesAct1 = ChunkMatch.FindDuplicatesInAct(chunksAct1, blockIdsAct1);
            Dictionary<int, List<ChunkMatch>> chunkMatchesAct2 = ChunkMatch.FindDuplicatesInAct(chunksAct2, blockIdsAct2);

            (List<ChunkData> Primary, List<ChunkData> Act1, List<ChunkData> Act2) Chunks
                = ChunkMatch.FindDuplicatesAcrossActs(chunkMatchesAct1, chunkMatchesAct2, blockIdsAct1, blockIdsAct2);

            Utils.EnsurePinned(Chunks.Primary[0], Chunks.Act2, Chunks.Primary.Count, padding: false);

            Utils.UpdateBlockRefs(Chunks.Primary, blockIdsAct1);
            Utils.UpdateBlockRefs(Chunks.Act1, blockIdsAct1);
            Utils.UpdateBlockRefs(Chunks.Act2, blockIdsAct2);

            Dictionary<int, List<IdMatch>> chunkIdsAct1 = Utils.GenerateIds(Chunks.Primary.Concat(Chunks.Act1).ToList(), chunkMatchesAct1);
            Dictionary<int, List<IdMatch>> chunkIdsAct2 = Utils.GenerateIds(Chunks.Primary.Concat(Chunks.Act2).ToList(), chunkMatchesAct2);



            Utils.UpdateChunkRefs(layoutAct1, chunkIdsAct1);
            Utils.UpdateChunkRefs(layoutAct2, chunkIdsAct2);

            List<ChunkData> chunksDeathEgg = ChunkData.Load(Utils.FileChunksDeathEgg);
            ChunkData.MarkPinned(chunksDeathEgg, Utils.DeathEggChunkIDsFG);

            List<BlockData> blocksDeathEgg = BlockData.Load(Utils.FileBlocksDeathEgg);
            BlockData.MarkUsedAndPinned(chunksDeathEgg, blocksDeathEgg, (0, 0), Utils.FileCollisionAct2);

            List<TileData> tilesDeathEgg = TileData.Load(Utils.FileTilesDeathEgg);
            TileData.MarkUsedAndPinned(blocksDeathEgg, tilesDeathEgg, Utils.PinnedTilesNone, Utils.PinnedTilesNone, Utils.PinnedTilesNone);
            tilesDeathEgg[0x11].PinnedId = 0x18B;
            tilesDeathEgg[0x51].PinnedId = 0x18C;
            tilesDeathEgg[0x51].Used = true;



            Dictionary<int, List<TileMatch>> tileMatchesDeathEgg = TileMatch.FindDuplicatesInAct(tilesDeathEgg);
            tilesDeathEgg = Utils.CreateShortlist<TileMatch, TileData>(tileMatchesDeathEgg);
            Utils.EnsurePinned(tilesDeathEgg[0], tilesDeathEgg, 0, padding: false);
            Dictionary<int, List<IdMatch>> tileIdsDeathEgg = Utils.GenerateIds(tilesDeathEgg, tileMatchesDeathEgg);

            Dictionary<int, List<BlockMatch>> blockMatchesDeathEgg = BlockMatch.FindDuplicatesInAct(blocksDeathEgg, tileIdsDeathEgg);
            blocksDeathEgg = Utils.CreateShortlist<BlockMatch, BlockData>(blockMatchesDeathEgg);
            Utils.UpdateTileRefs(blocksDeathEgg, tileIdsDeathEgg);
            Dictionary<int, List<IdMatch>> blockIdsDeathEgg = Utils.GenerateIds(blocksDeathEgg, blockMatchesDeathEgg);

            Dictionary<int, List<ChunkMatch>> chunkMatchesDeathEgg = ChunkMatch.FindDuplicatesInAct(chunksDeathEgg, blockIdsDeathEgg);
            chunksDeathEgg = Utils.CreateShortlist<ChunkMatch, ChunkData>(chunkMatchesDeathEgg);
            Utils.EnsurePinned(chunksDeathEgg[0], chunksDeathEgg, 0, padding: true);
            Utils.UpdateBlockRefs(chunksDeathEgg, blockIdsDeathEgg);



            LayoutData.Save(layoutAct1, Utils.FileLayoutAct1);
            LayoutData.Save(layoutAct2, Utils.FileLayoutAct2);

            ChunkData.Save(Chunks.Primary, Utils.FileChunksPrimary);
            ChunkData.Save(Chunks.Act1, Utils.FileChunksAct1);
            ChunkData.Save(Chunks.Act2, Utils.FileChunksAct2);

            BlockData.Save(Blocks.Primary, Utils.FileBlocksPrimary);
            BlockData.Save(Blocks.Act1, Utils.FileBlocksAct1);
            BlockData.Save(Blocks.Act2, Utils.FileBlocksAct2);

            BlockData.SaveCollision(collisionAct1, Utils.FileCollisionAct1);
            BlockData.SaveCollision(collisionAct2, Utils.FileCollisionAct2);

            TileData.Save(Tiles.Primary, Utils.FileTilesPrimary);
            TileData.Save(Tiles.Act1, Utils.FileTilesAct1);
            TileData.Save(Tiles.Act2, Utils.FileTilesAct2);



            ChunkData.Save(chunksDeathEgg, Utils.FileChunksDeathEgg);
            BlockData.Save(blocksDeathEgg, Utils.FileBlocksDeathEgg);
            TileData.Save(tilesDeathEgg, Utils.FileTilesDeathEgg);
        }
    }

}
