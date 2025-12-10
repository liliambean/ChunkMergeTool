using ChunkMergeTool.LevelData;
using System.Globalization;
using System.Text.Json;

namespace ChunkMergeTool.Reports
{
    internal static class ReportUtils
    {
        private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

        public static bool AnalyzeTiles(List<BlockConfirmMatch> blockConfirm, List<BlockData> blocksAct1, List<BlockData> blocksAct2, List<TileData> tilesAct1, List<TileData> tilesAct2)
        {
            foreach (BlockConfirmMatch match in blockConfirm)
            {
                BlockData block1 = blocksAct1[match.Block1];
                BlockData block2 = blocksAct2[match.Block2];

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

        public static List<BlockConfirmMatch>? AnalyzeBlocks(List<BlockMapping?> blockMappings)
        {
            List<BlockConfirmMatch> blockConfirm = [];

            for (int index = 0; index < blockMappings.Count; index++)
            {
                BlockMapping? mapping = blockMappings[index];
                if (mapping != null && !mapping.Common)
                    blockConfirm.Add(new BlockConfirmMatch(index, mapping));
            }

            string path = Path.Combine(Utils.WorkingDir, Utils.FileBlocksReport);

            if (File.Exists(path) && File.ReadAllText(path) is { Length: > 0 } text)
            {
                BlockReport report = JsonSerializer.Deserialize<BlockReport>(text)!;
                foreach (BlockConfirmMatch confirm in report.ConfirmMatches)
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
                BlockReport report = new(blockConfirm);

                using StreamWriter file = File.CreateText(path);
                file.Write(JsonSerializer.Serialize(report, jsonOptions));

                return null;
            }

            return blockConfirm;
        }

        public static List<BlockMapping?>? AnalyzeChunks(List<ChunkDataEx> chunksAct1, List<ChunkDataEx> chunksAct2, int blocksCommonCount)
        {
            Dictionary<int, List<int>?> chunkIgnore = [];
            List<(int, int)> chunkConfirm = [];
            string path = Path.Combine(Utils.WorkingDir, Utils.FileChunksReport);

            if (File.Exists(path) && File.ReadAllText(path) is { Length: > 0 } text)
            {
                ChunkReport report = JsonSerializer.Deserialize<ChunkReport>(text)!;
                foreach (ChunkIgnoreMatch ignore in report.IgnoreMatches)
                {
                    int index1 = int.Parse(ignore.Chunk1, NumberStyles.HexNumber);
                    List<int>? index2 = ignore.Chunk2?.Select(index => int.Parse(index, NumberStyles.HexNumber)).ToList();
                    chunkIgnore.Add(index1, index2);
                }

                foreach (List<string> confirm in report.ConfirmMatches)
                {
                    int index1 = int.Parse(confirm[0], NumberStyles.HexNumber);
                    int index2 = int.Parse(confirm[1], NumberStyles.HexNumber);
                    chunkConfirm.Add((index1, index2));
                }
            }

            List<BlockMapping?> blockMappings = new(0x300);
            for (int index = 0; index < blocksCommonCount; index++)
                blockMappings.Add(new BlockMapping(index));
            for (int index = blockMappings.Count; index < 0x300; index++)
                blockMappings.Add(null);

            bool errors = false;

            for (int index1 = 0; index1 < chunksAct1.Count; index1++)
            {
                if (chunkIgnore.TryGetValue(index1, out List<int>? ignore) && ignore == null)
                    continue;

                ChunkDataEx chunk1 = chunksAct1[index1];
                if (!chunk1.Used || chunk1.MatchKind != MatchKind.Unique) continue;

                for (int index2 = 1; index2 < chunksAct2.Count; index2++)
                {
                    if (ignore != null && ignore.Contains(index2))
                        continue;

                    ChunkDataEx chunk2 = chunksAct2[index2];
                    if (!chunk2.Used) continue;

                    Dictionary<int, int> guessedMappings = [];
                    bool match = true;

                    for (int blockIndex = 0; blockIndex < Utils.ChunkSize; blockIndex++)
                    {
                        BlockRef block1 = chunk1.Definition[blockIndex];
                        BlockRef block2 = chunk2.Definition[blockIndex];

                        if (block1.Id < blocksCommonCount)
                        {
                            if (block1.Id == block2.Id)
                                continue;

                            match = false;
                            break;
                        }

                        if (!guessedMappings.TryGetValue(block1.Id, out int guessedBlock2))
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

                    for (int blockIndex = 0; blockIndex < Utils.ChunkSize; blockIndex++)
                    {
                        BlockRef block1 = chunk1.Definition[blockIndex];
                        BlockRef block2 = chunk2.Definition[blockIndex];
                        BlockMapping? expected = blockMappings[block1.Id];

                        if (expected == null || expected.Id == block2.Id)
                            continue;

                        Console.WriteLine(
                            $"Expected block {expected.Id:X3} " + (expected.Common
                            ? "(part of primary set, shouldn't happen)"
                            : $"while mapping chunk {expected.Chunk1:X2} to {expected.Chunk2:X2}")
                            + $"\r\nAct 1 chunk: {index1:X2} | Act 1 block: {block1.Id:X3}"
                            + $"\r\nAct 2 chunk: {index2:X2} | Act 2 block: {block2.Id:X3}"
                        );

                        errors = true;
                    }

                    chunk1.MatchKind = MatchKind.Pending;
                    chunk1.Match = (byte)index2;

                    if (chunkConfirm.Any(match => match.Item1 == index1 && match.Item2 == index2))
                        chunk1.MatchKind = MatchKind.Confirmed;

                    foreach (KeyValuePair<int, int> m in guessedMappings)
                        blockMappings[m.Key] = new BlockMapping(m.Value, (byte)index1, (byte)index2);

                    break;
                }

                if (chunk1.MatchKind != MatchKind.Unique)
                    continue;
            }

            bool pendingAct1 = chunksAct1.Any(chunk => chunk.MatchKind == MatchKind.Pending);
            bool pendingAct2 = chunksAct2.Any(chunk => chunk.MatchKind == MatchKind.Pending);

            if (pendingAct1 || pendingAct2 || errors)
            {
                ChunkReport report = new(chunksAct1, chunksAct2, chunkIgnore);

                using StreamWriter file = File.CreateText(path);
                file.Write(JsonSerializer.Serialize(report, jsonOptions));

                return null;
            }

            return blockMappings;
        }

        public static List<ChunkDataEx> MarkDuplicateChunks(List<ChunkData> chunkData)
        {
            List<ChunkDataEx> chunks = chunkData.Select(chunk => new ChunkDataEx(chunk.Definition)).ToList();
            for (int index1 = 0; index1 < chunks.Count; index1++)
            {
                ChunkDataEx chunk1 = chunks[index1];
                if (chunk1.MatchKind == MatchKind.Duplicate) continue;

                for (int index2 = 0; index2 < chunks.Count; index2++)
                {
                    if (index1 == index2) continue;
                    ChunkDataEx chunk2 = chunks[index2];
                    bool match = true;

                    for (int blockIndex = 0; blockIndex < Utils.ChunkSize; blockIndex++)
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

            return chunks;
        }

        public static void BlankUnusedChunks(List<ChunkDataEx> chunks)
        {
            List<BlockRef> blankDefinition = chunks[0].Definition;
            foreach (ChunkDataEx chunk in chunks.Where(chunk => !chunk.Used))
                chunk.Definition = blankDefinition;
        }

        public static List<List<string>> GetChunkMatches(List<ChunkDataEx> chunks)
        {
            return chunks
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Pending)
                .Select(match => new List<string>
                {
                    match.index.ToString("X2"),
                    match.chunk.Match.ToString("X2")
                })
                .ToList();
        }


        public static List<List<string>> GetChunkDuplicates(List<ChunkDataEx> chunks)
        {
            return chunks
                .Select((chunk, index) => (chunk, index))
                .Where(match => match.chunk.MatchKind == MatchKind.Duplicate)
                .GroupBy(match => match.chunk.Match)
                .OrderBy(group => group.Key)
                .Select(group => group
                    .Select(chunk => chunk.index)
                    .Prepend(group.Key)
                    .Select(index => index.ToString("X2"))
                    .ToList())
                .ToList();
        }

        public static List<ChunkIgnoreMatch> GetChunkIgnores(Dictionary<int, List<int>?> chunkIgnore)
        {
            List<ChunkIgnoreMatch> ignoreMatches = [];

            if (chunkIgnore.Count == 0)
                ignoreMatches.Add(new ChunkIgnoreMatch(0, [0]));

            else foreach (int index1 in chunkIgnore.Keys)
            {
                List<int>? ignore = chunkIgnore[index1];
                ignoreMatches.Add(new ChunkIgnoreMatch(index1, ignore));
            }

            return ignoreMatches;
        }

        private static bool CompareTiles(TileRef tileRef1, TileRef tileRef2, List<TileData> tilesAct1, List<TileData> tilesAct2, bool xFlip, bool yFlip)
        {
            bool effectiveXFlip = xFlip ^ tileRef1.XFlip ^ tileRef2.XFlip;
            bool effectiveYFlip = yFlip ^ tileRef1.YFlip ^ tileRef2.YFlip;

            return tilesAct1[tileRef1.Id].Equals(tilesAct2[tileRef2.Id], effectiveXFlip, effectiveYFlip);
        }
    }

}
