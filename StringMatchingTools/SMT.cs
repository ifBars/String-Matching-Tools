using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections;

namespace StringMatchingTools
{
    public static class SMT
    {

        private static readonly IReadOnlyCollection<string> CommonWords = new[]
        {
            "the", "and", "a", "to", "in", "that", "it", "with", "as", "for", "was", "on", "are", "be", "by", "at",
            "an", "this", "who", "which", "or", "but", "not", "is", "error", "can", "were", "been", "being", "one",
            "can't", "do", "of", "if", "you", "they", "we", "all", "my", "your", "he", "she", "there", "some",
            "also", "what", "just", "so", "only", "like", "well", "will", "much", "more", "most", "no", "yes", "our"
        };

        public static string ExtractKeywords(this string input, int amt, string path = null)
        {
            IReadOnlyList<string> preferredWords = new string[0];
            if (path != null)
            {
                // Read words from the text file and store them in a list
                preferredWords = File.ReadAllLines(path);
            }

            // Split string into words
            var words = input.Split(' ')
                // Remove common words from string
                .Where(w => !CommonWords.Contains(w.ToLowerInvariant()))
                // If preferred words are provided, take them first
                .OrderByDescending(w => preferredWords.Contains(w.ToLowerInvariant()))
                // Take the specified amount of keywords
                .Take(amt);

            // Return Keywords
            return string.Join(" ", words);
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

        public static double Check(string uInput, string uInput2, bool preProcess)
        {
            if (preProcess)
            {
                uInput = Preprocess(uInput);
                uInput2 = Preprocess(uInput2);
            }

            int length = uInput.Length;
            int length2 = uInput2.Length;
            int minLength = Math.Min(length, length2);

            // Prepare a vector for the substitution cost of matching characters
            Vector<byte> match = new Vector<byte>((byte)1);

            // Prepare vectors for the insertion and deletion costs
            Vector<byte> insertion = new Vector<byte>((byte)1);
            Vector<byte> deletion = new Vector<byte>((byte)1);

            // Initialize the distances array
            int[] distances = new int[length2 + 1];

            for (int i = 0; i <= length2; i++)
            {
                distances[i] = i;
            }

            // Use parallelism to perform the calculation
            Parallel.For(0, length, i =>
            {
                int previousDistance = i;
                byte c1 = (byte)uInput[i];

                // Calculate the distances for the current row using SIMD instructions
                for (int j = 0; j < length2; j += Vector<byte>.Count)
                {
                    // Load a vector of characters from the second string
                    Vector<byte> v2 = new Vector<byte>(uInput2.Substring(j, Vector<byte>.Count).PadRight(Vector<byte>.Count, '\0').Select(c => (byte)c).ToArray());

                    // Compare the vectors to find matching characters
                    Vector<byte> mask = Vector.Equals(v2, new Vector<byte>(c1));

                    // Calculate the substitution costs using the mask
                    Vector<byte> vMatch = Vector.BitwiseAnd(mask, match);
                    Vector<byte> vSubstitution = Vector.Add(vMatch, Vector.Subtract(Vector<byte>.One, mask));

                    // Calculate the insertion and deletion costs
                    Vector<byte> vInsertion = new Vector<byte>(insertion) + Vector<byte>.One;
                    Vector<byte> vDeletion = new Vector<byte>(deletion) + Vector<byte>.One;

                    // Calculate the new distances vector using the minimum of the costs
                    Vector<byte> vDistances = Vector.Min(Vector.Min(vInsertion, vDeletion), vSubstitution);

                    // Store the new distances vector in the distances array
                    byte[] distancesArray = new byte[Vector<byte>.Count];
                    vDistances.CopyTo(distancesArray);
                    for (int k = 0; k < Vector<byte>.Count; k++)
                    {
                        distances[j + k + 1] = Math.Min(Math.Min(distances[j + k + 1], distances[j + k] + 1), previousDistance + distancesArray[k]);
                    }

                    // Update the previous distance
                    previousDistance = distances[j + Vector<byte>.Count];
                }

                // Update the last element of the distances array
                distances[length2] = Math.Min(distances[length2], previousDistance + 1);
            });

            double similarity = 1.0 - (double)distances[length2] / Math.Max(uInput.Length, uInput2.Length);
            return similarity;
        }

    }
}
