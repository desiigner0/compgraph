using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;
using System;
using System.Linq;

namespace lab1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for(int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for(int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    }
    class InvertFilter: Filters // инверсия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    } // инверсия
    class GrayScaleFilter: Filters // серый цвет
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double Intensity = (299 * sourceColor.R)/1000 + (587 * sourceColor.G) / 1000 + (114 * sourceColor.B) / 1000;
            Color resultColor = Color.FromArgb((int)(Intensity), (int)(Intensity), (int)(Intensity));
            return resultColor;
        }
    } // серый цвет
    class SepiaFilter: Filters // сепия
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            double k = 10;
            double Intensity = (299 * sourceColor.R) / 1000 + (587 * sourceColor.G) / 1000 + (114 * sourceColor.B) / 1000;
            Color resultColor = Color.FromArgb(Clamp((int)(Intensity + 2 * k), 0, 255), Clamp((int)(Intensity + k/2), 0, 255), Clamp((int)(Intensity - 1 * k), 0, 255));
            return resultColor;
        }
    } // сепия
    class Brightness: Filters // яркость
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 50;
            Color resultColor = Color.FromArgb(Clamp(sourceColor.R + k, 0, 255), Clamp(sourceColor.G + k, 0, 255), Clamp(sourceColor.B + k, 0, 255));
            return resultColor;
        }
    } // яркость
    class HistogramStretchFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
            return Color.FromArgb(
                Clamp((int)((intensity - minIntensity) * (255.0 / (maxIntensity - minIntensity))), 0, 255),
                Clamp((int)((intensity - minIntensity) * (255.0 / (maxIntensity - minIntensity))), 0, 255),
                Clamp((int)((intensity - minIntensity) * (255.0 / (maxIntensity - minIntensity))), 0, 255));
        }
        private int minIntensity = 255, maxIntensity = 0;
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            // Первый проход для нахождения минимального и максимального значения интенсивности
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    int intensity = (int)(0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B);
                    minIntensity = Math.Min(minIntensity, intensity);
                    maxIntensity = Math.Max(maxIntensity, intensity);
                }
            }
            // Второй проход для применения растяжения
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
    } // линейное растяжение
    class MedianFilter : Filters
    {
        int size; // размер окна
        int radius; // радиус окна

        public MedianFilter(int size)
        {
            this.size = size;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            radius = size / 2;
            // массив цветов в окрестности пикселя
            List<int> reds = new List<int>();
            List<int> greens = new List<int>();
            List<int> blues = new List<int>();
            // проходим вокруг пикселя
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    // Добавляем цвет в список
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    reds.Add(neighborColor.R);
                    greens.Add(neighborColor.G);
                    blues.Add(neighborColor.B);
                }
            }
            // Сортируем список цветов
            reds.Sort();
            greens.Sort();
            blues.Sort();
            // Вычисляем медиану
            int medianIndex = reds.Count / 2;
            int redMedian = reds[medianIndex];
            int greenMedian = greens[medianIndex];
            int blueMedian = blues[medianIndex];
            // Возвращаем новый цвет пикселя
            return Color.FromArgb(redMedian, greenMedian, blueMedian);
        }
    }  // медианный фильтр
    class MaxFilter: Filters
    {
        int size; // размер окна
        int radius; // радиус окна
        public MaxFilter(int size)
        {
            this.size = size;
            this.radius = size / 2;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            // Массив максимальных значений по каждому каналу
            int[] maxValues = new int[3];
            // Проходим по окну вокруг пикселя
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    // Получаем координаты соседнего пикселя
                    int xn = Clamp(x + j, 0, sourceImage.Width - 1);
                    int yn = Clamp(y + i, 0, sourceImage.Height - 1);
                    // Получаем цвет соседнего пикселя
                    Color color = sourceImage.GetPixel(xn, yn);
                    // Обновляем максимальные значения по каждому каналу
                    if (color.R > maxValues[0])
                        maxValues[0] = color.R;
                    if (color.G > maxValues[1])
                        maxValues[1] = color.G;
                    if (color.B > maxValues[2])
                        maxValues[2] = color.B;
                }
            }
            // Возвращаем новый цвет пикселя
            return Color.FromArgb(maxValues[0], maxValues[1], maxValues[2]);
        }
    } // фильтр максимумов
    class GrayWorldFilter: Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float avgR = 0, avgG = 0, avgB = 0;
            int pixelsCount = 0;
            // Вычисление средних значений каналов RGB
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    Color pixelColor = sourceImage.GetPixel(idX, idY);
                    avgR += pixelColor.R;
                    avgG += pixelColor.G;
                    avgB += pixelColor.B;
                    pixelsCount++;
                }
            }
            // подсчет средних
            avgR /= pixelsCount;
            avgG /= pixelsCount;
            avgB /= pixelsCount;
            float avgGray = (avgR + avgG + avgB) / 3f;
            float kR = avgGray / avgR;
            float kG = avgGray / avgG;
            float kB = avgGray / avgB;
            int newR = Clamp((int)(sourceImage.GetPixel(x, y).R * kR), 0, 255);
            int newG = Clamp((int)(sourceImage.GetPixel(x, y).G * kG), 0, 255);
            int newB = Clamp((int)(sourceImage.GetPixel(x, y).B * kB), 0, 255);
            Color resultColor = Color.FromArgb(newR, newG, newB);
            return resultColor;
        }
    } // фильтр серый мир
}
