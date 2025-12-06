using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;
using System.Globalization;
using System.Text.Json;

namespace ChunkMergeTool
{
    internal class Program
    {
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        private static void Main()
        {
            var layoutAct1 = LayoutData.Load(Utils.FileLayoutAct1);
            var layoutAct2 = LayoutData.Load(Utils.FileLayoutAct2);
            var chunksAct1 = ChunkData.Load(Utils.FileChunksAct1);
            var chunksAct2 = ChunkData.Load(Utils.FileChunksAct2);

            chunksAct1[0xDA].Used = true; // Pasted into layout when miniboss starts
            chunksAct2[0xA6].Used = true; // Alt death egg booster pasted into layout during cutscene
            chunksAct2[0xA7].Used = true; // Alt death egg booster pasted into layout during cutscene
            ChunkData.MarkUsedIfExistsInLayout(chunksAct1, layoutAct1);
            ChunkData.MarkUsedIfExistsInLayout(chunksAct2, layoutAct2);

            var blocksPrimary = BlockData.Load(Utils.FileBlocksPrimary);
            var blocksAct1 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct1)).ToList();
            var blocksAct2 = blocksPrimary.Concat(BlockData.Load(Utils.FileBlocksAct2)).ToList();
            BlockData.LoadCollisionIntoBlocks(Utils.FileCollisionAct1, blocksAct1);
            BlockData.LoadCollisionIntoBlocks(Utils.FileCollisionAct2, blocksAct2);

            var tilesPrimary = TileData.Load(Utils.FileTilesPrimary);
            var tilesAct1 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct1)).ToList();
            var tilesAct2 = tilesPrimary.Concat(TileData.Load(Utils.FileTilesAct2)).ToList();

            MarkDuplicateChunks(chunksAct1);
            MarkDuplicateChunks(chunksAct2);
            BlankUnusedChunks(chunksAct1);
            BlankUnusedChunks(chunksAct2);

            var blockMappings = AnalyzeChunks(chunksAct1, chunksAct2, blocksPrimary.Count);
            if (blockMappings == null)
            {
                Console.WriteLine("Completed with errors; a report has been created.");
                return;
            }

            var blockConfirm = AnalyzeBlocks(blockMappings);
            if (blockConfirm == null)
            {
                Console.WriteLine("Completed with errors; a report has been created.");
                return;
            }

            if (!AnalyzeTiles(blockConfirm, blocksAct1, blocksAct2, tilesAct1, tilesAct2))
            {
                Console.WriteLine("Tile mismatch error");
                return;
            }
        }

        private static void MarkDuplicateChunks(List<ChunkData> chunks)
        {
            for (var index1 = 0; index1 < chunks.Count; index1++)
            {
                var chunk1 = chunks[index1];
                if (chunk1.MatchKind == MatchKind.Duplicate) continue;

                for (var index2 = 0; index2 < chunks.Count; index2++)
                {
                    if (index1 == index2) continue;
                    var chunk2 = chunks[index2];
                    var match = true;

                    for (var blockIndex = 0; blockIndex < 0x40; blockIndex++)
                    {
                        if (chunk1.Definition[blockIndex].Word != chunk2.Definition[blockIndex].Word)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        chunk2.MatchKind = MatchKind.Duplicate;
                        chunk2.Match = (byte)index1;
                    }
                }
            }
        }

        private static void BlankUnusedChunks(List<ChunkData> chunks)
        {
            var blankDefinition = chunks[0].Definition;
            foreach (var chunk in chunks.Where(chunk => !chunk.Used))
                chunk.Definition = blankDefinition;
        }

        private static List<BlockMapping?>? AnalyzeChunks(List<ChunkData> chunksAct1, List<ChunkData> chunksAct2, int blocksCommonCount)
        {
            var chunkIgnore = new Dictionary<int, List<int>?>();
            var chunkConfirm = new List<(int, int)>();
            var path = Path.Combine(Utils.WorkingDir, Utils.FileChunksReport);

            if (File.Exists(path) && File.ReadAllText(path) is { Length: > 0 } text)
            {
                var report = JsonSerializer.Deserialize<ChunkReport>(text)!;
                foreach (var ignore in report.IgnoreMatches!)
                {
                    var index1 = int.Parse(ignore.Chunk1!, NumberStyles.HexNumber);
                    var index2 = ignore.Chunk2?.Select(index => int.Parse(index, NumberStyles.HexNumber)).ToList();
                    chunkIgnore.Add(index1, index2);
                }

                foreach (var confirm in report.ConfirmMatches!)
                {
                    var index1 = int.Parse(confirm[0], NumberStyles.HexNumber);
                    var index2 = int.Parse(confirm[1], NumberStyles.HexNumber);
                    chunkConfirm.Add((index1, index2));
                }
            }

            var blockMappings = new List<BlockMapping?>(0x300);
            for (var index = 0; index < blocksCommonCount; index++)
                blockMappings.Add(new BlockMapping(index));
            for (var index = blockMappings.Count; index < 0x300; index++)
                blockMappings.Add(null);

            var errors = false;

            for (var index1 = 0; index1 < chunksAct1.Count; index1++)
            {
                if (chunkIgnore.TryGetValue(index1, out var ignore) && ignore == null)
                    continue;

                var chunk1 = chunksAct1[index1];
                if (!chunk1.Used || chunk1.MatchKind != MatchKind.Unique) continue;

                for (int index2 = 1; index2 < chunksAct2.Count; index2++)
                {
                    if (ignore != null && ignore.Contains(index2))
                        continue;

                    var chunk2 = chunksAct2[index2];
                    if (!chunk2.Used) continue;

                    var guessedMappings = new Dictionary<int, int>();
                    var match = true;

                    for (var blockIndex = 0; blockIndex < 0x40; blockIndex++)
                    {
                        var block1 = chunk1.Definition[blockIndex];
                        var block2 = chunk2.Definition[blockIndex];

                        if (block1.Id < blocksCommonCount)
                        {
                            if (block1.Id == block2.Id)
                                continue;

                            match = false;
                            break;
                        }

                        if (!guessedMappings.TryGetValue(block1.Id, out var guessedBlock2))
                        {
                            guessedMappings.Add(block1.Id, block2.Id);
                        }
                        else if (block2.Id != guessedBlock2)
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;

                    for (var blockIndex = 0; blockIndex < 0x40; blockIndex++)
                    {
                        var block1 = chunk1.Definition[blockIndex];
                        var block2 = chunk2.Definition[blockIndex];
                        var expected = blockMappings[block1.Id];

                        if (expected == null || expected.Id == block2.Id)
                            continue;

                        Console.WriteLine(
                            $"Act 1 chunk: {index1:X}\r\n" +
                            $"Act 2 chunk: {index2:X}\r\n" +
                            $"Act 1 block: {block1.Id:X}\r\n" +
                            $"Act 2 block: {block2.Id:X}\r\n" +
                            $"Expected block: {expected.Id:X}\r\n" +
                            (expected.Common ? "Block is part of primary set (shouldn't happen)" :
                            $"Guess produced while mapping chunk {expected.Chunk1:X} to {expected.Chunk2:X}")
                        );

                        errors = true;
                    }

                    chunk1.MatchKind = MatchKind.Pending;
                    chunk1.Match = (byte)index2;

                    if (chunkConfirm.Any(match => match.Item1 == index1 && match.Item2 == index2))
                        chunk1.MatchKind = MatchKind.Confirmed;

                    foreach (var m in guessedMappings)
                        blockMappings[m.Key] = new BlockMapping(m.Value, (byte)index1, (byte)index2);

                    break;
                }

                if (chunk1.MatchKind != MatchKind.Unique)
                    continue;
            }

            var pendingAct1 = chunksAct1.Any(chunk => chunk.MatchKind == MatchKind.Pending);
            var pendingAct2 = chunksAct2.Any(chunk => chunk.MatchKind == MatchKind.Pending);

            if (pendingAct1 || pendingAct2 || errors)
            {
                var report = new ChunkReport(chunksAct1, chunksAct2, chunkIgnore);

                using var file = File.CreateText(path);
                file.Write(JsonSerializer.Serialize(report, jsonOptions));

                return null;
            }

            return blockMappings;
        }

        private static List<BlockConfirmMatch>? AnalyzeBlocks(List<BlockMapping?> blockMappings)
        {
            var blockConfirm = new List<BlockConfirmMatch>();

            for (var index = 0; index < blockMappings.Count; index++)
            {
                var mapping = blockMappings[index];
                if (mapping != null && !mapping.Common)
                    blockConfirm.Add(new BlockConfirmMatch(index, mapping));
            }

            var path = Path.Combine(Utils.WorkingDir, Utils.FileBlocksReport);

            if (File.Exists(path) && File.ReadAllText(path) is { Length: > 0 } text)
            {
                var report = JsonSerializer.Deserialize<BlockReport>(text)!;
                foreach (var confirm in report.ConfirmMatches!)
                {
                    if (blockConfirm.FirstOrDefault(match => match.Block1 == confirm.Block1
                        && match.Block2 == confirm.Block2) is BlockConfirmMatch match)
                    {
                        match.XFlip = confirm.XFlip;
                        match.YFlip = confirm.YFlip;
                        match.MatchKind = MatchKind.Confirmed;
                    }
                }
            }

            if (blockConfirm.Any(match => match.MatchKind != MatchKind.Confirmed))
            {
                var report = new BlockReport(blockConfirm);

                using var file = File.CreateText(path);
                file.Write(JsonSerializer.Serialize(report, jsonOptions));

                return null;
            }

            return blockConfirm;
        }

        private static bool AnalyzeTiles(List<BlockConfirmMatch> blockConfirm, List<BlockData> blocksAct1, List<BlockData> blocksAct2, List<IList<byte>> tilesAct1, List<IList<byte>> tilesAct2)
        {
            foreach (var match in blockConfirm)
            {
                var block1 = blocksAct1[match.Block1];
                var block2 = blocksAct2[match.Block2];

                if (!match.XFlip && !match.YFlip)
                {
                    if (!CompareTiles(block1.Definition[0], block2.Definition[0], tilesAct1, tilesAct2, false, false) ||
                        !CompareTiles(block1.Definition[1], block2.Definition[1], tilesAct1, tilesAct2, false, false) ||
                        !CompareTiles(block1.Definition[2], block2.Definition[2], tilesAct1, tilesAct2, false, false) ||
                        !CompareTiles(block1.Definition[3], block2.Definition[3], tilesAct1, tilesAct2, false, false))
                        return false;
                }
                else if (match.XFlip && !match.YFlip)
                {
                    if (!CompareTiles(block1.Definition[0], block2.Definition[1], tilesAct1, tilesAct2, true, false) ||
                        !CompareTiles(block1.Definition[1], block2.Definition[0], tilesAct1, tilesAct2, true, false) ||
                        !CompareTiles(block1.Definition[2], block2.Definition[3], tilesAct1, tilesAct2, true, false) ||
                        !CompareTiles(block1.Definition[3], block2.Definition[2], tilesAct1, tilesAct2, true, false))
                        return false;
                }
                else if (!match.XFlip && match.YFlip)
                {
                    if (!CompareTiles(block1.Definition[0], block2.Definition[2], tilesAct1, tilesAct2, false, true) ||
                        !CompareTiles(block1.Definition[1], block2.Definition[3], tilesAct1, tilesAct2, false, true) ||
                        !CompareTiles(block1.Definition[2], block2.Definition[0], tilesAct1, tilesAct2, false, true) ||
                        !CompareTiles(block1.Definition[3], block2.Definition[1], tilesAct1, tilesAct2, false, true))
                        return false;
                }
                else
                {
                    if (!CompareTiles(block1.Definition[0], block2.Definition[3], tilesAct1, tilesAct2, true, true) ||
                        !CompareTiles(block1.Definition[1], block2.Definition[2], tilesAct1, tilesAct2, true, true) ||
                        !CompareTiles(block1.Definition[2], block2.Definition[1], tilesAct1, tilesAct2, true, true) ||
                        !CompareTiles(block1.Definition[3], block2.Definition[0], tilesAct1, tilesAct2, true, true))
                        return false;
                }
            }

            return true;
        }

        private static bool CompareTiles(TileRef tileAct1, TileRef tileAct2, List<IList<byte>> tilesAct1, List<IList<byte>> tilesAct2, bool xFlip, bool yFlip)
        {
            var effectiveXFlip = xFlip ^ tileAct1.XFlip ^ tileAct2.XFlip;
            var effectiveYFlip = yFlip ^ tileAct1.YFlip ^ tileAct2.YFlip;
            IList<int> lookup;

            if (!effectiveXFlip && !effectiveYFlip) lookup =
            [
                0x00, 0x01, 0x02, 0x03,
                0x04, 0x05, 0x06, 0x07,
                0x08, 0x09, 0x0A, 0x0B,
                0x0C, 0x0D, 0x0E, 0x0F,
                0x10, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17,
                0x18, 0x19, 0x1A, 0x1B,
                0x1C, 0x1D, 0x1E, 0x1F,
            ];
            else if (effectiveXFlip && !effectiveYFlip) lookup =
            [
                0x03, 0x02, 0x01, 0x00,
                0x07, 0x06, 0x05, 0x04,
                0x0B, 0x0A, 0x09, 0x08,
                0x0F, 0x0E, 0x0D, 0x0C,
                0x13, 0x12, 0x11, 0x10,
                0x17, 0x16, 0x15, 0x14,
                0x1B, 0x1A, 0x19, 0x18,
                0x1F, 0x1E, 0x1D, 0x1C,
            ];
            else if (!effectiveXFlip && effectiveYFlip) lookup =
            [
                0x1C, 0x1D, 0x1E, 0x1F,
                0x18, 0x19, 0x1A, 0x1B,
                0x14, 0x15, 0x16, 0x17,
                0x10, 0x11, 0x12, 0x13,
                0x0C, 0x0D, 0x0E, 0x0F,
                0x08, 0x09, 0x0A, 0x0B,
                0x04, 0x05, 0x06, 0x07,
                0x00, 0x01, 0x02, 0x03,
            ];
            else lookup =
            [
                0x1F, 0x1E, 0x1D, 0x1C,
                0x1B, 0x1A, 0x19, 0x18,
                0x17, 0x16, 0x15, 0x14,
                0x13, 0x12, 0x11, 0x10,
                0x0F, 0x0E, 0x0D, 0x0C,
                0x0B, 0x0A, 0x09, 0x08,
                0x07, 0x06, 0x05, 0x04,
                0x03, 0x02, 0x01, 0x00,
            ];

            var tile1 = tilesAct1[tileAct1.Id];
            var tile2 = tilesAct2[tileAct2.Id];

            for (var index = 0; index < 0x20; index++)
            {
                var byte1 = tile1[index];
                var byte2 = tile2[lookup[index]];

                if (effectiveXFlip)
                    byte2 = (byte)(((byte2 & 0x0F) << 4) | ((byte2 & 0xF0) >> 4));

                if (byte1 != byte2)
                    return false;
            }

            return true;
        }

    }

}