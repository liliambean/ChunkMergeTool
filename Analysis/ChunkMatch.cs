using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkMatch(ChunkData chunk) : IMatch<ChunkData>
    {
        public ChunkData Data { get; set; } = chunk;

        public bool Primary { get; set; }

        public int Id { get; set; }

        public static Dictionary<int, ChunkMatch> FindMatches(
            List<ChunkData> chunks, Dictionary<int, BlockMatch> blocks)
        {
            Dictionary<int, List<ChunkMatch>> matches = [];

            for (int index = 0; index < chunks.Count; index++)
            {
                ChunkData chunk = chunks[index];
                if (!chunk.Used) continue;

                List<ChunkMatch> chunkMatches = [];

                if (chunk.Equals(chunk, blocks, blocks))
                    chunkMatches.Add(new ChunkMatch(chunk));

                matches[index] = chunkMatches;
            }

            for (int index1 = 0; index1 < chunks.Count - 1; index1++)
            {
                ChunkData chunk1 = chunks[index1];
                if (!chunk1.Used) continue;

                for (int index2 = index1 + 1; index2 < chunks.Count; index2++)
                {
                    ChunkData chunk2 = chunks[index2];
                    if (!chunk2.Used) continue;

                    List<ChunkMatch> chunk1matches = matches[index1];
                    List<ChunkMatch> chunk2matches = matches[index2];

                    if (chunk1.Equals(chunk2, blocks, blocks))
                    {
                        chunk1matches.Add(new ChunkMatch(chunk2));
                        chunk2matches.Add(new ChunkMatch(chunk1));
                    }
                }
            }

            return matches.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.OrderBy(chunk => chunks.IndexOf(chunk.Data)).First());
        }

        public static void MarkPrimary(
            Dictionary<int, ChunkMatch> matches1, Dictionary<int, ChunkMatch> matches2,
            Dictionary<int, BlockMatch> blocks1, Dictionary<int, BlockMatch> blocks2)
        {
            List<ChunkData> act1 = Utils.CreateShortlist<ChunkMatch, ChunkData>(matches1);
            List<ChunkData> act2 = Utils.CreateShortlist<ChunkMatch, ChunkData>(matches2);

            foreach (ChunkData chunk1 in act1)
            {
                foreach (ChunkData chunk2 in act2)
                    if (chunk1.Equals(chunk2, blocks1, blocks2))
                    {
                        foreach (ChunkMatch chunk in matches2.Values.Where(chunk => chunk.Data == chunk2))
                        {
                            chunk.Data = chunk1;
                            chunk.Data.Used = false;
                        }

                        chunk1.Primary = true;
                    }

                act2.RemoveAll(chunk => !chunk.Used);
            }
        }

        public static void UpdateBlockRefs(List<ChunkData> chunks, Dictionary<int, BlockMatch> matches)
        {
            foreach (ChunkData chunk in chunks)
                foreach (BlockRef blockRef in chunk.Definition)
                {
                    BlockMatch match = matches[blockRef.Id];
                    blockRef.Id = match.Id;
                    blockRef.XFlip ^= match.XFlip;
                    blockRef.YFlip ^= match.YFlip;
                }
        }
    }
}
