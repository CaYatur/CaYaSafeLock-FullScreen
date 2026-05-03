using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CYStartupControl
{
    internal static class Program
    {
        // Kullanýlacak olan DPI Awareness API'leri
        [DllImport("user32.dll")]
        private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

        // Sabit DPI için kullanýlan deðer
        private const int DPI_AWARENESS_CONTEXT_UNAWARE = -1;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            // Uygulamanýn her zaman 100% DPI ile açýlmasýný saðla (96 DPI)
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_UNAWARE);
            // Uygulama yapýlandýrmasýný baþlat
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            //Application.Run(new DisableSafeMode());
            //pplication.Run(new StartupScreenBlock());
        }
    }
}