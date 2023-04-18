using System;
using System.ComponentModel;
using System.Drawing;

namespace lab1
{
    class MatrixFilters : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilters() { }
        public MatrixFilters(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int i = -radiusY; i <= radiusY; i++)
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[j + radiusX, i + radiusY];
                    resultG += neighborColor.G * kernel[j + radiusX, i + radiusY];
                    resultB += neighborColor.B * kernel[j + radiusX, i + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultG, 0, 255));
        }
    } 
    class BlurFilter : MatrixFilters // размытие
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    } // размытие
    class GaussianFilter : MatrixFilters // гаусс
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1; // размер ядра
            kernel = new float[size, size]; // ядро фильтра
            float norm = 0; // коэффициент нормировки ядра
            for (int i = -radius; i <= radius; i++) // рассчитываем ядро линейного фильтра
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++) // нормируем ядро
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(5, 1.4f);
        }
    } // гаусс
    class SobelFilter : MatrixFilters // собель
    {
        public SobelFilter()
        {
            kernel = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultXr = 0, resultXg = 0, resultXb = 0, resultYr = 0, resultYg = 0, resultYb = 0;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    float intensityR1 = neighborColor.R;
                    float intensityG1 = neighborColor.G;
                    float intensityB1 = neighborColor.B;

                    resultXr += intensityR1 * kernel[j + radiusX, i + radiusY] * ((j == 0) ? 2 : 1);
                    resultYr += intensityR1 * kernel[i + radiusY, j + radiusX] * ((i == 0) ? 2 : 1);

                    resultXg += intensityG1 * kernel[j + radiusX, i + radiusY] * ((j == 0) ? 2 : 1);
                    resultYg += intensityG1 * kernel[i + radiusY, j + radiusX] * ((i == 0) ? 2 : 1);

                    resultXb += intensityB1 * kernel[j + radiusX, i + radiusY] * ((j == 0) ? 2 : 1);
                    resultYb += intensityB1 * kernel[i + radiusY, j + radiusX] * ((i == 0) ? 2 : 1);
                }
            }

            float intensityXr = Clamp((int)resultXr, 0, 255);
            float intensityXg = Clamp((int)resultXg, 0, 255);
            float intensityXb = Clamp((int)resultXb, 0, 255);

            float intensityYr = Clamp((int)resultYr, 0, 255);
            float intensityYg = Clamp((int)resultYg, 0, 255);
            float intensityYb = Clamp((int)resultYb, 0, 255);

            float intensityR = Clamp((int)Math.Sqrt(intensityXr * intensityXr + intensityYr * intensityYr), 0, 255);
            float intensityG = Clamp((int)Math.Sqrt(intensityXg * intensityXg + intensityYg * intensityYg), 0, 255);
            float intensityB = Clamp((int)Math.Sqrt(intensityXb * intensityXb + intensityYb * intensityYb), 0, 255);

            Color resultColor = Color.FromArgb(
                Clamp((int)intensityR, 0, 255), // red channel
                Clamp((int)intensityG, 0, 255), // green channel
                Clamp((int)intensityB, 0, 255));  // blue channel

            return resultColor;
        }
    } // собель
    class Sharpness : MatrixFilters // резкость
    {
        public Sharpness()
        {
            kernel = new float[3, 3] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    } // резкость
    class Embossing : MatrixFilters // тиснение
    {
        public Embossing()
        {
            kernel = new float[3, 3] { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int i = -radiusY; i <= radiusY; i++)
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[j + radiusX, i + radiusY];
                    resultG += neighborColor.G * kernel[j + radiusX, i + radiusY];
                    resultB += neighborColor.B * kernel[j + radiusX, i + radiusY];
                }
            return Color.FromArgb(Clamp((int)resultR + 128, 0, 255), Clamp((int)resultG + 128, 0, 255), Clamp((int)resultG + 128, 0, 255));
        }
    } // тиснение
    class SharrFilter : MatrixFilters // щарр
    {
        public SharrFilter()
        {
            kernel = new float[,] { { 3, 10, 3 }, { 0, 0, 0 }, { -3, -10, -3 } };
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultX = 0, resultY = 0;

            for (int i = -radiusY; i <= radiusY; i++)
            {
                for (int j = -radiusX; j <= radiusX; j++)
                {
                    int idX = Clamp(x + j, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + i, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    float intensityS = neighborColor.R * 0.299f + neighborColor.G * 0.587f + neighborColor.B * 0.114f;

                    resultX += intensityS * kernel[j + radiusX, i + radiusY] * ((j == 0) ? 2 : 1);
                    resultY += intensityS * kernel[i + radiusY, j + radiusX] * ((i == 0) ? 2 : 1);
                }
            }

            float intensityX = Clamp((int)resultX, 0, 255);
            float intensityY = Clamp((int)resultY, 0, 255);
            float intensity = Clamp((int)Math.Sqrt(intensityX * intensityX + intensityY * intensityY), 0, 255);

            return Color.FromArgb((int)intensity, (int)intensity, (int)intensity);
        }
    } // щарр
    class GlowingEdges : MatrixFilters
    {
        MedianFilter medianFilter = new MedianFilter(3);
        SobelFilter sobelFilter = new SobelFilter();
        MaxFilter maxFilter = new MaxFilter(3);
        public GlowingEdges()
        {
            medianFilter = new MedianFilter(6);
            sobelFilter = new SobelFilter();
            maxFilter = new MaxFilter(3);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap medianImage = medianFilter.processImage(sourceImage, worker);
            Bitmap sobelImage = sobelFilter.processImage(medianImage, worker);
            Bitmap resultImage = maxFilter.processImage(sobelImage, worker);
            return resultImage;
        }
    } // светящиеся края
}
