using GameEngine.Maths.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniverseSimulator
{
    public static class Utils
    {
        public static float SqrtLength(this Vector2F v) => (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        public static Vector2F Normalized(this Vector2F v) => v / v.SqrtLength();
    }
}
