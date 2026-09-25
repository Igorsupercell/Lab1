using System.Text;

namespace GeneticSearch
{
    static class RLDecoder
    {
        public static string Decode(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    int count = input[i] - '0';
                    if (i + 1 < input.Length)
                    {
                        char nextChar = input[i + 1];
                        sb.Append(nextChar, count);
                        i++;
                    }
                }
                else
                {
                    sb.Append(input[i]);
                }
            }

            return sb.ToString();
        }
    }
}