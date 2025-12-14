using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace ChunkMergeTool
{
    internal static class Utils
    {
        public const int ChunkSize = 0x40;
        public const int BlockSize = 4;
        public const int TileSize = 0x20;

        public const string WorkingDir = @"D:\s3unlocked\Levels\LBZ\Chunks";

        public const string FileBlocksReport = @"blocks.txt";
        public const string FileChunksReport = @"chunks.txt";

        public const string FileLayoutAct1 = @"..\Layout\1.bin";
        public const string FileLayoutAct2 = @"..\Layout\2.bin";
        public const string FileCollisionAct1 = @"..\Collision\1.bin";
        public const string FileCollisionAct2 = @"..\Collision\2.bin";

        public const string FileChunksPrimary = @"Primary";
        public const string FileChunksAct1 = @"Act 1";
        public const string FileChunksAct2 = @"Act 2";
        public const string FileBlocksPrimary = @"..\Blocks\Primary";
        public const string FileBlocksAct1 = @"..\Blocks\Act 1 Secondary";
        public const string FileBlocksAct2 = @"..\Blocks\Act 2 Secondary";
        public const string FileTilesPrimary = @"..\Tiles\Primary";
        public const string FileTilesAct1 = @"..\Tiles\Act 1 Secondary";
        public const string FileTilesAct2 = @"..\Tiles\Act 2 Secondary";

        public static readonly List<byte> EventChunkIDsAct1 = [0xDA];
        public static readonly List<byte> EventChunkIDsAct2 = [0xA6, 0xA7];
        public static readonly List<Range> PinnedTilesPrimary = [new(0, 0x48), new(0x160, 0x178)];
        public static readonly Range PinnedTilesAct1 = new(0x350, 0x36C);
        public static readonly Range PinnedTilesAct2 = new(0x2C3, 0x2E4);

        public static void ForEachFlipWhere(Func<bool, bool, bool> predicate, Action<bool, bool> callback)
        {
            if (predicate(false, false)) callback(false, false);
            if (predicate(true, false)) callback(true, false);
            if (predicate(false, true)) callback(false, true);
            if (predicate(true, true)) callback(true, true);
        }

        public static bool Equals(this ChunkData chunk1, ChunkData chunk2,
            Dictionary<int, List<IdMatch>> blockIds1, Dictionary<int, List<IdMatch>> blockIds2)
        {
            for (int index = 0; index < ChunkSize; index++)
            {
                BlockRef blockRef1 = chunk1.Definition[index];
                BlockRef blockRef2 = chunk2.Definition[index];

                if (blockRef1.SolidLayerA != blockRef2.SolidLayerA || blockRef1.SolidLayerB != blockRef2.SolidLayerB)
                    return false;

                if (!DeepEquals(blockIds1[blockRef1.Id], blockIds2[blockRef2.Id],
                    blockRef1.XFlip ^ blockRef2.XFlip, blockRef1.YFlip ^ blockRef2.YFlip))
                    return false;
            }

            return true;
        }

        public static bool Equals(this BlockData block1, BlockData block2, bool xFlip, bool yFlip,
            Dictionary<int, List<IdMatch>> tileIds1, Dictionary<int, List<IdMatch>> tileIds2)
        {
            IList<int> lookup;

            if (!xFlip && !yFlip) lookup =
            [
                0x00, 0x01,
                0x02, 0x03,
            ];
            else if (xFlip && !yFlip) lookup =
            [
                0x01, 0x00,
                0x03, 0x02,
            ];
            else if (!xFlip && yFlip) lookup =
            [
                0x02, 0x03,
                0x00, 0x01,
            ];
            else lookup =
            [
                0x03, 0x02,
                0x01, 0x00,
            ];

            for (int index = 0; index < BlockSize; index++)
            {
                TileRef tileRef1 = block1.Definition[index];
                TileRef tileRef2 = block2.Definition[lookup[index]];

                if (tileRef1.Palette != tileRef2.Palette || tileRef1.Priority != tileRef2.Priority)
                    return false;

                if (!DeepEquals(tileIds1[tileRef1.Id], tileIds2[tileRef2.Id],
                    tileRef1.XFlip ^ tileRef2.XFlip ^ xFlip, tileRef1.YFlip ^ tileRef2.YFlip ^ yFlip))
                    return false;
            }

            if (block1.Collision != block2.Collision)
            {
                // LBZ2 has a bunch of busted collision, hahaa
                if (block2.Collision == 0x0000)
                    block1.Collision = 0x0000;
                else if (block1.Collision != 0x0000)
                    block1.Collision = 0xFFFF;

                // return false;
            }

            return true;
        }

        public static bool Equals(this TileData tile1, TileData tile2, bool xFlip, bool yFlip)
        {
            IList<int> lookup;

            if (!xFlip && !yFlip) lookup =
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
            else if (xFlip && !yFlip) lookup =
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
            else if (!xFlip && yFlip) lookup =
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

            for (int index = 0; index < TileSize; index++)
            {
                int byte1 = tile1.Bytes[index];
                int byte2 = tile2.Bytes[lookup[index]];

                if (xFlip)
                    byte2 = ((byte2 & 0x0F) << 4) | ((byte2 & 0xF0) >> 4);

                if (byte1 != byte2)
                    return false;
            }

            return true;
        }

        private static bool DeepEquals(this List<IdMatch> blockIds1, List<IdMatch> blockIds2, bool xFlip, bool yFlip)
        {
            foreach (IdMatch match1 in blockIds1)
                foreach (IdMatch match2 in blockIds2)
                {
                    if (match1.Id != match2.Id)
                        continue;

                    bool match2_XFlip = match2.XFlip ^ xFlip;
                    bool match2_YFlip = match2.YFlip ^ yFlip;

                    if (match1.XFlip != match2_XFlip || match1.YFlip != match2_YFlip)
                        continue;

                    return true;
                }

            return false;
        }

        private static IdMatch GetFirstMatch(List<IdMatch> matches)
        {
            return matches.OrderBy(m => m.Id).ThenBy(m => m.XFlip).ThenBy(m => m.YFlip).First();
        }

        public static List<TData> CreateShortlist<TMatch, TData>(Dictionary<int, List<TMatch>> dictionary) where TMatch : IMatch<TData>
        {
            return dictionary
                .GroupBy(entry => entry.Value.OrderBy(m => m.Id).ThenBy(m => m.XFlip).ThenBy(m => m.YFlip).First().Data)
                .OrderBy(group => group.Min(entry => entry.Key))
                .Select(group => group.Key)
                .ToList();
        }

        public static void UpdateTileRefs(List<BlockData> blocks, Dictionary<int, List<IdMatch>> tileIds)
        {
            foreach (BlockData block in blocks)
                foreach (TileRef tileRef in block.Definition)
                {
                    IdMatch match = GetFirstMatch(tileIds[tileRef.Id]);
                    tileRef.Id = match.Id;
                    tileRef.XFlip ^= match.XFlip;
                    tileRef.YFlip ^= match.YFlip;
                }
        }

        public static void UpdateBlockRefs(List<ChunkData> chunks, Dictionary<int, List<IdMatch>> blockIds)
        {
            foreach (ChunkData chunk in chunks)
                foreach (BlockRef blockRef in chunk.Definition)
                {
                    IdMatch match = GetFirstMatch(blockIds[blockRef.Id]);
                    blockRef.Id = match.Id;
                    blockRef.XFlip ^= match.XFlip;
                    blockRef.YFlip ^= match.YFlip;
                }
        }

        public static void UpdateChunkRefs(LayoutData layout, Dictionary<int, List<IdMatch>> chunkIds)
        {
            foreach (byte[] layoutRow in layout.Rows)
                for (int index = 0; index < layoutRow.Length; index++)
                {
                    IdMatch match = GetFirstMatch(chunkIds[layoutRow[index]]);
                    layoutRow[index] = (byte)match.Id;
                }
        }

        public static Dictionary<int, List<IdMatch>> GenerateIds<TMatch, TData>(
            List<TData> data, Dictionary<int, List<TMatch>> matches) where TMatch : IMatch<TData>
        {
            return matches.ToDictionary(entry => entry.Key,
                entry => entry.Value.Where(match => data.Contains(match.Data)).Select(match => new IdMatch(
                    data.IndexOf(match.Data),
                    match.XFlip,
                    match.YFlip)
                ).ToList());
        }

        public static int ReadWord(FileStream file)
        {
            return (file.ReadByte() << 8) | file.ReadByte();
        }

        public static IEnumerable<byte> ToBytes(this IEnumerable<int> words)
        {
            foreach (int word in words)
            {
                yield return (byte)(word >> 8);
                yield return (byte)word;
            }

            yield break;
        }

        public static (string, string) GetKosFileNames(string filename)
        {
            return ($"{filename}.bin", $"{filename} unc.bin");
        }

        public static void ProcessKosFile(string source, string destination, bool moduled, bool extract)
        {
            StringBuilder args = new();

            if (extract) { args.Append("-x "); }
            if (moduled) { args.Append("-m "); }

            args.Append('"');
            args.Append(source);
            args.Append("\" \"");
            args.Append(destination);
            args.Append('"');

            Process process = Process.Start(new ProcessStartInfo("koscmp.exe", args.ToString())
            {
                WorkingDirectory = WorkingDir,
                CreateNoWindow = true,
            })!;

            process.WaitForExit();
        }
    }

}
