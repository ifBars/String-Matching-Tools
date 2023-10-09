using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringMatchingTools
{
    public static class SMT
    {
        private static readonly HashSet<string> CommonWords = new HashSet<string>
        {
            "the", "and", "a", "to", "in", "that", "it", "with", "as", "for", "was", "on", "are", "be", "by", "at",
            "an", "this", "who", "which", "or", "but", "not", "is", "error", "can", "were", "been", "being", "one",
            "can't", "do", "of", "if", "you", "they", "we", "all", "my", "your", "he", "she", "there", "some",
            "also", "what", "just", "so", "only", "like", "well", "will", "much", "more", "most", "no", "yes", "our"
        };

        public static string ExtractKeywords(this string input, int amt, string path = null)
        {
            List<string> preferredWords = new List<string>();
            if (path != null)
            {
                try
                {
                    preferredWords = File.ReadAllLines(path).ToList();
                }
                catch (IOException ex)
                {
                    // Handle the exception here, e.g., log it or throw a custom exception.
                    throw ex;
                }
            }

            var words = input.Split(' ')
                .Where(w => !CommonWords.Contains(w.ToLowerInvariant()));

            if (preferredWords.Count > 0)
            {
                words = words
                    .Where(w => preferredWords.Contains(w.ToLowerInvariant()))
                    .Concat(words.Where(w => !preferredWords.Contains(w.ToLowerInvariant())));
            }

            words = words.Take(amt);

            return string.Join(" ", words);
        }

        private static int Calculate(string source1, string source2)
        {
            // Generate a cache key
            string cacheKey = $"{source1}|{source2}";

            // Check if the result is already cached
            int cachedResult = RetrieveFromCache(cacheKey);
            if (cachedResult != -1)
            {
                return cachedResult;
            }

            int source1Length = source1.Length;
            int source2Length = source2.Length;

            int[,] matrix = new int[source1Length + 1, source2Length + 1];

            for (int i = 0; i <= source1Length; i++)
            {
                matrix[i, 0] = i;
            }

            for (int j = 0; j <= source2Length; j++)
            {
                matrix[0, j] = j;
            }

            for (int i = 1; i <= source1Length; i++)
            {
                for (int j = 1; j <= source2Length; j++)
                {
                    int cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            // Cache the result before returning
            CacheResult(cacheKey, matrix[source1Length, source2Length]);

            return matrix[source1Length, source2Length];
        }

        private static void CacheResult(string key, int result)
        {
            // Cache the result with a specified expiration time (e.g., 1 hour)
            DateTime expirationTime = DateTime.Now.AddHours(1);
            PCache.Cache(key, result, permanent: false, expirationTime: expirationTime);
        }

        private static int RetrieveFromCache(string key)
        {
            object cachedResult = PCache.Retrieve(key);
            if (cachedResult != null && cachedResult is int)
            {
                return (int)cachedResult;
            }
            return -1; // Indicate that the result was not found in the cache
        }

        private static string Preprocess(string input)
        {
            input = input.ToLowerInvariant();
            input = new string(input.Where(c => !char.IsPunctuation(c)).ToArray());

            var words = input.Split(' ')
                .Where(w => !CommonWords.Contains(w));

            return string.Join(" ", words);
        }

        public static double Check(string uInput, string uInput2, bool preProccess)
        {
            if (preProccess)
            {
                uInput = Preprocess(uInput);
                uInput2 = Preprocess(uInput2);
            }

            var task1 = Task.Factory.StartNew(() => Calculate(uInput, uInput2));
            var task2 = Task.Factory.StartNew(() => Calculate(uInput2, uInput));
            Task.WaitAll(task1, task2);

            int distance = Math.Min(task1.Result, task2.Result);
            double similarity = 1.0 - (double)distance / Math.Max(uInput.Length, uInput2.Length);

            return similarity;
        }
    }
}
