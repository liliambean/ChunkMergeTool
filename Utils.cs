using ChunkMergeTool.Analysis;
using ChunkMergeTool.LevelData;
using System.Diagnostics;
using System.Text;

namespace ChunkMergeTool
{
    internal static class Utils
    {
        public static readonly string WorkingDir = @"D:\s3unlocked\Levels\LBZ\Chunks";

        public static readonly string FileBlocksReport = @"blocks.txt";
        public static readonly string FileChunksReport = @"chunks.txt";

        public static readonly string FileLayoutAct1 = @"..\Layout\1.bin";
        public static readonly string FileLayoutAct2 = @"..\Layout\2.bin";
        public static readonly string FileCollisionAct1 = @"..\Collision\1.bin";
        public static readonly string FileCollisionAct2 = @"..\Collision\2.bin";

        public static readonly string FileChunksAct1 = @"Act 1";
        public static readonly string FileChunksAct2 = @"Act 2";
        public static readonly string FileBlocksPrimary = @"..\Blocks\Primary";
        public static readonly string FileBlocksAct1 = @"..\Blocks\Act 1 Secondary";
        public static readonly string FileBlocksAct2 = @"..\Blocks\Act 2 Secondary";
        public static readonly string FileTilesPrimary = @"..\Tiles\Primary";
        public static readonly string FileTilesAct1 = @"..\Tiles\Act 1 Secondary";
        public static readonly string FileTilesAct2 = @"..\Tiles\Act 2 Secondary";

        public static readonly List<int> EventChunkIDsAct1 = [0xDA];
        public static readonly List<int> EventChunkIDsAct2 = [0xA6, 0xA7];
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

        public static List<TData> CreateShortlist<TMatch, TData>(Dictionary<int, TMatch> dictionary) where TData: IData where TMatch : IMatch<TData>
        {
            return dictionary
                .GroupBy(entry => entry.Value.Data)
                .OrderBy(group => group.Min(entry => entry.Key))
                .Select(group => group.Key)
                .ToList();
        }

        public static (List<TData>, List<TData>, List<TData>) GenerateLists<TMatch, TData>(
            Dictionary<int, TMatch> matches1, Dictionary<int, TMatch> matches2) where TData : IData where TMatch : IMatch<TData>
        {
            List<TData> act1 = CreateShortlist<TMatch, TData>(matches1);
            List<TData> act2 = CreateShortlist<TMatch, TData>(matches2);
            List<TData> primary = act1.Where(item => item.Primary).ToList();

            act1.RemoveAll(primary.Contains);
            act2.RemoveAll(primary.Contains);

            return (primary, act1, act2);
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
