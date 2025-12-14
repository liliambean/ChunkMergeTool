using ChunkMergeTool.LevelData;

namespace ChunkMergeTool.Analysis
{
    internal class ChunkMatch(int id, ChunkData chunk) : IMatch<ChunkData>
    {
        public int Id { get; set; } = id;

        public ChunkData Data { get; set; } = chunk;

        public bool XFlip => false;

        public bool YFlip => false;

        public static Dictionary<int, List<ChunkMatch>> FindDuplicatesInAct(List<ChunkData> chunks, Dictionary<int, List<IdMatch>> blockIds)
        {
            Dictionary<int, List<ChunkMatch>> matches = [];

            for (int index = 0; index < chunks.Count; index++)
            {
                ChunkData chunk = chunks[index];
                if (!chunk.Used) continue;

                List<ChunkMatch> chunkMatches = [];

                if (chunk.Equals(chunk, matches, matches, blockIds, blockIds))
                    chunkMatches.Add(new ChunkMatch(index, chunk));

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

                    if (chunk1.Equals(chunk2, matches, matches, blockIds, blockIds))
                    {
                        chunk1matches.Add(new ChunkMatch(index2, chunk2));
                        chunk2matches.Add(new ChunkMatch(index1, chunk1));
                    }
                }
            }

            return matches;
        }

        public static (List<ChunkData>, List<ChunkData>, List<ChunkData>) FindDuplicatesAcrossActs(
            Dictionary<int, List<ChunkMatch>> matches1, Dictionary<int, List<ChunkMatch>> matches2,
            Dictionary<int, List<IdMatch>> blockIds1, Dictionary<int, List<IdMatch>> blockIds2)
        {
            List<ChunkData> act1 = Utils.CreateShortlist<ChunkMatch, ChunkData>(matches1);
            List<ChunkData> act2 = Utils.CreateShortlist<ChunkMatch, ChunkData>(matches2);
            List<ChunkData> primary = [];

            foreach (ChunkData chunk1 in act1)
            {
                bool isMatch = false;

                foreach (ChunkData chunk2 in act2)
                    if (chunk1.Equals(chunk2, matches1, matches2, blockIds1, blockIds2))
                    {
                        foreach (ChunkMatch match in matches2.SelectMany(entry => entry.Value).Where(match => match.Data == chunk2))
                            match.Data = chunk1;

                        isMatch = true;
                        chunk2.Used = false;
                    }

                act2.RemoveAll(chunk => !chunk.Used);
                if (isMatch) primary.Add(chunk1);
            }

            act1.RemoveAll(primary.Contains);
            return (primary, act1, act2);
        }
    }
}
