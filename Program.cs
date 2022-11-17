using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniverseSimulator
{
    static class Program
    {
        public static Game game;

        [STAThread]
        static void Main(string[] args)
        {
            game = new Game();
            game.Run(new Size(1000, 1000), "UniverseSimulator");
        }
    }
}
