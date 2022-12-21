using System;
using System.Drawing;

namespace FloatingHorizon
{
    
    class Horizon
    {
        int imageWidth, imageHeight;
        double[] lowerHorizon, upperHorizon;
        public double xMin, xMax, yMin, yMax;
        public double xStep, yStep;
        double angleX, angleY, angleZ;
        Graphics graphics;
        Pen upLinePen, downLinePen;
        Point3D bodyCenter;
        public Func<double,double,double> func;

        public Horizon(int imageWidth, int imageHeight,Graphics graphics)
        {
            this.graphics = graphics;
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            lowerHorizon = new double[imageWidth];
            upperHorizon = new double[imageWidth];
            upLinePen = new Pen(Color.Red,1);
            downLinePen = new Pen( Color.Blue,1);
            angleX = 0.3;
            angleY = 0;
            angleZ = 0.6;
        }
        private void CalcBodyCenter()
        {
            bodyCenter.X = (xMax + xMin) / 2;
            bodyCenter.Y = (yMin + yMax) / 2;
            bodyCenter.Z = func(bodyCenter.X, bodyCenter.Y);
        }

        public void Draw()
        {
            CalcBodyCenter();
            ResetHorizons();
            graphics.Clear(Color.White);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            XLines();
            ResetHorizons();
            YLines();

        }
        private void XLines()
        {
            Point prevPoint;
            for (double currentX = xMax; currentX >= xMin; currentX -= xStep)
            {
                double z;
                z = func(currentX, yMin);
                prevPoint = TransformTo2D(currentX, yMin, z);
                Visibility prevVisibility = CheckVisibility(prevPoint);
                Point currentPoint = new Point();
                for (double currentY = yMin; currentY <= yMax; currentY += yStep)
                {
                    z = func(currentX, currentY);
                    currentPoint = TransformTo2D(currentX, currentY, z);
                    Visibility currentVisibility = CheckVisibility(currentPoint);
                    if (prevVisibility == currentVisibility)
                    {
                        if (currentVisibility == Visibility.VisibleAndLower)
                        {
                            graphics.DrawLine(downLinePen, prevPoint, currentPoint);
                        }
                        else if (currentVisibility == Visibility.VisibleAndUpper)
                        {
                            graphics.DrawLine(upLinePen, prevPoint, currentPoint);
                        }
                        UpdateHorizons(prevPoint, currentPoint);
                    }
                    prevVisibility = currentVisibility;
                    prevPoint = currentPoint;
                }
            }
        }

        private void YLines()
        {
            Point prevPoint;
            for (double currentY = yMax; currentY >= yMin; currentY -= yStep)
            {
                double z;
                z = func(xMin, currentY);
                prevPoint = TransformTo2D(xMin, currentY, z);
                Visibility prevVisibility = CheckVisibility(prevPoint);
                Point currentPoint = new Point();
                for (double currentX = xMin; currentX <= xMax; currentX += xStep)
                {
                    z = func(currentX, currentY);
                    currentPoint = TransformTo2D(currentX, currentY, z);
                    Visibility currentVisibility = CheckVisibility(currentPoint);
                    if (prevVisibility == currentVisibility)
                    {
                        if (currentVisibility == Visibility.VisibleAndLower)
                        {
                            graphics.DrawLine(downLinePen, prevPoint, currentPoint);
                        }
                        else if (currentVisibility == Visibility.VisibleAndUpper)
                        {
                            graphics.DrawLine(upLinePen, prevPoint, currentPoint);
                        }
                        UpdateHorizons(prevPoint, currentPoint);
                    }
                    prevVisibility = currentVisibility;
                    prevPoint = currentPoint;
                }
            }
        }

        private void ResetHorizons()
        {
            for (int i = 0; i < imageWidth; ++i)
            {
                upperHorizon[i] = imageHeight;
                lowerHorizon[i] = 0;
            }
        }
        private void SwapPoints<PointType>(ref PointType firstPoint, ref PointType secondPoint)
        {
            PointType tempPoint = firstPoint;
            firstPoint = secondPoint;
            secondPoint = tempPoint;
        }

        private Visibility CheckVisibility(Point currentPoint)
        {
            if (currentPoint.Y > upperHorizon[currentPoint.X] && currentPoint.Y < lowerHorizon[currentPoint.X]) return Visibility.Invisible;
            else
            {
                if (currentPoint.Y <= upperHorizon[currentPoint.X]) return Visibility.VisibleAndUpper;
                else return Visibility.VisibleAndLower;
            }
        }

        private void UpdateHorizons(Point firstPoint, Point secondPoint)
        {
            if (firstPoint.X == secondPoint.X)
            {
                lowerHorizon[secondPoint.X] = Math.Max(lowerHorizon[secondPoint.X], Math.Max(secondPoint.Y, firstPoint.Y));
                upperHorizon[secondPoint.X] = Math.Min(upperHorizon[secondPoint.X], Math.Min(secondPoint.Y, firstPoint.Y));
            }
            else
            {
                if (secondPoint.X <= firstPoint.X)
                    SwapPoints(ref firstPoint, ref secondPoint);
                double lineCoef = (secondPoint.Y - firstPoint.Y) / (secondPoint.X - firstPoint.X);
                double currentY;
                for (int currentX = firstPoint.X; currentX <= secondPoint.X; ++currentX)
                {
                    currentY = lineCoef * (currentX - firstPoint.X) + firstPoint.Y;
                    lowerHorizon[currentX] = Math.Max(lowerHorizon[currentX], currentY);
                    upperHorizon[currentX] = Math.Min(upperHorizon[currentX], currentY);
                }
            }
        }

        private Point TransformTo2D(double x, double y, double z)
        {
            double scaleRatio = Math.Min(imageWidth, imageHeight) / (Math.Max(xMax - xMin, yMax - yMin)); //scale
            x = bodyCenter.X + (x - bodyCenter.X) * scaleRatio;
            y = bodyCenter.Y + (y - bodyCenter.Y) * scaleRatio;
            z = -(bodyCenter.Z + (z - bodyCenter.Z) * scaleRatio);
            
            //rotating by z
            double temp = y * Math.Cos(angleZ) + x * Math.Sin(angleZ);
            x = -y * Math.Sin(angleZ) + x * Math.Cos(angleZ);
            y = temp;

            //rotating by x
            temp = z * Math.Cos(angleX) + y * Math.Sin(angleX);
            y = -z * Math.Sin(angleX) + y * Math.Cos(angleX);
            z = temp;

            //rotating by y
            temp = x * Math.Cos(angleY) + z * Math.Sin(angleY);
            z = -x * Math.Sin(angleY) + z * Math.Cos(angleY);
            x = temp;

            x += imageWidth / 2 - bodyCenter.X;
            z += imageHeight / 2 - bodyCenter.Z;
            return new Point((int)Math.Round(x), (int)Math.Round(z));
        }

        enum Visibility
        {
            VisibleAndUpper,
            VisibleAndLower,
            Invisible
        }

    }
    public struct Point3D
    {
        private double x, y, z;
        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
        public double Z { get => z; set => z = value; }
    }
}
