using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace CourseProject.Lab5
{
    public class RleCoding : IEncodingAlgorithm
    {
        public string Encode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            StringBuilder encoded = new StringBuilder();

            int count = 1;
            char current = input[0];

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == current)
                {
                    count++;
                }
                else
                {
                    encoded.Append(current);
                    encoded.Append(count.ToString(CultureInfo.InvariantCulture));
                    current = input[i];
                    count = 1;
                }
            }

            encoded.Append(current);
            encoded.Append(count.ToString(CultureInfo.InvariantCulture));

            return encoded.ToString();
        }

        public string Decode(string encodedInput)
        {
            if (string.IsNullOrEmpty(encodedInput))
                return string.Empty;

            StringBuilder decoded = new StringBuilder();

            for (int i = 0; i < encodedInput.Length;)
            {
                char character = encodedInput[i++];
                StringBuilder countStr = new StringBuilder();
                
                while (i < encodedInput.Length && char.IsDigit(encodedInput[i]))
                {
                    countStr.Append(encodedInput[i]);
                    i++;
                }

                if (int.TryParse(countStr.ToString(), out int count))
                {
                    decoded.Append(character, count);
                }
                else
                {
                    throw new FormatException("Некорректный формат закодированной строки.");
                }
            }

            return decoded.ToString();
        }

        public double CalculateEfficiency(string input, string encodedInput)
        {
            if (string.IsNullOrEmpty(input))
                return 0.0;

            double originalSize = input.Length;
            double encodedSize = encodedInput.Length;

            return originalSize == 0 ? 0 : (1.0 - encodedSize / originalSize) * 100.0;
        }

        public string EncodeBytes(byte[] input)
        {
            string inputString = Convert.ToBase64String(input);
            return Encode(inputString);
        }

        public byte[] DecodeBytes(string encodedInput)
        {
            string decodedString = Decode(encodedInput);
            return Convert.FromBase64String(decodedString);
        }

        public static string EncodeMatrix(double[,] matrix)
        {
            StringBuilder builder = new StringBuilder();
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    builder.Append(matrix[i, j].ToString(CultureInfo.InvariantCulture));
                    builder.Append(",");
                }
                builder.Append(";");
            }

            return new RleCoding().Encode(builder.ToString());
        }

        public static double[,] DecodeMatrix(string encodedMatrix, int rows, int cols)
        {
            string decodedString = new RleCoding().Decode(encodedMatrix);
            string[] rowStrings = decodedString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] colStrings = rowStrings[i].Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = double.Parse(colStrings[j], CultureInfo.InvariantCulture);
                }
            }

            return matrix;
        }

        public string Encode(int[] input)
        {
            if (input == null || input.Length == 0)
                return string.Empty;

            StringBuilder encoded = new StringBuilder();
            int count = 1;
            int current = input[0];

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] == current)
                {
                    count++;
                }
                else
                {
                    encoded.Append($"{current}:{count},");
                    current = input[i];
                    count = 1;
                }
            }

            encoded.Append($"{current}:{count}");
            return encoded.ToString();
        }

        public int[] DecodeToIntArray(string encodedInput)
        {
            if (string.IsNullOrEmpty(encodedInput))
                return Array.Empty<int>();

            var parts = encodedInput.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<int> decoded = new List<int>();

            foreach (var part in parts)
            {
                var pair = part.Split(':');
                int value = int.Parse(pair[0]);
                int count = int.Parse(pair[1]);

                decoded.AddRange(Enumerable.Repeat(value, count));
            }

            return decoded.ToArray();
        }
    }
}
