﻿using System.ComponentModel;
using System.Drawing;
using System;

namespace lab1
{
    class Erosion : MatrixFilters // сужение
    {
        public Erosion()
        {
            // kernel = new float[,] { { 1, 1, 1,1,1 }, { 1, 1, 1,1,1 }, { 1, 1, 1,1,1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 } };
            kernel = new float[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public Color Erode(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor(sourceImage, x, y);
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            // устанавливаем начальное значение минимальной яркости
            int minIntensity = 255;

            // проходим по всем элементам структурного элемента
            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    // вычисляем координаты соседнего пикселя
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    // получаем цвет соседнего пикселя
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    // находим минимальную яркость среди всех соседних пикселей и значения в структурном элементе
                    int intensity = (int)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);
                    if (kernel[j + radiusX, i + radiusY] > 0 && intensity < minIntensity)
                    {
                        minIntensity = intensity;
                    }
                }
            }
            return Color.FromArgb(minIntensity, minIntensity, minIntensity);
        }
    } // сужение
    class Dilation : MatrixFilters // расширение
    {
        public Dilation()
        {
            // kernel = new float[,] { { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 } };
            kernel = new float[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public Color Dilate(Bitmap sourceImage, int x, int y)
        {
            return calculateNewPixelColor(sourceImage, x, y);
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            // устанавливаем начальное значение максимальной яркости
            int maxIntensity = 0;

            // проходим по всем элементам структурного элемента
            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    // вычисляем координаты соседнего пикселя
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);

                    // получаем цвет соседнего пикселя
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    // находим максимальную яркость среди всех соседних пикселей и значения в структурном элементе
                    int intensity = (int)(0.299 * neighborColor.R + 0.587 * neighborColor.G + 0.114 * neighborColor.B);
                    if (kernel[j + radiusX, i + radiusY] > 0 && intensity > maxIntensity)
                    {
                        maxIntensity = intensity;
                    }
                }
            }
            return Color.FromArgb(maxIntensity, maxIntensity, maxIntensity);
        }
    } // расширение
    class Opening : MatrixFilters // открытие
    {
        private Erosion erosionFilter;
        private Dilation dilationFilter;
        public Opening()
        {
            erosionFilter = new Erosion();
            dilationFilter = new Dilation();
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap erodedImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            // применяем сужение к исходному изображению
            for (int y = 0; y < sourceImage.Height; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Height * 100));
                if (worker.CancellationPending)
                    return null;

                for (int x = 0; x < sourceImage.Width; x++)
                {
                    erodedImage.SetPixel(x, y, erosionFilter.Erode(sourceImage, x, y));
                }
            }

            // применяем расширение к изображению
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int y = 0; y < sourceImage.Height; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Height * 100));
                if (worker.CancellationPending)
                    return null;

                for (int x = 0; x < sourceImage.Width; x++)
                {
                    resultImage.SetPixel(x, y, dilationFilter.Dilate(erodedImage, x, y));
                }
            }
            return resultImage;
        }
    } // открытие
    class Closing : MatrixFilters // закрытие
    {
        private Erosion erosionFilter;
        private Dilation dilationFilter;
        public Closing()
        {
            erosionFilter = new Erosion();
            dilationFilter = new Dilation();
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap dilatedImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            // применяем сужение к исходному изображению
            for (int y = 0; y < sourceImage.Height; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Height * 100));
                if (worker.CancellationPending)
                    return null;
                for (int x = 0; x < sourceImage.Width; x++)
                {
                    dilatedImage.SetPixel(x, y, dilationFilter.Dilate(sourceImage, x, y));
                }
            }

            // применяем расширение к изображению
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int y = 0; y < sourceImage.Height; y++)
            {
                worker.ReportProgress((int)((float)y / sourceImage.Height * 100));
                if (worker.CancellationPending)
                    return null;

                for (int x = 0; x < sourceImage.Width; x++)
                {
                    resultImage.SetPixel(x, y, erosionFilter.Erode(dilatedImage, x, y));
                }
            }
            return resultImage;
        }
    } // закрытие
    class Gradient: MatrixFilters
    {
        private Erosion erosionFilter;
        private Dilation dilationFilter;
        public Gradient()
        {
            erosionFilter = new Erosion();
            dilationFilter = new Dilation();
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color erosionColor = erosionFilter.Erode(sourceImage, x, y);
            Color dilationColor = dilationFilter.Dilate(sourceImage, x, y);
            int r = Clamp(dilationColor.R - erosionColor.R, 0, 255);
            int g = Clamp(dilationColor.G - erosionColor.G , 0, 255);
            int b = Clamp(dilationColor.B - erosionColor.B, 0, 255);
            return Color.FromArgb(r, g, b);
        }
    } // градиент
    class TopHat : MatrixFilters
    {
        public TopHat()
        {

        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            Bitmap openedImage = new Opening().processImage(sourceImage, worker);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    Color openedColor = openedImage.GetPixel(i, j);
                    int r = Clamp(sourceColor.R - openedColor.R, 0, 255);
                    int g = Clamp(sourceColor.G - openedColor.G, 0, 255);
                    int b = Clamp(sourceColor.B - openedColor.B, 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
            return resultImage;
        }
    } // tophat
    class BlackHat: MatrixFilters
    {
        public BlackHat()
        {

        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Bitmap closedImage = new Closing().processImage(sourceImage, worker);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    Color sourceColor = sourceImage.GetPixel(i, j);
                    Color closedColor = closedImage.GetPixel(i, j);
                    int r = Clamp(sourceColor.R - closedColor.R, 0, 255);
                    int g = Clamp(sourceColor.G - closedColor.G, 0, 255);
                    int b = Clamp(sourceColor.B - closedColor.B, 0, 255);
                    resultImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }
            return resultImage;
        }
    } // blackhat
}
