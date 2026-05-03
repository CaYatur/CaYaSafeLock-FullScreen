using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PCDisableCY
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
            // Uygulamanýn her zaman 100% DPI ile açýlmasýný saðla (96 DPI)
            SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_UNAWARE);

            // Uygulama yapýlandýrmasýný baþlat
            ApplicationConfiguration.Initialize();

            // Ana formu çalýþtýr
            Application.Run(new Main());

            //Application.Run(new VoteSystem());

            // Diðer form seçenekleri (yorum satýrýndan çýkararak kullanabilirsin)
            // Application.Run(new CheckUninstaller());
            // Application.Run(new LockScreen());
            // Application.Run(new USBblock());
        }
    }
}
