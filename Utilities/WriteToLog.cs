using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPFTrek.Utilities
{
    /*
     * There are plenty of arguments to be made over what to log and
     * when to log.  In a professional application, it's really a 
     * good idea to follow convention and maintain a robust logging 
     * system using something like nLog.
     * 
     * In a demo game where a some of you might already be
     * overloaded with information, I'm going to use a very simple
     * application logger of user interaction to give you an
     * idea of their use.
     * 
     * Here's some references to get started if you want to learn more:
     * https://www.infoworld.com/article/2980677/implement-a-simple-logger-in-c.html
     * https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.utilities.logger?view=netframework-4.8
     * https://stackoverflow.com/questions/5057567/how-do-i-do-logging-in-c-sharp-without-using-3rd-party-libraries
     * https://stackify.com/nlog-vs-log4net-vs-serilog/
     * https://raygun.com/blog/c-sharp-logging-best-practices/
     * https://stackify.com/csharp-logging-best-practices/
     * 
     */
    class WriteToLog
    {
        static readonly object pblock = new object();
        static string lostInfo=string.Empty;
        static int daysToKeepLog = 2;

        static Func<string, string> LogName =(nameChange) =>(nameChange == null || nameChange.Length == 0 ? "XLogger" : nameChange) + "_*.log";

        /*
         * Append a string to the XLogger_YYYYMMDD.Log file
         * 
         */
        public static void write(string info)
        {
            Object _lockObj = new Object();
            bool _lockWasTaken = false;

            try
            {
                // try to get a lock 
                System.Threading.Monitor.Enter(_lockObj, ref _lockWasTaken);

                // It was uccessful, so send on to the log file
                write(lostInfo+info, string.Empty, false);
                lostInfo = "";
            }
            catch (Exception)
            {
                // capture the lost message
                lostInfo += "----------> " + info+"\r\n";
            }
            finally
            {
                if (_lockWasTaken) System.Threading.Monitor.Exit(_lockObj);
            }

        }


        /*
         * Write a string to a log file with flag to erase file before writing
         * adding date & time to the string before writing
         * 
         * string - info         -> what to append to log file
         * string - nameChange   -> name to use for log file instead of XLogger
         * bool   - eraseCurrent -> if true, creates a new file
         */
        public static void write(string info, string nameChange, bool eraseCurrent)
        {
            DateTime date = DateTime.Now;
            string now = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logFileName = LogName(nameChange).Replace("*", date.ToString("yyyyMMdd"));
            string infoOut = now + " - ";

            info = info.Replace("\n",String.Empty);

            // erase the current log file?
            if (eraseCurrent)
            {
                File.Delete(logFileName);
            }

            // if there are leading EOLs then transfer them to before
            // the date and time stamp before saving
            while (info.Length > 0 && info[0]=='\r')
            {
                infoOut = "\n\r" + infoOut;
                info = info.Substring(1);
            }

            infoOut += info;

            using (StreamWriter file = new StreamWriter(logFileName, true))
            {
                file.WriteLine(infoOut);
            }
        }


        /*
         * Delete logs using default value
         */
        public static void ClearOldLogs()
        {
            ClearOldLogs("",daysToKeepLog);
        }



        /*
         * Delete log files over a certain number of days in age
         * 
         */
        public static void ClearOldLogs(string nameChange, int ageInDays)
        {
            string logFileName = LogName(nameChange);
            DateTime date = DateTime.Now;
            int fdate;
            
            // get the int value of the data format
            int.TryParse(date.ToString("yyyyMMdd"), out fdate);

            // grab all matching files
            string[] files = Directory.GetFiles(".", logFileName);
            string info = "";
            string file;
            int j;

            // is it too old?
            for (int i = 0; i < files.Length; i++)
            {
                j = files[i].IndexOf('\\');

                if (j >= 0)
                {
                    file = files[i].Substring(j + 1);
                }
                else
                {
                    file = files[i];
                }

                j = file.IndexOf('_');

                // is there a _ and is the string long enough to
                // have format of at least *_yyyyMMdd.
                if (j > 0 && file.IndexOf('.') >= j + 8) 
                {
                    int.TryParse(file.Substring(j + 1, 8), out j);

                    if (j>0)
                    {
                        if (j < (fdate - ageInDays))
                        {
                            try
                            {
                                System.IO.File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                write("Can't delete file " + file + "\r\n" + "Exception - " + ex.Message);
                            }
                        }
                    }
                }
            }
        }
    }
}
