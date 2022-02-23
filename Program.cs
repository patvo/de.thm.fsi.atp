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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread mainThread = Thread.CurrentThread;

            ////////
            // For demo purposes only 1 instance is need,
            // because there is only one physical RFID reader connected.
            // Therefore all other rfid readers listed in db are NOT instantiated!
            ////////
            //StartThreads();
            StartBl("192.168.178.144");

            Application.EnableVisualStyles();
            Application.Run();
        }

        /// <summary>
        /// Start own thread for every RFID reader listed in database.
        /// </summary>
        private static void StartThreads()
        {
            foreach (DataRow row in DataController.GetReader().Rows)
            {
                Thread t = new Thread(() => StartBl(row["ipAdresse"].ToString()));
                t.Start();
                Thread.Sleep(100); // Needed to ensure no TCP/IP connection attempts at the exact same time
            }
        }
        /// <summary>
        /// Start business logic instance with given IP address of RFID reader.
        /// </summary>
        /// <param name="readerAddr">IP address of RFID reader</param>
        private static void StartBl(string readerAddr)
        {
            AtpBl atpBl = new AtpBl(readerAddr);
        }

    }
}
