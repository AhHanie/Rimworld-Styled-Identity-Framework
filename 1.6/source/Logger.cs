using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Styled_Identity_Framework
{
    public static class Logger
    {
        private const string Prefix = "[Styled Identity Framework] ";

        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Log.Message(Prefix + message);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Log.Warning(Prefix + message);
        }

        [Conditional("DEBUG")]
        public static void Error(string message)
        {
            Log.Error(Prefix + message);
        }

        [Conditional("DEBUG")]
        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            string prefix = string.IsNullOrWhiteSpace(context) ? Prefix : Prefix + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
