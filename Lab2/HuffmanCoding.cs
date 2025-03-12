using System.Text;

namespace CourseProject.Lab2
{
    public class HuffmanCoding : IEncodingAlgorithm
    {
        public Dictionary<char, string>? HuffmanTable;
        private HuffmanNode? root;

        public string Encode(string input)
        {
            BuildHuffmanTree(input);
            return string.Concat(input.Select(c => HuffmanTable[c]));
        }

        public string Decode(string encodedInput)
        {
            if (HuffmanTable == null)
                throw new NullReferenceException("HuffmanTable is null. Невозможно декодировать без таблицы.");
            
            var inverseTable = HuffmanTable.ToDictionary(kv => kv.Value, kv => kv.Key);

            var result = new StringBuilder();
            string currentCode = "";
            foreach (var bit in encodedInput)
            {
                currentCode += bit;
                if (inverseTable.ContainsKey(currentCode))
                {
                    result.Append(inverseTable[currentCode]);
                    currentCode = "";
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

        private void BuildHuffmanTree(string input)
        {
            var frequencies = input.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            var nodes = new List<HuffmanNode>();
            foreach (var kv in frequencies)
            {
                nodes.Add(new HuffmanNode(kv.Key, kv.Value));
            }
            while (nodes.Count > 1)
            {
                nodes = nodes.OrderBy(n => n.Frequency).ToList();
                var left = nodes[0];
                var right = nodes[1];
                nodes.RemoveRange(0, 2);
                nodes.Add(new HuffmanNode(left, right));
            }
            root = nodes[0];
            HuffmanTable = new Dictionary<char, string>();
            GenerateHuffmanCodes(root, "");
        }

        private void GenerateHuffmanCodes(HuffmanNode node, string code)
        {
            if (node.IsLeaf())
            {
                HuffmanTable[node.Symbol] = code;
                return;
            }
            GenerateHuffmanCodes(node.Left, code + "0");
            GenerateHuffmanCodes(node.Right, code + "1");
        }
    }

    public class HuffmanNode
    {
        public char Symbol { get; }
        public int Frequency { get; }
        public HuffmanNode Left { get; }
        public HuffmanNode Right { get; }

        public HuffmanNode(char symbol, int frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
        }

        public HuffmanNode(HuffmanNode left, HuffmanNode right)
        {
            Left = left;
            Right = right;
            Frequency = left.Frequency + right.Frequency;
        }

        public bool IsLeaf() => Left == null && Right == null;
    }
}
