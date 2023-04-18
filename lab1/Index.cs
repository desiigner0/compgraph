using System;
using System.Drawing;

namespace lab1
{
    class Translation : Filters // перенос
    {
        private int _deltaX; // координаты смещения
        private int _deltaY; 
        public Translation(int deltaX, int deltaY)
        {
            _deltaX = deltaX;
            _deltaY = deltaY;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int newPosX = x + _deltaX;
            int newPosY = y + _deltaY;
            // проверяем, что новые координаты пикселя в пределах изображения
            if (newPosX < 0 || newPosX >= sourceImage.Width || newPosY < 0 || newPosY >= sourceImage.Height)
            {
                return Color.Black; // если за пределами, то возвращаем черный цвет
            }
            else
            {
                return sourceImage.GetPixel(newPosX, newPosY);
            }
        }
    } // перенос
    class Turn: Filters // поворот
    {
        private float _degree; // угол поворота
        // координаты центра поворота
        private int _X;
        private int _Y;
        public Turn(int deltaX, int deltaY, float degree)
        {
            _X = deltaX;
            _Y = deltaY;
            _degree = degree;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            float angleRad = (float)(_degree * Math.PI / 180);
            float newPosX = (x - _X) * (float)Math.Cos(angleRad) - (y - _Y) * (float)Math.Sin(angleRad) + _X;
            float newPosY = (x - _X) * (float)Math.Sin(angleRad) + (y - _Y) * (float)Math.Cos(angleRad) + _Y;
            // проверяем, что новые координаты пикселя в пределах изображения
            if (newPosX < 0 || newPosX >= sourceImage.Width || newPosY < 0 || newPosY >= sourceImage.Height)
            {
                return Color.Black; // если за пределами, то возвращаем черный цвет
            }
            else
            {
                return sourceImage.GetPixel((int)newPosX, (int)newPosY);
            }
        }
    } // поворот
}
