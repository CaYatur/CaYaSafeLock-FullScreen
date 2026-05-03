using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PCDisableCY
{
    public partial class USBblock : Form
    {
        static bool Closing = false;

        public static event EventHandler USBinserted;
        public static event EventHandler USBremoved;

        public USBblock()
        {
            InitializeComponent();
        }

        static bool USBIN = false;

        const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        const uint IOCTL_STORAGE_MEDIA_REMOVAL = 0x2D4804;
        const uint FSCTL_LOCK_VOLUME = 0x00090018;
        const uint FSCTL_DISMOUNT_VOLUME = 0x00090020;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        static List<int> initialProcessIds = new List<int>();

        private async void USBblock_Load(object sender, EventArgs e)
        {
            LockScreen.RequestEvent += Close;
            Closing = false;
            //this.Visible = false;
            Hide();
            //TopMost = true;
            //TopMost = true;
            //Focus();

            Thread bgThread = new Thread(() =>
            {
                if (Closing == false)
                {
                    // Enumerate connected USB flash drives at startup
                    Console.WriteLine("Currently connected USB flash drives:");
                    
                    // Başlangıçta çalışan işlemleri al
                    initialProcessIds = GetRunningProcessIds();

                    EnumerateConnectedUSBFlashDrives();

                    // USB cihazı takma ve çıkarma olaylarını dinlemek için WMI olayları kur
                    ManagementEventWatcher insertWatcher = new ManagementEventWatcher();
                    ManagementEventWatcher removeWatcher = new ManagementEventWatcher();

                    WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
                    WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");

                    insertWatcher.EventArrived += new EventArrivedEventHandler(UsbInsertedEvent);
                    removeWatcher.EventArrived += new EventArrivedEventHandler(UsbRemovedEvent);

                    insertWatcher.Query = insertQuery;
                    removeWatcher.Query = removeQuery;

                    insertWatcher.Start();
                    removeWatcher.Start();

                    Console.WriteLine("USB takma ve çıkarma olayları dinleniyor... Çıkmak için Ctrl+C'ye basın.");
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }




            });
            bgThread.IsBackground = true;
            bgThread.Start();


            Thread bgThread2 = new Thread(() =>
            {
                if (Closing == false)
                {
                    ManagementEventWatcher watcher = new ManagementEventWatcher();
                    WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");

                    watcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
                    watcher.Query = query;
                    watcher.Start();

                    Console.WriteLine("Listening for USB device insertions...");
                    Console.ReadLine();
                }



            });
            bgThread2.IsBackground = true;
            bgThread2.Start();

            //Thread bgThread3 = new Thread(() =>
            //{
            //    // Programın bulunduğu derleme bilgisini al
            //    Assembly assembly = Assembly.GetExecutingAssembly();

            //    // Derleme bilgisinden adı al
            //    string programName = assembly.GetName().Name;


            //    string targetProcessName = "CaYaDevTool.exe"; // Sizin programınızın adı

            //    // Süreç oluşturma ve kapatma olaylarını izlemek için WMI sorgusu
            //    string creationQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";
            //    string deletionQuery = "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process'";

            //    ManagementEventWatcher creationWatcher = new ManagementEventWatcher(new WqlEventQuery(creationQuery));
            //    ManagementEventWatcher deletionWatcher = new ManagementEventWatcher(new WqlEventQuery(deletionQuery));

            //    creationWatcher.EventArrived += new EventArrivedEventHandler(Watcher_EventArrived);
            //    deletionWatcher.EventArrived += (sender, e) =>
            //    {
            //        var process = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            //        string processName = process["Name"].ToString();

            //        if (processName.Equals(targetProcessName, StringComparison.OrdinalIgnoreCase))
            //        {
            //            Console.WriteLine($"Your program {targetProcessName} was closed!");

            //            // Kapatılma girişiminde bulunan işlemi bulma ve kapatma
            //            KillSuspiciousProcesses();
            //        }
            //    };

            //    creationWatcher.Start();
            //    deletionWatcher.Start();

            //});
            //bgThread3.IsBackground = true;
            //bgThread3.Start();
            //label1.Text = "Sistem yükleniyor lütfen bekleyiniz...";
            //await Task.Delay(1000);
            //this.Visible = false;
            //label1.Text = "Kilit ekranında USB takılması nedeniyle çoğu işlem devre dışı bırakılmıştır. Lütfen USB cihazınızı çıkarınız.";

        }

        static Form USBb;
        private void EnumerateConnectedUSBFlashDrives()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_DiskDrive WHERE MediaType='Removable Media' OR PNPDeviceID LIKE 'USBSTOR%'");
                foreach (ManagementObject diskDrive in searcher.Get())
                {
                    //Console.WriteLine("USB Flash Drive: " + diskDrive["DeviceID"]);
                    //Console.WriteLine("Model: " + diskDrive["Model"]);
                    //Console.WriteLine("PNPDeviceID: " + diskDrive["PNPDeviceID"]);
                    //Console.WriteLine();
                    //MessageBox.Show("USB Flash Drive: " + diskDrive["DeviceID"] + "Model: " + diskDrive["Model"] + "PNPDeviceID: " + diskDrive["PNPDeviceID"]);
                    try
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            //Environment.Exit(0);
                            Console.WriteLine("USB takıldı.");
                            

                            // Başlangıçta çalışan işlemleri al
                            //initialProcessIds = GetRunningProcessIds();

                            USBinserted?.Invoke(USBb, EventArgs.Empty);
                            //MessageBox.Show("");

                        }));
                        USBIN = true;
                        Closing = false;
                        MonitorNewProcesses();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        // Handle the exception, maybe log it or show a message to the user
                        Console.WriteLine($"ObjectDisposedException: {ex.Message}");
                    }
                    Console.WriteLine("USB takıldı.");


                }

            }
            catch (ManagementException e)
            {
                Console.WriteLine("An error occurred while querying for USB flash drives: " + e.Message);
            }
        }




        private void Close(object sender, EventArgs e)
        {
            // Form2'den gelen isteği işleyin.
            // Bu metot, Form2'de bir buton tıklandığında çağrılacak.
            //MessageBox.Show("Form2'den istek alındı ve işlem gerçekleştirildi.");
            //MessageBox.Show("A");
            Closing = true;
            Close();
            Dispose();
        }

        private static void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            var process = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string processName = process["Name"].ToString();
            Console.WriteLine($"Process created: {processName}");
        }

        private static void KillSuspiciousProcesses()
        {
            var allProcesses = Process.GetProcesses()
                                      .OrderByDescending(p => p.StartTime);

            foreach (var process in allProcesses)
            {
                if (process.ProcessName.Equals("yourprogram", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    process.Kill();
                    Console.WriteLine($"Suspicious process {process.ProcessName} (ID: {process.Id}) kapatıldı.");
                    break; // İlk başarılı kapatma işleminden sonra döngüyü sonlandır
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    Console.WriteLine($"Erişim engellendi: {process.ProcessName} (ID: {process.Id}), {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {process.ProcessName} (ID: {process.Id}), {ex.Message}");
                }
            }
        }


        private static void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB device inserted!");
            EjectAllUSBDrives();
        }

        private static void EjectAllUSBDrives()
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    try
                    {
                        USBRemoval.EjectDrive(drive.Name);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to eject drive {drive.Name}: {ex.Message}");
                    }
                }
            }
        }



        private static List<int> GetRunningProcessIds()
        {
            return Process.GetProcesses().Select(p => p.Id).ToList();
        }

        private void UsbInsertedEvent(object sender, EventArrivedEventArgs e)
        {

            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        // Your code here
            //        this.Visible = true;
            //    });
            //}
            if (Closing == false)
            {
                Console.WriteLine("USB takıldı.");
                USBIN = true;
                string driveLetter = GetDriveLetterFromEvent(e);
                if (driveLetter != null)
                {
                    LockVolume(driveLetter);
                    MonitorNewProcesses();
                }
                EjectAllUSBDrives();
            }


        }

        private void UsbRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB çıkarıldı. İzleme durduruldu.");
            USBremoved?.Invoke(this, EventArgs.Empty);
            USBIN = false;
            // USB çıkarıldığında yapılacak işlemler

            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        Close();
            //        // Your code here
            //        this.Visible = false;
            //    });
            //}
        }


        private static string GetDriveLetterFromEvent(EventArrivedEventArgs e)
        {
            // EventArrivedEventArgs nesnesinden sürücü harfini alın
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent;
            string driveLetter = instance["DriveName"] as string;
            return driveLetter;
        }

        private static void LockVolume(string driveLetter)
        {
            string volumePath = @"\\.\" + driveLetter.TrimEnd('\\') + ":";
            IntPtr hVolume = CreateFile(volumePath, 0xC0000000, 0x00000003, IntPtr.Zero, 3, 0, IntPtr.Zero);

            if (hVolume.ToInt32() == -1)
            {
                Console.WriteLine("Sürücü kilitlenemedi.");
                return;
            }

            uint bytesReturned;
            if (DeviceIoControl(hVolume, FSCTL_LOCK_VOLUME, IntPtr.Zero, 0, IntPtr.Zero, 0, out bytesReturned, IntPtr.Zero))
            {
                Console.WriteLine("Sürücü kilitlendi.");
            }
            else
            {
                Console.WriteLine("Sürücü kilitlenemedi.");
            }

            CloseHandle(hVolume);
        }

        private static void MonitorNewProcesses()
        {
            while (USBIN == true)
            {
                if (Closing == false)
                {
                    List<int> currentProcessIds = GetRunningProcessIds();
                    var newProcessIds = currentProcessIds.Except(initialProcessIds).ToList();

                    foreach (var processId in newProcessIds)
                    {
                        try
                        {
                            Process process = Process.GetProcessById(processId);
                            Console.WriteLine($"Yeni işlem bulundu: {process.ProcessName} (ID: {processId}). Kapatılıyor...");
                            process.Kill();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hata: {ex.Message}");
                        }
                    }

                    Thread.Sleep(1000);
                }
            }

        }

        private void USBblock_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing == false)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                //Close();
            }

        }
    }


    class USBRemoval
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        const uint GENERIC_WRITE = 0x40000000;
        const uint OPEN_EXISTING = 3;
        const uint IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;

        public static void EjectDrive(string driveLetter)
        {
            SafeFileHandle handle = CreateFile(
                @"\\.\" + driveLetter[0] + ":",
                GENERIC_WRITE,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                0,
                IntPtr.Zero);

            if (handle.IsInvalid)
            {
                Console.WriteLine($"Failed to open handle to drive {driveLetter}.");
                return;
            }

            uint bytesReturned = 0;
            bool result = DeviceIoControl(
                handle,
                IOCTL_STORAGE_EJECT_MEDIA,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                ref bytesReturned,
                IntPtr.Zero);

            if (!result)
            {
                Console.WriteLine($"Failed to eject drive {driveLetter}.");
            }
            else
            {
                Console.WriteLine($"Drive {driveLetter} ejected successfully.");
            }

            handle.Close();
        }
    }


}
