using ConsoleTables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TestTask.core;
using static System.TimeZoneInfo;

namespace TestTask.client
{
    public class Program
    {
        static void Main()
        {
            App app = new App();
            app.MainDialog();
        }
    }
}
