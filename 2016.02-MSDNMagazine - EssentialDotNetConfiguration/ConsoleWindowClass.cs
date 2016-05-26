using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EssentialDotNetConfiguration
{
    public class ConsoleWindow
    {
        public ConsoleWindow(){}

        public ConsoleWindow(int height, int width, int top = 0, int left = 0)
        {
            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        public int Height { get; set; }
        public int Width { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }

        public static void SetConsoleWindow(ConsoleWindow consoleWindow)
        {
            try
            {
                Console.WindowHeight = consoleWindow.Height;
                Console.WindowWidth = consoleWindow.Width;
                Console.WindowTop = consoleWindow.Top;
                Console.WindowLeft = consoleWindow.Left;
            }
            catch (System.IO.IOException){ /* Ignore */ }
        }
    }
}
