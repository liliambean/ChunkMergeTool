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

        public static bool CompareTiles(TileRef tileRef1, TileRef tileRef2, List<TileData> tilesAct1, List<TileData> tilesAct2, bool xFlip, bool yFlip)
        {
            bool effectiveXFlip = xFlip ^ tileRef1.XFlip ^ tileRef2.XFlip;
            bool effectiveYFlip = yFlip ^ tileRef1.YFlip ^ tileRef2.YFlip;
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

            TileData tile1 = tilesAct1[tileRef1.Id];
            TileData tile2 = tilesAct2[tileRef2.Id];

            for (int index = 0; index < 0x20; index++)
            {
                byte byte1 = tile1.Bytes[index];
                byte byte2 = tile2.Bytes[lookup[index]];

                if (effectiveXFlip)
                    byte2 = (byte)(((byte2 & 0x0F) << 4) | ((byte2 & 0xF0) >> 4));

                if (byte1 != byte2)
                    return false;
            }

            return true;
        }
    }

}
