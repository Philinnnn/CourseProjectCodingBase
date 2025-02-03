using System.Text;

namespace CourseProject.Lab2
{
    public class ReedSolomonCoding : IEncodingAlgorithm
    {
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
            if (encoded.Length < 3)
                throw new ArgumentException("Недостаточная длина блока для декодирования RS.");
            int k = encoded.Length - 2;
            byte[] data = new byte[k];
            byte[] parity = new byte[2];
            Array.Copy(encoded, 0, data, 0, k);
            Array.Copy(encoded, k, parity, 0, 2);

            byte[] syndrome = ComputeSyndrome(data, parity);
            if (syndrome[0] == 0 && syndrome[1] == 0)
            {
                return Encoding.UTF8.GetString(data);
            }
            else
            {
                byte[] correctedData = new byte[data.Length];
                Array.Copy(data, correctedData, data.Length);
                bool corrected = false;
                for (int i = 0; i < correctedData.Length; i++)
                {
                    byte original = correctedData[i];
                    for (int e = 1; e < 256; e++)
                    {
                        correctedData[i] = (byte)(original ^ (byte)e);
                        byte[] newSyndrome = ComputeSyndrome(correctedData, parity);
                        if (newSyndrome[0] == 0 && newSyndrome[1] == 0)
                        {
                            corrected = true;
                            break;
                        }
                    }
                    if (corrected)
                        break;
                    else
                        correctedData[i] = original;
                }
                return Encoding.UTF8.GetString(correctedData);
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
            byte[] parity = new byte[2];
            foreach (byte b in data)
            {
                parity[0] = GF256.Add(parity[0], b);
                parity[1] = GF256.Add(parity[1], GF256.Multiply(b, 2));
            }
            return parity;
        }

        private byte[] ComputeSyndrome(byte[] data, byte[] parity)
        {
            byte[] computed = ComputeParity(data);
            return new byte[] { (byte)(computed[0] ^ parity[0]), (byte)(computed[1] ^ parity[1]) };
        }

        private byte[] HexStringToBytes(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
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
            int logA = log[a];
            int logB = log[b];
            return exp[logA + logB];
        }

        public static byte Divide(byte a, byte b)
        {
            if (b == 0)
                throw new DivideByZeroException();
            if (a == 0)
                return 0;
            int logA = log[a];
            int logB = log[b];
            int diff = logA - logB;
            if (diff < 0)
                diff += 255;
            return exp[diff];
        }
    }
}
