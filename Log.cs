using System;
using System.Text;

namespace Sean.Shared
{
    public static class Log
    {
        public static void WriteInfo(string str)
        {
            Console.WriteLine(str);
        }
        public static void WriteError(string str)
        {
            Console.WriteLine(str);
        }
    }
}