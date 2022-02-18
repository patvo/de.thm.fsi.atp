using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace de.thm.fsi.atp
{
    internal static class Program
    {
        public static Form1 frm1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            frm1 = new Form1();

            Thread mainThread = Thread.CurrentThread;
            //StartThreads();
            AtpBl atpBl = new AtpBl("192.168.178.101");

            Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(frm1);
        }

        private static void StartThreads()
        {
            foreach (DataRow row in DataController.GetReader().Rows)
            {
                Thread t = new Thread(() => StartController(row["ipAdresse"].ToString()));
                t.Start();
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="readerAddr">IP address of RFID reader</param>
        private static void StartController(string readerAddr)
        {
            AtpBl atpBl = new AtpBl(readerAddr);
        }

    }
}
