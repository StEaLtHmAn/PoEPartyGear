using Emgu.CV;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Tesseract;

public class ImageProcessing
{
    public static Point LocateImageSingle(Bitmap bmpSource, Bitmap bmpImage, double threshold = 0.85)
    {
        Image<Bgr, byte> source = bmpSource.ToImage<Bgr, byte>();
        Image<Bgr, byte> template = bmpImage.ToImage<Bgr, byte>();

        Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
        source.Dispose();
        template.Dispose();

        double[] minValues, maxValues;
        Point[] minLocations, maxLocations;
        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
        result.Dispose();

        if (maxValues[0] > threshold)
        {
            return maxLocations[0];
        }
        return Point.Empty;
    }

    public static Point[] LocateImageMulti(Bitmap bmpSource, Bitmap bmpImage, double threshold = 0.85)
    {
        Image<Bgr, byte> source = bmpSource.ToImage<Bgr, byte>();
        Image<Bgr, byte> template = bmpImage.ToImage<Bgr, byte>();

        Image<Gray, float> result = source.MatchTemplate(template, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);
        source.Dispose();
        template.Dispose();

        List<Point> pointList = new List<Point>();
        for (int y = 0; y < result.Data.GetLength(0); y++)
        {
            for (int x = 0; x < result.Data.GetLength(1); x++)
            {
                if (result.Data[y, x, 0] >= threshold) //Check if its a valid match
                {
                    Point tmpPoint = new Point(x, y);
                    if (!pointList.Contains(tmpPoint))//make sure we dont get duplicates, might have to improve this to skip nearby points as well
                        pointList.Add(tmpPoint);
                }
            }
        }
        result.Dispose();

        return pointList.ToArray();
    }

    public static Bitmap ScreenshotWindow(int handle)
    {
        RECT rc;
        Win32.GetWindowRect(handle, out rc); //gets dimensions of the window
        if (rc.Width + rc.Height != 0)
        {
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(new Point(rc.Left, rc.Top), Point.Empty, rc.Size);
            }
            return bmp;
        }
        throw new Exception("NoWindowAvailable");
    }

    public static string ReadTextInRect(int handle, Rectangle section, string whiteList = "", int ImageValueThresholdMin = 178, int ImageValueThresholdMax = 255)
    {
        using (var engine = new TesseractEngine("tessdata", "eng"))
        {
            if (!string.IsNullOrEmpty(whiteList))
                engine.SetVariable("tessedit_char_whitelist", whiteList);

            using (Bitmap CurrentView = ScreenshotWindow(handle))
            using (Bitmap croppedCurrentView = cropAtRect(CurrentView, section))
            {
                using (Image<Gray, byte> imageHSVDest = croppedCurrentView.ToImage<Hsv, byte>().Resize(2.5, Emgu.CV.CvEnum.Inter.Cubic).InRange(new Hsv(0, 0, ImageValueThresholdMin), new Hsv(255, 255, ImageValueThresholdMax)).SmoothBlur(3, 3).Not())
                using (Pix img = PixConverter.ToPix(imageHSVDest.ToBitmap()))
                {
                    //string fileName = DateTime.Now.Ticks + ".png";
                    //while (File.Exists(fileName))
                    //{
                    //    Thread.Sleep(1);
                    //    fileName = DateTime.Now.Ticks + ".png";
                    //}
                    //img.Save(fileName);

                    using (Page page = engine.Process(img))
                    {
                        return page.GetText().Trim();
                    }
                }
            }
        }
    }

    public static Bitmap cropAtRect(Bitmap b, Rectangle r)
    {
        Bitmap nb = new Bitmap(r.Width, r.Height);
        using (Graphics g = Graphics.FromImage(nb))
        {
            g.DrawImage(b, -r.X, -r.Y);
            return nb;
        }
    }
}