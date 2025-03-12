using System;
using System.Text;

namespace CourseProject.Lab2
{
    public class ReedSolomonCoding : IEncodingAlgorithm
    {
        private const int PARITY_BYTES = 4;
        
        public string Encode(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            byte[] parity = ComputeParity(data);
            byte[] encoded = new byte[data.Length + parity.Length];
            Array.Copy(data, 0, encoded, 0, data.Length);
            Array.Copy(parity, 0, encoded, data.Length, parity.Length);
            return BitConverter.ToString(encoded).Replace("-", "");
        }

        public string Decode(string encodedInput)
        {
            byte[] encoded = HexStringToBytes(encodedInput);
            if (encoded.Length < PARITY_BYTES + 1)
                throw new ArgumentException("Недостаточная длина блока для декодирования RS.");

            int k = encoded.Length - PARITY_BYTES;
            byte[] data = new byte[k];
            byte[] parity = new byte[PARITY_BYTES];
            Array.Copy(encoded, 0, data, 0, k);
            Array.Copy(encoded, k, parity, 0, PARITY_BYTES);

            byte[] syndrome = ComputeSyndrome(data, parity);
            if (IsZero(syndrome))
            {
                return Encoding.UTF8.GetString(data);
            }
            else
            {
                Console.WriteLine("Обнаружена ошибка в данных!");

                byte[] correctedData = (byte[])data.Clone();
                bool corrected = TryCorrectErrors(correctedData, parity);

                return Encoding.UTF8.GetString(corrected ? correctedData : data);
            }
        }

        public double CalculateEfficiency(string input, string encodedInput)
        {
            int originalSize = input.Length * 8;
            int encodedSize = (encodedInput.Length / 2) * 8;
            return 1 - (double)encodedSize / originalSize;
        }

        private byte[] ComputeParity(byte[] data)
        {
            byte[] parity = new byte[PARITY_BYTES];

            for (int i = 0; i < PARITY_BYTES; i++)
            {
                byte sum = 0;
                for (int j = 0; j < data.Length; j++)
                {
                    byte factor = GF256.Pow(2, i * j);
                    byte term = GF256.Multiply(data[j], factor);
                    sum = GF256.Add(sum, term);
                }
                parity[i] = sum;
            }
            return parity;
        }

        private byte[] ComputeSyndrome(byte[] data, byte[] parity)
        {
            byte[] computed = ComputeParity(data);
            byte[] syndrome = new byte[PARITY_BYTES];
            for (int i = 0; i < PARITY_BYTES; i++)
            {
                syndrome[i] = GF256.Add(computed[i], parity[i]);
            }
            return syndrome;
        }

        private bool TryCorrectErrors(byte[] data, byte[] parity)
        {
            byte[] syndrome = ComputeSyndrome(data, parity);
            if (IsZero(syndrome)) return true;

            for (int i = 0; i < data.Length; i++)
            {
                byte original = data[i];
                for (int e = 1; e < 256; e++)
                {
                    data[i] = (byte)(original ^ (byte)e);
                    byte[] newSyndrome = ComputeSyndrome(data, parity);
                    if (IsZero(newSyndrome))
                    {
                        Console.WriteLine($"Ошибка исправлена в байте {i}: {original} -> {data[i]}");
                        return true;
                    }
                }
                data[i] = original;
            }

            Console.WriteLine("Не удалось исправить ошибку!");
            return false;
        }

        private bool IsZero(byte[] array)
        {
            foreach (byte b in array)
            {
                if (b != 0)
                    return false;
            }
            return true;
        }

        private byte[] HexStringToBytes(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public string IntroduceError(string encodedInput)
        {
            byte[] encoded = HexStringToBytes(encodedInput);
            Random random = new Random();
            int errors = random.Next(1, 3);
            Console.WriteLine($"Количество ошибок: {errors}");
            for (int i = 0; i < errors; i++)
            {
                int errorIndex = random.Next(encoded.Length - PARITY_BYTES);
                byte original = encoded[errorIndex];
                byte errorValue;
                do
                {
                    errorValue = (byte)random.Next(1, 256);
                } while (errorValue == original);
                encoded[errorIndex] ^= errorValue;
                Console.WriteLine($"Ошибка внесена в байт {errorIndex}: {original} -> {encoded[errorIndex]}");
            }
            return BitConverter.ToString(encoded).Replace("-", "");
        }
    }

    public static class GF256
    {
        private const int Prim = 0x11d;
        private static readonly byte[] exp = new byte[512];
        private static readonly byte[] log = new byte[256];

        static GF256()
        {
            int x = 1;
            for (int i = 0; i < 255; i++)
            {
                exp[i] = (byte)x;
                log[x] = (byte)i;
                x <<= 1;
                if (x >= 256)
                    x ^= Prim;
            }
            for (int i = 255; i < 512; i++)
            {
                exp[i] = exp[i - 255];
            }
        }

        public static byte Add(byte a, byte b)
        {
            return (byte)(a ^ b);
        }

        public static byte Multiply(byte a, byte b)
        {
            if (a == 0 || b == 0)
                return 0;
            int index = log[a] + log[b];
            return exp[index % 255];
        }

        public static byte Divide(byte a, byte b)
        {
            if (b == 0)
                throw new DivideByZeroException();
            if (a == 0)
                return 0;
            int diff = log[a] - log[b];
            if (diff < 0)
                diff += 255;
            return exp[diff];
        }

        public static byte Pow(byte a, int power)
        {
            if (power == 0)
                return 1;
            if (a == 0)
                return 0;
            int resultLog = (log[a] * power) % 255;
            return exp[resultLog];
        }
    }
}
