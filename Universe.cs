using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniverseSimulator
{
    public static class Universe
    {
        public const float Gravity = 0.0001f;

        public static readonly int CLR_BLACK = Color.Black.ToArgb();
        public static readonly int CLR_WHITE = Color.White.ToArgb();
        public static readonly int CLR_ORANGE = Color.Orange.ToArgb();
        public static readonly int CLR_YELLOW = Color.Yellow.ToArgb();
        public static readonly int CLR_RED = Color.Red.ToArgb();
        public static readonly int CLR_DARK_RED = Color.DarkRed.ToArgb();
        public static readonly int CLR_SUPER_DARK_RED = Color.FromArgb(65, 0, 0).ToArgb();
        public static readonly int CLR_CYAN = Color.Cyan.ToArgb();
        public static readonly int CLR_GREEN = Color.Green.ToArgb();
    }
}
