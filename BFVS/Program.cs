using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFVS
{
    class Program
    {
        static readonly int CursorOffset = 2;
        static readonly int ClickHoldTime = 8;
        static readonly int ClickingInterval = 5;
        static Win32.VirtualKeys DefaultKey = Win32.VirtualKeys.Numpad0;
        static Win32.VirtualKeys MBTDefaultKey = Win32.VirtualKeys.Numpad1;
        static Win32.VirtualKeys IFVDefaultKey = Win32.VirtualKeys.Numpad2;

        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            ScreenCapture.IsAvailable();

            //Console.WriteLine("Preparing to benchmark...");
            //Process[] ps = Process.GetProcesses();
            //Process bf = ps.First(p => p.ProcessName.ToLower().StartsWith("bf") && p.MainWindowTitle.StartsWith("Battlefield"));
            //Console.WriteLine("Starting in 3 seconds...");
            //Thread.Sleep(3000);
            //Console.WriteLine("Starting");
            ////CaptureHandler.StartWindowCapture(bf.Handle);
            //CaptureHandler.StartPrimaryMonitorCapture();
            //double framesPerSecond = TemplateMatching.ScreenshotCaptureSpeedTest(bf.MainWindowHandle);
            //Console.WriteLine("Done: " + framesPerSecond);
            //CaptureHandler.Stop();

            //Console.ReadLine();
            //return;
            Console.WriteLine("### Battlefield Vehicle Stealer ver. {0} ###", Assembly.GetEntryAssembly().GetName().Version.ToString());

            Console.WriteLine("Parsing key...");
            Win32.VirtualKeys key = DefaultKey;

            Console.WriteLine("Getting process");
            Process[] processes = Process.GetProcesses();
            Process process = processes.First(p => p.ProcessName.ToLower().StartsWith("bf") && p.MainWindowTitle.StartsWith("Battlefield"));
            if (process == null)
            {
                Console.WriteLine("No Battlefield process found!");
                return;
            }
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExitedCallback;

            Console.WriteLine("Getting handle");
            IntPtr handle = process.MainWindowHandle;
            Console.WriteLine("Process handle: {0}", handle.ToString());

            Console.WriteLine("Clicking loop...");
            bool isClicking = false;
            System.Drawing.Point mouse = System.Drawing.Point.Empty;
            while (true)
            {
                if (!WindowsManager.IsWindowInFocus(handle))
                    continue;

                bool parsingMBT = WindowsManager.IsKeyPressed(MBTDefaultKey);
                bool parsingIFV = WindowsManager.IsKeyPressed(IFVDefaultKey);
                if (parsingMBT || parsingIFV)
                {
                    Console.WriteLine("ScreenCapture analyze started");
                    CaptureHandler.StartPrimaryMonitorCapture();

                    Point? pos = TemplateMatching.IconDetectionLoop();
                    CaptureHandler.Stop();
                    Console.WriteLine("ScreenCapture analyze done");

                    if (pos == null) // weird
                        continue;
                    Clicker.ClickAtLocation(pos.Value, true);

                    Thread.Sleep(65); // animation delay

                    if (parsingMBT)
                    {
                        Clicker.ClickAtLocation(Clicker.MBTPoint, true);
                        Clicker.ClickAtLocation(Clicker.MBTPoint, true);
                        Clicker.ClickAtLocation(Clicker.MBTPoint, true);
                    }
                    else if (parsingIFV)
                    {
                        Clicker.ClickAtLocation(Clicker.IFVPoint, true);
                        Clicker.ClickAtLocation(Clicker.IFVPoint, true);
                        Clicker.ClickAtLocation(Clicker.IFVPoint, true);
                    }
                    
                    Console.WriteLine("Chosen vehicle selected");
                }

                if (WindowsManager.IsKeyPressed(DefaultKey))
                {
                    isClicking = !isClicking;
                    if (isClicking)
                    {
                        Console.WriteLine("Clicking enabled");
                        mouse = WindowsManager.GetMousePosition();
                    }
                    else
                        Console.WriteLine("Clicking disabled");
                }

                if (isClicking)
                {
                    Point pos = Clicker.ShakeCursorAtLocation(mouse);
                    Clicker.ClickAtLocation(pos, true);
                }
            }
        }

        private static void ProcessExitedCallback(object sender, EventArgs e)
        {
            Console.WriteLine("Target process exited");
            Environment.Exit(0);
        }
    }
}