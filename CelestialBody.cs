using GameEngine.Maths;
using GameEngine.Maths.Vectors;
using GameEngine.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniverseSimulator
{
    public class CelestialBody : GravityObject
    {
        public int Color;

        public CelestialBody(Vector2F _pos, float _mass, float _radius, Vector2F _initialVelocity, Color _color, UIManager ui, int _index) 
            : base(_pos, _mass, _radius, _initialVelocity, ui, _index)
        {
            Color = _color.ToArgb();
        }

        public override void Draw()
        {
            if ((int)Pos.X >= 0 && (int)Pos.Y >= Game.barHeight && (int)Pos.X < (int)Program.game.trails.Width && Pos.Y < Program.game.trails.Height) {
                Program.game.DrawLine((Vector2I)Pos, (Vector2I)Pos + (Vector2I)velocity, Universe.CLR_ORANGE);
                Program.game.FillCircle((Vector2I)Pos, (int)Radius, Color);
                infoText.X = (int)Pos.X;
                infoText.Y = (int)Pos.Y + (int)Radius + 4;
                infoText.Active = true;
                float length = velocity.SqrtLength();
                infoText.Text = $"Speed {MathPlus.Round(length, 2)}\n" +
                                $"ID {index}";
            }
            else {
                int x = MathPlus.Clamp((int)Pos.X, 2, Program.game.trails.Width - 2);
                int y = MathPlus.Clamp((int)Pos.Y, Game.barHeight + 2, Program.game.trails.Height - 2);

                int s = 5;
                for (int _x = -s; _x <= s; _x++)
                    for (int _y = -s; _y <= s; _y++)
                        Program.game.DrawPoint(new Vector2I(x + _x, y + _y), Universe.CLR_CYAN);
                infoText.Active = false;
            }

            if ((int)Pos.X >= 0 && (int)Pos.Y >= Game.barHeight && (int)Pos.X < (int)Program.game.trails.Width && Pos.Y < Program.game.trails.Height &&
                lastX < 0 || lastY < Game.barHeight || lastX >= (int)Program.game.trails.Width || lastY > Program.game.trails.Height)
                trailPoints.Clear();

            lastX = (int)Pos.X;
            lastY = (int)Pos.Y;

            if ((int)Pos.X >= 0 && (int)Pos.Y >= 0 && (int)Pos.X < (int)Program.game.trails.Width && Pos.Y < Program.game.trails.Height)
                trailPoints.Add(new Vector2I((int)Pos.X, (int)Pos.Y));

            if (trailPoints.Count > 500)
                trailPoints.RemoveAt(0);

            for (int i = 1; i < trailPoints.Count; i++)
                Program.game.DrawTrail(trailPoints[i - 1], trailPoints[i], Universe.CLR_SUPER_DARK_RED);
        }
    }
}
