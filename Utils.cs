using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;
using System.Diagnostics;
using System.Text;

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

        public static int ReadWord(FileStream file)
        {
            return (file.ReadByte() << 8) | file.ReadByte();
        }

        public static void ForEachFlipWhere(Func<bool, bool, bool> predicate, Action<bool, bool> callback)
        {
            if (predicate(false, false)) callback(false, false);
            if (predicate(true, false)) callback(true, false);
            if (predicate(false, true)) callback(false, true);
            if (predicate(true, true)) callback(true, true);
        }

        public static bool Equals(
            this ChunkData chunk1, ChunkData chunk2,
            Dictionary<int, BlockMatch> blocks1, Dictionary<int, BlockMatch> blocks2)
        {
            for (int index = 0; index < ChunkSize; index++)
            {
                BlockRef blockRef1 = chunk1.Definition[index];
                BlockRef blockRef2 = chunk2.Definition[index];

                if (blockRef1.SolidLayerA != blockRef2.SolidLayerA || blockRef1.SolidLayerB != blockRef2.SolidLayerB)
                    return false;

                BlockMatch match1 = blocks1[blockRef1.Id];
                BlockMatch match2 = blocks2[blockRef2.Id];

                if (match1.Id != match2.Id || match1.XFlip != match2.XFlip || match1.YFlip != match2.YFlip)
                    return false;
            }

            return true;
        }

        public static bool Equals(
            this BlockData block1, BlockData block2, bool xFlip, bool yFlip,
            Dictionary<int, TileMatch> tiles1, Dictionary<int, TileMatch> tiles2)
        {
            if (block1.Collision != block2.Collision)
                return false;

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

                TileMatch match1 = tiles1[tileRef1.Id];
                TileMatch match2 = tiles2[tileRef2.Id];
                bool xFlip2 = match2.XFlip ^ xFlip;
                bool yFlip2 = match2.YFlip ^ yFlip;

                if (match1.Id != match2.Id || match1.XFlip != xFlip2 || match1.YFlip != yFlip2)
                    return false;
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

        public static List<TData> CreateShortlist<TMatch, TData>(
            Dictionary<int, TMatch> dictionary) where TData: IData where TMatch : IMatch<TData>
        {
            return dictionary
                .GroupBy(entry => entry.Value.Data)
                .OrderBy(group => group.Min(entry => entry.Key))
                .Select(group => group.Key)
                .ToList();
        }

        public static (List<TData>, List<TData>, List<TData>) GenerateLists<TMatch, TData>(
            Dictionary<int, TMatch> matches1, Dictionary<int, TMatch> matches2) where TMatch : IMatch<TData> where TData : IData
        {
            List<TData> act1 = CreateShortlist<TMatch, TData>(matches1);
            List<TData> act2 = CreateShortlist<TMatch, TData>(matches2);
            List<TData> primary = act1.Where(item => item.Primary).ToList();

            act1.RemoveAll(primary.Contains);
            act2.RemoveAll(primary.Contains);

            return (primary, act1, act2);
        }

        public static List<TData> EnsureIds<TMatch, TData>(
            List<TData> data, Dictionary<int, TMatch> matches) where TMatch : IMatch<TData> where TData : IData
        {
            foreach (TMatch match in matches.Values)
                match.Id = data.IndexOf(match.Data);

            return data;
        }

        public static void UpdateChunkRefs(LayoutData layout, Dictionary<int, ChunkMatch> matches)
        {
            foreach (byte[] layoutRow in layout.Rows)
                for (int index = 0; index < layoutRow.Length; index++)
                    layoutRow[index] = (byte)matches[layoutRow[index]].Id;
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
