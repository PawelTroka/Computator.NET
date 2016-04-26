using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Computator.NET.Logging
{
    public class SimpleLogger
    {
        public static readonly string LogsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Computator.NET", "Logs");

        public SimpleLogger(object o) : this()
        {
            ClassName = o.GetType().FullName;
        }

        public SimpleLogger()
        {
            Parameters = new Dictionary<string, object>();
        }

        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }

        public void Log(string message, ErrorType errorType, Exception ex)
        {
            var date = DateTime.Now;
            var fileName = date.ToString("yyyy-MM-dd   HH.mm.ss") + "  " + errorType + ".log";

            if (!Directory.Exists(LogsDirectory))
                Directory.CreateDirectory(LogsDirectory);

            var sw = new StreamWriter(Path.Combine(LogsDirectory, fileName));


            sw.WriteLine("Date: ");
            sw.WriteLine(date.ToLongDateString());
            sw.WriteLine();
            sw.WriteLine();


            sw.WriteLine("Class: ");
            sw.WriteLine(ClassName);
            sw.WriteLine();
            sw.WriteLine();

            sw.WriteLine("Method: ");
            sw.WriteLine(MethodName);
            sw.WriteLine();
            sw.WriteLine();


            sw.WriteLine("System: ");
            foreach (var prop in typeof(Environment).GetProperties())
            {
                if (prop.CanRead)
                    sw.WriteLine(prop.Name + ":" + "" + prop.GetValue(null, null));
            }
            sw.WriteLine();
            sw.WriteLine();


            sw.WriteLine("Parameters: ");
            foreach (var param in Parameters)
            {
                sw.WriteLine(param.Key + ": " + param.Value);
            }
            sw.WriteLine();
            sw.WriteLine();


            sw.WriteLine("Message: ");
            sw.WriteLine(message);
            sw.WriteLine();
            sw.WriteLine();


            var exception = ex;
            var count = 0;

            sw.WriteLine("-------------------------------------------------");
            while (exception != null)
            {
                sw.WriteLine("Exception" + count);

                sw.WriteLine("ToString():");
                sw.WriteLine(exception.ToString());
                sw.WriteLine("---------------------------");


                sw.WriteLine("Data:");
                foreach (DictionaryEntry d in exception.Data)
                    sw.WriteLine(d.Key + ": " + d.Value);
                sw.WriteLine("---------------------------");


                sw.WriteLine("HelpLink:");
                sw.WriteLine(exception.HelpLink);
                sw.WriteLine("---------------------------");

                sw.WriteLine("Source:");
                sw.WriteLine(exception.Source);
                sw.WriteLine("---------------------------");

                sw.WriteLine("StackTrace:");
                sw.WriteLine(exception.StackTrace);
                sw.WriteLine("---------------------------");

                sw.WriteLine("TargetSite:");
                sw.WriteLine(exception.TargetSite);
                sw.WriteLine("-------------------------------------------------");

                exception = exception.InnerException;
                count++;
            }
            sw.Close();
        }
    }
}