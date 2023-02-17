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
                // Read words from the text file and store them in a list
                preferredWords = File.ReadAllLines(path).ToList();
            }

            // Split string into words
            var words = input.Split(' ')
                // Remove common words from string
                .Where(w => !CommonWords.Contains(w.ToLowerInvariant()));

                // If preferred words are provided, take them first
            if (preferredWords.Count > 0)
            {
                words = words
                    .Where(w => preferredWords.Contains(w.ToLowerInvariant()))
                    .Concat(words.Where(w => !preferredWords.Contains(w.ToLowerInvariant())));
            }

            // Take the specified amount of keywords
            words = words.Take(amt);

            // Return Keywords
            return string.Join(" ", words);
        }

            private static int Calculate(string source1, string source2)
            {
                var source1Length = source1.Length;
                var source2Length = source2.Length;

                var matrix = new int[source1Length + 1, source2Length + 1];

                // First calculation, if one entry is empty return full length
                if (source1Length == 0)
                    return source2Length;

                if (source2Length == 0)
                    return source1Length;

                // Initialization of matrix with row size source1Length and columns size source2Length
                for (var i = 0; i <= source1Length; matrix[i, 0] = i++) { }
                for (var j = 0; j <= source2Length; matrix[0, j] = j++) { }

                // Calculate rows and collumns distances
                for (var i = 1; i <= source1Length; i++)
                {
                    for (var j = 1; j <= source2Length; j++)
                    {
                        var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                        matrix[i, j] = Math.Min(
                            Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                            matrix[i - 1, j - 1] + cost);
                    }
                }
                // return result
                return matrix[source1Length, source2Length];
            }

        private static string Preprocess(string input)
        {
            // Convert to lowercase
            input = input.ToLowerInvariant();

            // Remove punctuation
            input = new string(input.Where(c => !char.IsPunctuation(c)).ToArray());

            // Remove stop words
            var words = input.Split(' ')
                .Where(w => !CommonWords.Contains(w));

            return string.Join(" ", words);
        }

        public static int Check(string uInput, string uInput2, bool preProccess)
        {

            if(preProccess == true)
            {
                // Preprocess the inputs
                uInput = Preprocess(uInput);
                uInput2 = Preprocess(uInput2);
            }

            // Use parallelism to perform the calculation
            var task1 = Task.Factory.StartNew(() => Calculate(uInput, uInput2));
            var task2 = Task.Factory.StartNew(() => Calculate(uInput2, uInput));
            Task.WaitAll(task1, task2);

            int distance = Math.Min(task1.Result, task2.Result);

            // Return distance between strings
            return ((int)(1.0 - distance / Math.Max(uInput.Length, uInput2.Length)));
        }

    }
}
