﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringMatchingTools
{
    public static class SMT
    {

        private static readonly string[] CommonWords = new[]
        {
            "the", "and", "a", "to", "in", "that", "it", "with", "as", "for", "was", "on", "are", "be", "by", "at",
            "an", "this", "who", "which", "or", "but", "not", "is", "error", "can", "were", "been", "being", "one",
            "can't", "do"
        };

        public static string ExtractKeywords(this string input)
        {
            var words = input.Split(' ')
                .Where(w => !CommonWords.Contains(w.ToLowerInvariant()))
                .Take(4);

            return string.Join(" ", words);
        }
            private static int Calculate(string source1, string source2) //O(n*m)
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

        public static string Preprocess(string input)
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

        public static double Check(string uInput, string uInput2)
        {
            // Preprocess the inputs
            uInput = Preprocess(uInput);
            uInput2 = Preprocess(uInput2);

            // Use parallelism to perform the calculation
            var task1 = Task.Factory.StartNew(() => Calculate(uInput, uInput2));
            var task2 = Task.Factory.StartNew(() => Calculate(uInput2, uInput));
            Task.WaitAll(task1, task2);

            int distance = Math.Min(task1.Result, task2.Result);

            return 1.0 - (double)distance / Math.Max(uInput.Length, uInput2.Length);
        }

    }
}
