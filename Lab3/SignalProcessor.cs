using System;
using System.Linq;

namespace Lab3
{
    public class SignalProcessor
    {
        private int sampleRate;
        private double duration;
        private double frequency;
        private double noiseLevel;
        private double lossPercentage;
        private Random rnd = new Random();

        public double[] OriginalSignal { get; private set; }
        public double[] ProcessedSignal { get; private set; }
        public double[] FilteredSignal { get; private set; }
        public double[] Time { get; private set; }

        public SignalProcessor(int sampleRate, double duration, double frequency, double noiseLevel, double lossPercentage)
        {
            this.sampleRate = sampleRate;
            this.duration = duration;
            this.frequency = frequency;
            this.noiseLevel = noiseLevel;
            this.lossPercentage = lossPercentage;
            GenerateSignal();
        }

        private void GenerateSignal()
        {
            int sampleCount = (int)(sampleRate * duration);
            Time = new double[sampleCount];
            OriginalSignal = new double[sampleCount];

            // Генерация синусоиды
            for (int i = 0; i < sampleCount; i++)
            {
                Time[i] = i / (double)sampleRate;
                OriginalSignal[i] = Math.Sin(2 * Math.PI * frequency * Time[i]);
            }

            ProcessSignal();
            FilterSignal();
        }

        private void ProcessSignal()
        {
            ProcessedSignal = (double[])OriginalSignal.Clone();

            // Добавление шума каждые 10 точек
            for (int i = 9; i < ProcessedSignal.Length; i += 10)
            {
                ProcessedSignal[i] += (rnd.NextDouble() * 2 - 1) * noiseLevel;
            }

            // Удаление 10% случайных значений (замена на NaN)
            int lossCount = (int)(ProcessedSignal.Length * lossPercentage);
            var indices = Enumerable.Range(0, ProcessedSignal.Length)
                                    .OrderBy(x => rnd.Next())
                                    .Take(lossCount);
            foreach (var index in indices)
            {
                ProcessedSignal[index] = double.NaN;
            }
        }

        private void FilterSignal()
        {
            int n = ProcessedSignal.Length;
            double[] filled = new double[n];
            // Заполнение пропущенных значений: если значение NaN, заменяем предыдущим значением или 0, если это первый элемент
            for (int i = 0; i < n; i++)
            {
                if (double.IsNaN(ProcessedSignal[i]))
                    filled[i] = (i == 0) ? 0 : filled[i - 1];
                else
                    filled[i] = ProcessedSignal[i];
            }

            // Применяем простой скользящий средний фильтр с окном 5
            int window = 5;
            FilteredSignal = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                int count = 0;
                int start = Math.Max(0, i - window / 2);
                int end = Math.Min(n, i + window / 2 + 1);
                for (int j = start; j < end; j++)
                {
                    sum += filled[j];
                    count++;
                }
                FilteredSignal[i] = sum / count;
            }
        }
    }
}
