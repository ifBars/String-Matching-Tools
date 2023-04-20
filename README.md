SMT (String Matching Tools) is a C# library that provides two useful functions:

ExtractKeywords: This function extracts the specified number of keywords from a given input string, excluding a set of common words. Optionally, you can provide a file containing preferred words that will be taken first when selecting the keywords. This function is useful for text summarization, keyword extraction, and content analysis.

Check: This function calculates the similarity score between two input strings. It uses the Levenshtein Distance algorithm to calculate the number of edits (insertions, deletions, substitutions) required to transform one string into another. The result is normalized to a score between 0.0 (no similarity) and 1.0 (exact match).

To use SMT, follow these steps:

1. Add the SMT library to your C# project.
2. Add the namespace using StringMatchingTools; at the beginning of your code.
3. Call the desired function and pass the input parameters.

    Example usage:

         using StringMatchingTools;
         string input = "The quick brown fox jumps over the lazy dog.";
         string preferredWordsFile = "preferredWords.txt";
         int numberOfKeywords = 5;
         string keywords = input.ExtractKeywords(numberOfKeywords, preferredWordsFile);

         string input1 = "The quick brown fox jumps over the lazy dog.";
         string input2 = "The quick brown cat jumps over the lazy dog.";
         bool preProcess = true;
         double similarityScore = SMT.Check(input1, input2, preProcess);

In the first example, the function ExtractKeywords is called to extract the 5 most relevant keywords from the input string, excluding common words and giving priority to preferred words contained in the file "preferredWords.txt".

In the second example, the function Check is called to calculate the similarity score between two input strings. The third parameter preProcess indicates whether the input strings should be preprocessed (converted to lowercase, remove punctuation, and remove stop words) before the calculation. The result is a score between 0.0 and 1.0, where 1.0 means the strings are identical.

For any comments, questions, concerns, Discord: P.Penguin#7468
