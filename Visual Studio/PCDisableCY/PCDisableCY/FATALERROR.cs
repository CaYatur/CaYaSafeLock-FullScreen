using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCDisableCY
{
    public partial class FATALERROR : Form
    {
        private bool Closing = false;
        public FATALERROR()
        {
            InitializeComponent();
        }

        private void FATALERROR_Load(object sender, EventArgs e)
        {
            LockScreen.RequestEvent += Close;

            Thread bgThread = new Thread(() =>
            {

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


            });
            bgThread.IsBackground = true;
            bgThread.Start();
        }




        private void UsbInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB takıldı.");

        }



        private void UsbRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB çıkarıldı.");
            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    Closing = true;
                    this.Visible = false;
                    Hide();
                    GC.Collect();
                    Close();
                });
            }
        }





        private void Close(object sender, EventArgs e)
        {
            // Form2'den gelen isteği işleyin.
            // Bu metot, Form2'de bir buton tıklandığında çağrılacak.
            //MessageBox.Show("Form2'den istek alındı ve işlem gerçekleştirildi.");
            //MessageBox.Show("A");
            //Environment.Exit(0);


            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    Closing = true;
                    this.Visible = false;
                    Hide();
                    GC.Collect();
                    Close();
                });
            }

        }

        private void FATALERROR_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing == false)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
