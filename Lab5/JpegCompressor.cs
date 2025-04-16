using System;
using System.Text;
using CourseProject.Lab2;

namespace CourseProject.Lab5
{
    public static class JpegCompressor
    {
        public static (byte[] compressedData, byte[] restoredPixels, int width, int height, int stride) Compress(
            byte[] pixels, int width, int height, bool use420 = true, int quality = 75)
        {
            int stride = width * 4;

            // --- STEP 1: RGB to YCbCr ---
            double[,] Y = new double[height, width];
            double[,] Cb = new double[height, width];
            double[,] Cr = new double[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int i = y * stride + x * 4;
                    byte B = pixels[i];
                    byte G = pixels[i + 1];
                    byte R = pixels[i + 2];

                    Y[y, x] = 0.299 * R + 0.587 * G + 0.114 * B;
                    Cb[y, x] = -0.168736 * R - 0.331264 * G + 0.5 * B + 128;
                    Cr[y, x] = 0.5 * R - 0.418688 * G - 0.081312 * B + 128;
                }

            // --- STEP 2: Subsampling ---
            int chromaWidth = use420 ? width / 2 : width / 2;
            int chromaHeight = use420 ? height / 2 : height;

            double[,] CbSub = new double[chromaHeight, chromaWidth];
            double[,] CrSub = new double[chromaHeight, chromaWidth];

            for (int y = 0; y < chromaHeight; y++)
                for (int x = 0; x < chromaWidth; x++)
                {
                    if (use420)
                    {
                        CbSub[y, x] = (Cb[y * 2, x * 2] + Cb[y * 2, x * 2 + 1] + Cb[y * 2 + 1, x * 2] + Cb[y * 2 + 1, x * 2 + 1]) / 4.0;
                        CrSub[y, x] = (Cr[y * 2, x * 2] + Cr[y * 2, x * 2 + 1] + Cr[y * 2 + 1, x * 2] + Cr[y * 2 + 1, x * 2 + 1]) / 4.0;
                    }
                    else // 4:2:2
                    {
                        CbSub[y, x] = (Cb[y, x * 2] + Cb[y, x * 2 + 1]) / 2.0;
                        CrSub[y, x] = (Cr[y, x * 2] + Cr[y, x * 2 + 1]) / 2.0;
                    }
                }

            // --- STEP 3: Block 8x8 + DCT + Quantization ---
            double[,] Q = GetQuantMatrix(quality);
            ApplyBlockwiseDCTAndQuant(ref Y, Q);
            ApplyBlockwiseDCTAndQuant(ref CbSub, Q);
            ApplyBlockwiseDCTAndQuant(ref CrSub, Q);

            // --- STEP 4: Zigzag + RLE + Huffman ---
            var rle = new RleCoding();
            var huffman = new HuffmanCoding();

            var compressedY = CompressMatrix(Y, Q, rle, huffman);
            var compressedCb = CompressMatrix(CbSub, Q, rle, huffman);
            var compressedCr = CompressMatrix(CrSub, Q, rle, huffman);
            
            var compressedData = CombineCompressedData(compressedY, compressedCb, compressedCr);

            // --- STEP 5: Dequantize + IDCT ---
            ApplyBlockwiseIDCT(ref Y, Q);
            ApplyBlockwiseIDCT(ref CbSub, Q);
            ApplyBlockwiseIDCT(ref CrSub, Q);

            // --- STEP 6: Upsampling ---
            double[,] CbFull = new double[height, width];
            double[,] CrFull = new double[height, width];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int subY = use420 ? y / 2 : y;
                    int subX = x / 2;
                    subY = Math.Min(subY, chromaHeight - 1);
                    subX = Math.Min(subX, chromaWidth - 1);

                    if (use420)
                    {
                        double fracY = (y % 2) / 2.0;
                        double fracX = (x % 2) / 2.0;
                        int y0 = Math.Min(subY, chromaHeight - 2);
                        int x0 = Math.Min(subX, chromaWidth - 2);

                        CbFull[y, x] = (1 - fracY) * (1 - fracX) * CbSub[y0, x0] +
                                       fracY * (1 - fracX) * CbSub[y0 + 1, x0] +
                                       (1 - fracY) * fracX * CbSub[y0, x0 + 1] +
                                       fracY * fracX * CbSub[y0 + 1, x0 + 1];

                        CrFull[y, x] = (1 - fracY) * (1 - fracX) * CrSub[y0, x0] +
                                       fracY * (1 - fracX) * CrSub[y0 + 1, x0] +
                                       (1 - fracY) * fracX * CrSub[y0, x0 + 1] +
                                       fracY * fracX * CrSub[y0 + 1, x0 + 1];
                    }
                    else
                    {
                        double fracX = (x % 2) / 2.0;
                        CbFull[y, x] = (1 - fracX) * CbSub[subY, subX] + fracX * CbSub[subY, Math.Min(subX + 1, chromaWidth - 1)];
                        CrFull[y, x] = (1 - fracX) * CrSub[subY, subX] + fracX * CrSub[subY, Math.Min(subX + 1, chromaWidth - 1)];
                    }
                }

            // --- STEP 7: YCbCr to RGB ---
            byte[] restoredPixels = new byte[height * stride];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    double Yv = Y[y, x];
                    double Cbv = CbFull[y, x] - 128;
                    double Crv = CrFull[y, x] - 128;

                    double R = Yv + 1.402 * Crv;
                    double G = Yv - 0.344136 * Cbv - 0.714136 * Crv;
                    double B = Yv + 1.772 * Cbv;

                    int i = y * stride + x * 4;
                    restoredPixels[i] = Clamp(B);
                    restoredPixels[i + 1] = Clamp(G);
                    restoredPixels[i + 2] = Clamp(R);
                    restoredPixels[i + 3] = 255;
                }

            return (compressedData, restoredPixels, width, height, stride);
        }

        private static string CompressMatrix(double[,] matrix, double[,] Q, RleCoding rle, HuffmanCoding huffman)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            StringBuilder compressed = new StringBuilder();

            for (int y = 0; y < rows; y += 8)
                for (int x = 0; x < cols; x += 8)
                {
                    double[,] block = GetBlock(matrix, x, y);
                    double[,] quantizedBlock = QuantizeBlock(block, Q);
                    int[] zigzagArray = ZigzagScan(quantizedBlock);
                    string rleEncoded = rle.Encode(string.Join(",", zigzagArray));
                    compressed.Append(huffman.Encode(rleEncoded));
                    compressed.Append(";");
                }

            return compressed.ToString();
        }

        private static double[,] QuantizeBlock(double[,] block, double[,] Q)
        {
            double[,] quantized = new double[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    quantized[i, j] = Math.Round(block[i, j] / Q[i, j]);
            return quantized;
        }

        private static int[] ZigzagScan(double[,] block)
        {
            int[] zigzag = new int[64];
            int[,] zigzagOrder = {
                { 0, 1, 5, 6,14,15,27,28 },
                { 2, 4, 7,13,16,26,29,42 },
                { 3, 8,12,17,25,30,41,43 },
                { 9,11,18,24,31,40,44,53 },
                {10,19,23,32,39,45,52,54 },
                {20,22,33,38,46,51,55,60 },
                {21,34,37,47,50,56,59,61 },
                {35,36,48,49,57,58,62,63 }
            };

            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    zigzag[zigzagOrder[i, j]] = (int)block[i, j];

            return zigzag;
        }

        private static byte[] CombineCompressedData(string y, string cb, string cr)
        {
            string combined = $"{y}|{cb}|{cr}";
            return Encoding.UTF8.GetBytes(combined);
        }

        private static void ApplyBlockwiseDCTAndQuant(ref double[,] data, double[,] Q)
        {
            int h = data.GetLength(0);
            int w = data.GetLength(1);
            for (int y = 0; y < h; y += 8)
                for (int x = 0; x < w; x += 8)
                {
                    double[,] block = GetBlock(data, x, y);
                    double[,] dct = DCT(block);
                    double[,] quant = new double[8, 8];
                    for (int i = 0; i < 8; i++)
                        for (int j = 0; j < 8; j++)
                            quant[i, j] = Math.Round(dct[i, j] / Q[i, j]);
                    SetBlock(ref data, x, y, quant);
                }
        }

        private static void ApplyBlockwiseIDCT(ref double[,] data, double[,] Q)
        {
            int h = data.GetLength(0);
            int w = data.GetLength(1);
            for (int y = 0; y < h; y += 8)
                for (int x = 0; x < w; x += 8)
                {
                    double[,] block = GetBlock(data, x, y);
                    double[,] dequant = new double[8, 8];
                    for (int i = 0; i < 8; i++)
                        for (int j = 0; j < 8; j++)
                            dequant[i, j] = block[i, j] * Q[i, j];
                    double[,] idct = IDCT(dequant);
                    SetBlock(ref data, x, y, idct);
                }
        }

        private static double[,] GetBlock(double[,] src, int x, int y)
        {
            double[,] b = new double[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    int yy = y + i;
                    int xx = x + j;
                    b[i, j] = (yy < src.GetLength(0) && xx < src.GetLength(1)) ? src[yy, xx] : 0;
                }
            return b;
        }

        private static void SetBlock(ref double[,] dst, int x, int y, double[,] block)
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    int yy = y + i;
                    int xx = x + j;
                    if (yy < dst.GetLength(0) && xx < dst.GetLength(1))
                        dst[yy, xx] = block[i, j];
                }
        }

        private static double[,] DCT(double[,] f)
        {
            double[,] F = new double[8, 8];
            for (int u = 0; u < 8; u++)
                for (int v = 0; v < 8; v++)
                {
                    double sum = 0;
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                            sum += f[x, y] *
                                Math.Cos((2 * x + 1) * u * Math.PI / 16) *
                                Math.Cos((2 * y + 1) * v * Math.PI / 16);
                    double cu = (u == 0) ? 1 / Math.Sqrt(2) : 1;
                    double cv = (v == 0) ? 1 / Math.Sqrt(2) : 1;
                    F[u, v] = 0.25 * cu * cv * sum;
                }
            return F;
        }

        private static double[,] IDCT(double[,] F)
        {
            double[,] f = new double[8, 8];
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    double sum = 0;
                    for (int u = 0; u < 8; u++)
                        for (int v = 0; v < 8; v++)
                        {
                            double cu = (u == 0) ? 1 / Math.Sqrt(2) : 1;
                            double cv = (v == 0) ? 1 / Math.Sqrt(2) : 1;
                            sum += cu * cv * F[u, v] *
                                Math.Cos((2 * x + 1) * u * Math.PI / 16) *
                                Math.Cos((2 * y + 1) * v * Math.PI / 16);
                        }
                    f[x, y] = 0.25 * sum;
                }
            return f;
        }

        // Обновленный метод для получения квантовочной матрицы с учетом качества
        private static double[,] GetQuantMatrix(int quality)
        {
            double[,] baseQ = new double[8, 8]
            {
                {16,11,10,16,24,40,51,61},
                {12,12,14,19,26,58,60,55},
                {14,13,16,24,40,57,69,56},
                {14,17,22,29,51,87,80,62},
                {18,22,37,56,68,109,103,77},
                {24,35,55,64,81,104,113,92},
                {49,64,78,87,103,121,120,101},
                {72,92,95,98,112,100,103,99}
            };

            // Вычисляем масштаб сжатия в зависимости от качества
            double scale = quality < 50 ? 200 - 2 * quality : 5000.0 / quality;
            double[,] Q = new double[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    int q = (int)(baseQ[i, j] * scale / 100);
                    Q[i, j] = Math.Max(1, Math.Min(255, q));
                }

            return Q;
        }

        private static byte Clamp(double val) => (byte)Math.Min(255, Math.Max(0, val));
    }
}
