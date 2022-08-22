using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace BFVS
{
    class TemplateMatching
    {
        static double Threshold = 0.85;
        static Mat IconVehicleSelect = new Mat("BF_Small_Icon_Vehicle_0.png", ImreadModes.Grayscale);

        static double CalcMatList(List<Mat> mats, Mat template)
        {
            double avg = 0;
            foreach (var sample in mats)
            {
                var result = new Mat(sample.Rows - template.Rows + 1, sample.Cols - template.Cols + 1, MatType.CV_32FC1);
                Cv2.MatchTemplate(sample, template, result, TemplateMatchModes.CCoeffNormed);
                //Cv2.ImShow("Heat Map", result);

                double minValue;
                double maxValue;
                Point minLocation;
                Point maxLocation;
                Cv2.MinMaxLoc(result, out minValue, out maxValue, out minLocation, out maxLocation);
                avg += maxValue;
                Console.WriteLine(maxValue);

                Rect rectangle = new Rect(new Point(maxLocation.X, maxLocation.Y), new Size(template.Width, template.Height));
                Cv2.Rectangle(sample, rectangle, Scalar.LimeGreen, 3);

                Cv2.ImShow(maxValue.ToString(), sample);
                Cv2.WaitKey();
                Cv2.DestroyAllWindows();
            }

            return avg / mats.Count;
        }

        public static void MatchTestSamples()
        {
            string[] filesLoadingStages = Directory.GetFiles(@"D:\Trashland\BF2042 OpenCV Samples\Loading Stages");
            string[] filesMapPreload = Directory.GetFiles(@"D:\Trashland\BF2042 OpenCV Samples\Map Preload");
            string[] filesVehicleIcons = Directory.GetFiles(@"D:\Trashland\BF2042 OpenCV Samples\Vehicle Icons");
            string[] filesVehicleSelect = Directory.GetFiles(@"D:\Trashland\BF2042 OpenCV Samples\Vehicle Select");

            List<Mat> matsLoadingStages = new List<Mat>();
            List<Mat> matsMapPreload = new List<Mat>();
            List<Mat> matsVehicleIcons = new List<Mat>();
            List<Mat> matsVehicleSelect = new List<Mat>();

            filesLoadingStages.ToList().ForEach(file => matsLoadingStages.Add(new Mat(file, ImreadModes.Grayscale)));
            filesMapPreload.ToList().ForEach(file => matsMapPreload.Add(new Mat(file, ImreadModes.Grayscale)));
            filesVehicleIcons.ToList().ForEach(file => matsVehicleIcons.Add(new Mat(file, ImreadModes.Grayscale)));
            filesVehicleSelect.ToList().ForEach(file => matsVehicleSelect.Add(new Mat(file, ImreadModes.Grayscale)));

            var iconVehicleSelect = new Mat(@"D:\Trashland\BF2042 OpenCV Samples\BF_Small_Icon_Vehicle_0.png", ImreadModes.Grayscale);

            Console.WriteLine("=== Loading Stages ===");
            Console.WriteLine("Average: " + CalcMatList(matsLoadingStages, iconVehicleSelect));

            Console.WriteLine("=== Map Preload ===");
            Console.WriteLine("Average: " + CalcMatList(matsMapPreload, iconVehicleSelect));

            Console.WriteLine("=== Vehicle Icons ===");
            Console.WriteLine("Average: " + CalcMatList(matsVehicleIcons, iconVehicleSelect));

            Console.WriteLine("=== Vehicle Select ===");
            Console.WriteLine("Average: " + CalcMatList(matsVehicleSelect, iconVehicleSelect));
        }

        public static Point? MatchFrame(Mat frame)
        {
            var result = new Mat(frame.Rows - IconVehicleSelect.Rows + 1, frame.Cols - IconVehicleSelect.Cols + 1, MatType.CV_32FC1);
            Cv2.MatchTemplate(frame, IconVehicleSelect, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);

            if (maxValue >= Threshold)
                return maxLocation;

            return null;
        }

        public static System.Drawing.Point? IconDetectionLoop()
        {
            int duration = 60000;
            bool detecting = true;

            System.Drawing.Point? pos = null;

            Task task = new Task(() => {
                while (detecting)
                {
                    (var frame, _, _) = LatestFrame.GetLatestFrameAsMat();
                    if (frame == null)
                        continue;

                    Mat grayscaleFrame = frame.CvtColor(ColorConversionCodes.BGRA2GRAY);
                    frame.Dispose();
                    Point? maxLocation = MatchFrame(grayscaleFrame);
                    grayscaleFrame.Dispose();

                    if (maxLocation != null)
                    {
                        pos = new System.Drawing.Point(maxLocation.Value.X, maxLocation.Value.Y);
                        break;
                    }
                }
            });

            task.Start();
            task.Wait(duration); // timeout in case of problems
            detecting = false;

            return pos;
        }

        public static Mat GetMatScreenshot(IntPtr handle)
        {
            var screenshot = WindowsManager.GetScreenshotGDI(handle);
            return BitmapConverter.ToMat(screenshot);
        }

        public static double ScreenshotCaptureSpeedTest(IntPtr handle)
        {
            // GDI - ~40 FPS
            // ScreenCapture - 144 FPS

            double duration = 60000;
            int calls = 0;
            bool testing = true;

            Task task = new Task(() => {
                while (testing)
                {
                    //var screenshot = GetMatScreenshot(handle); // GDI
                    (var screenshot, _, _) = LatestFrame.GetLatestFrameAsMat(); // ScreenCapture
                    if (screenshot == null)
                        continue;

                    //OpenCvSharp.Cv2.ImShow("Latest Frame", screenshot);
                    //OpenCvSharp.Cv2.WaitKey();

                    _ = screenshot.DataStart;
                    screenshot.Dispose();
                    calls++;
                }
            });

            task.Start();
            task.Wait(Convert.ToInt32(duration));
            testing = false;

            return calls / duration * 1000;
        }
    }
}
