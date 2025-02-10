using System.Text;

namespace CourseProject.Lab2
{
    public class ShannonFanoCoding : IEncodingAlgorithm
    {
        private Dictionary<char, string>? shannonFanoTable;
        public string Encode(string input)
        {
            var frequencies = input.GroupBy(c => c)
                                   .Select(g => new SymbolFrequency { Symbol = g.Key, Frequency = g.Count() })
                                   .OrderByDescending(x => x.Frequency)
                                   .ToList();
            shannonFanoTable = new Dictionary<char, string>();
            GenerateCodes(frequencies, "");
            return string.Concat(input.Select(c => shannonFanoTable[c]));
        }
        public string Decode(string encodedInput)
        {
            var reverseTable = shannonFanoTable.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            var result = new StringBuilder();
            var current = "";
            foreach (var bit in encodedInput)
            {
                current += bit;
                if (reverseTable.ContainsKey(current))
                {
                    result.Append(reverseTable[current]);
                    current = "";
                }
            }
            return result.ToString();
        }
        public double CalculateEfficiency(string input, string encodedInput)
        {
            int originalSize = input.Length * 8;
            int encodedSize = encodedInput.Length;
            return 1 - (double)encodedSize / originalSize;
        }
        private void GenerateCodes(List<SymbolFrequency> symbols, string prefix)
        {
            if (symbols.Count == 1)
            {
                shannonFanoTable[symbols[0].Symbol] = prefix == "" ? "0" : prefix;
                return;
            }
            int total = symbols.Sum(s => s.Frequency);
            int half = total / 2;
            int sum = 0;
            int splitIndex = 0;
            for (int i = 0; i < symbols.Count; i++)
            {
                sum += symbols[i].Frequency;
                if (sum >= half)
                {
                    splitIndex = i;
                    break;
                }
            }
            var left = symbols.Take(splitIndex + 1).ToList();
            var right = symbols.Skip(splitIndex + 1).ToList();
            GenerateCodes(left, prefix + "0");
            if (right.Count > 0)
                GenerateCodes(right, prefix + "1");
        }
    }
    public class SymbolFrequency
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
    }
}
