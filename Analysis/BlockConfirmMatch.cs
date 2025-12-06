using System.Globalization;
using System.Text.Json.Serialization;

namespace ChunkMergeTool.Analysis
{
    internal class BlockConfirmMatch
    {
        public string BlockAct1
        {
            get => Block1.ToString("X");
            set => Block1 = int.Parse(value, NumberStyles.HexNumber);
        }

        public string BlockAct2
        {
            get => Block2.ToString("X");
            set => Block2 = int.Parse(value, NumberStyles.HexNumber);
        }

        [JsonIgnore]
        public int Block1 { get; set; }

        [JsonIgnore]
        public int Block2 { get; set; }

        [JsonIgnore]
        public BlockMapping? Mapping { get; set; }

        [JsonIgnore]
        public MatchKind MatchKind { get; set; }

        public bool XFlip { get; set; }

        public bool YFlip { get; set; }

        public BlockConfirmMatch(int id, BlockMapping mapping)
        {
            Block1 = id;
            Block2 = mapping.Id;
            Mapping = mapping;
            MatchKind = MatchKind.Pending;
        }

        public BlockConfirmMatch()
        {
        }
    }

}
