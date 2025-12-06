using System.Diagnostics;
using System.Text;

namespace ChunkMergeTool.LevelData
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

        public static int ReadWord(FileStream file)
        {
            return (file.ReadByte() << 8) | file.ReadByte();
        }

        public static void ProcessKosFile(string source, string destination, bool moduled, bool extract)
        {
            var args = new StringBuilder();

            if (extract) { args.Append("-x "); }
            if (moduled) { args.Append("-m "); }

            args.Append('"');
            args.Append(source);
            args.Append("\" \"");
            args.Append(destination);
            args.Append('"');

            var process = Process.Start(new ProcessStartInfo("koscmp.exe", args.ToString())
            {
                WorkingDirectory = WorkingDir,
                CreateNoWindow = true,
            });

            process!.WaitForExit();
        }
    }

}
