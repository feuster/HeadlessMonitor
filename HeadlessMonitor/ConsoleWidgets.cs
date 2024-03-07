using System.Text;

namespace HeadlessMonitor
{
    public static class ConsoleWidgets
    {
        /// <summary>
        /// Figlet
        /// </summary>
        public const string Figlet = @"                       _   _                _ _               __  __             _ _
                      | | | | ___  __ _  __| | | ___  ___ ___|  \/  | ___  _ __ (_) |_ ___  _ __
                      | |_| |/ _ \/ _` |/ _` | |/ _ \/ __/ __| |\/| |/ _ \| '_ \| | __/ _ \| '__|
                      |  _  |  __/ (_| | (_| | |  __/\__ \__ \ |  | | (_) | | | | | || (_) | |
                      |_| |_|\___|\__,_|\__,_|_|\___||___/___/_|  |_|\___/|_| |_|_|\__\___/|_|";

        public static bool Close { get; set; } = false;

        #region Console Encoding
        /// <summary>
        /// Switch the console encoding
        /// </summary>
        /// <param name="NewEncoding">System.Text.Encoding value</param>
        internal static void ConsoleEncoding(Encoding NewEncoding)
        {
            Console.InputEncoding = NewEncoding;
            Console.OutputEncoding = NewEncoding;
        }

        /// <summary>
        /// Switch the console encoding to default
        /// </summary>
        public static void ConsoleEncodingDefault()
        {
            ConsoleEncoding(Encoding.Default);
        }

        /// <summary>
        /// Switch the console encoding to UTF8
        /// </summary>
        public static void ConsoleEncodingUTF8()
        {
            ConsoleEncoding(Encoding.UTF8);
        }

        /// <summary>
        /// Switch the console encoding to Unicode
        /// </summary>
        public static void ConsoleEncodingUnicode()
        {
            ConsoleEncoding(Encoding.Unicode);
        }

        /// <summary>
        /// Switch the console encoding to latin1
        /// </summary>
        public static void ConsoleEncodingLatin1()
        {
            ConsoleEncoding(Encoding.Latin1);
        }

        /// <summary>
        /// Switch the console encoding to ASCII
        /// </summary>
        public static void ConsoleEncodingASCII()
        {
            ConsoleEncoding(Encoding.ASCII);
        }
        #endregion

        #region Widgets
        /// <summary>
        /// Print a header line
        /// </summary>
        /// <param name="Text">text to print</param>
        /// <param name="ForeColor">text color</param>
        /// <param name="BackColor">background color</param>
        public static void PrintLine(string Text = "", ConsoleColor ForeColor = ConsoleColor.White, ConsoleColor BackColor = ConsoleColor.Black)
        {
            int w = Console.WindowWidth;
            ConsoleColor f = Console.ForegroundColor;
            ConsoleColor b = Console.BackgroundColor;
            char hyphen;
            if (Console.OutputEncoding == Encoding.ASCII)
                hyphen = '-';
            else
                hyphen = (char)0x2015;
            Console.ForegroundColor = ForeColor;
            Console.BackgroundColor = BackColor;
            string Line;
            if (!string.IsNullOrEmpty(Text))
            {
                Line = new string(hyphen, (w - Text.Length - 2) / 2) + $" {Text} ";
                Line += new string(hyphen, (w - Line.Length));
            }
            else
                Line = new string(hyphen, w);
            Console.WriteLine(Line);
            Console.ForegroundColor = f;
            Console.BackgroundColor = b;
        }

        /// <summary>
        /// "Press any key" widget
        /// </summary>
        /// <param name="ForeColor">text color</param>
        /// <param name="BackColor">background color</param>
        /// <param name="ExitProgram">exit program after a key has been pressed</param>
        public static void PressAnyKey(ConsoleColor ForeColor = ConsoleColor.DarkRed, ConsoleColor BackColor = ConsoleColor.Black, bool ExitProgram = false)
        {
            if(Close)
                Environment.Exit(0);
            PrintLine("Press any Key", ForeColor, BackColor);
            Console.ReadKey();
            if (ExitProgram)
                Environment.Exit(0);
        }

        /// <summary>
        /// "Press any key" widget with default setting
        /// </summary>
        /// <param name="ExitProgram">exit program after a key has been pressed</param>
        public static void PressAnyKey(bool ExitProgram)
        {
            PressAnyKey(ConsoleColor.Yellow, ConsoleColor.Black, ExitProgram);
        }
        #endregion
    }
}
