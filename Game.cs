using GameEngine;
using GameEngine.Inputs;
using GameEngine.Maths;
using GameEngine.Maths.Vectors;
using GameEngine.UI;
using GameEngine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MouseButtons = GameEngine.Inputs.MouseButtons;
using Font = GameEngine.Font.Font;

namespace UniverseSimulator
{
    public class Game : Engine
    {
        public DirectBitmap trails;
        private List<GravityObject> objects;
        private Random rng;

        private bool collision;

        // Window move
        private Vector2D oldMousePos;
        private Vector2D windowPos;
        private bool dragging;

        public const int barHeight = 30;
        public UIManager ui;

        protected override void Initialize()
        {
            GameWindow.WindowStyle = WindowStyle.None;
            GameWindow.ReInit();

            GameWindow.Top = Screen.PrimaryScreen.Bounds.Height / 2 - GameWindow.Height / 2;

            Font font_roboto_thin = new Font(Environment.CurrentDirectory + "/Data/Roboto-Thin.ttf", 1);
            Font font_roboto_medium = new Font(Environment.CurrentDirectory + "/Data/Roboto-Medium.ttf", 2);
            ui = new UIManager(this, font_roboto_thin, font_roboto_medium);

            trails = new DirectBitmap(GameWindow.Buffer.Width, GameWindow.Buffer.Height);
            trails.Clear(Color.Black);
            rng = new Random(DateTime.Now.Millisecond * DateTime.Now.Second / DateTime.Now.Day);
            objects = new List<GravityObject>()
            {
                new CelestialBody(new Vector2F(GameWindow.Buffer.Width / 2, GameWindow.Buffer.Height / 2), 50000000f, 60f, new Vector2F(0f, 0f), Color.Yellow, ui, 0),
            };

            GenRandom(50);

            GameWindow.Input.MouseDown += (object sender, IMouseEventArgs args) =>
            {
                if (args.Position.Y < 30 && (args.Buttons & MouseButtons.Left) == MouseButtons.Left && GameWindow.WindowState != WindowState.Minimized) {
                    if (args.Position.X > GameWindow.Width - 31) // Close if clicked the X button
                    {
                        Close();
                    }
                    else if (args.Position.X > GameWindow.Width - 62) // Minimize if clicked the - button
                    {
                        GameWindow.WindowState = WindowState.Minimized;
                        GameWindow.Input.ResetMouseButtons();
                    }
                    else
                        dragging = true;
                }
            };
            windowPos = GameWindow.positionD;

            dragging = false;

            SetConsolePosAndSize();

            Console.CursorVisible = false;
            collision = false;
        }

        private void GenRandom(int count)
        {
            for (int i = 0; i < count; i++) {
                float angle = ((float)Math.PI / 180f) * (float)rng.Next(0, 360);
                Vector2I offset = (Vector2I)(new Vector2F((float)Math.Cos(angle), (float)Math.Sin(angle)) * (float)rng.Next(100, GameWindow.Width / 2 - 20));
                float size = (float)rng.NextDouble() * 16f + 2f;
                float mass = (float)rng.NextDouble() * size * 5f + size;

                float angleOff = rng.Next(0, 20) < 10 ? 90f : -90f;
                Vector2F velocity = new Vector2F((float)Math.Cos(angle + angleOff), (float)Math.Sin(angle + angleOff)) * (float)rng.Next(30, 90);

                objects.Add(new GravityObject(new Vector2F(GameWindow.Width / 2, GameWindow.Height / 2) + offset, mass, size, velocity, ui, objects.Count));
            }
        }

        protected override void Draw(int x, int y, Color color)
        {
            Draw(x, y, color.ToArgb());
        }

        protected override void Draw(int x, int y, int color)
        {
            if (x < 0 || y < 0 || x >= GameWindow.Width || y >= GameWindow.Height)
                return;

            Color c = Color.FromArgb(color);

            if (c.R == 255 && c.G == 255 && c.B == 255) {
                Color clr = Color.FromArgb(GameWindow.Buffer.Read(x, y));
                if ((clr.R == 255 && clr.G == 255 && clr.B == 255) || (clr.R == 255 && clr.G == 255 && clr.B == 0))
                    collision = true;
            }

            base.Draw(x, y, color);
        }

        private void SetConsolePosAndSize()
        {
            SystemPlus.Extensions.ConsoleExtensions.RECT r = SystemPlus.Extensions.ConsoleExtensions.GetWindowRect();
            SystemPlus.Extensions.ConsoleExtensions.SetWindowPosition((int)GameWindow.Left + GameWindow.Width, (int)GameWindow.Top, 300, objects.Count * 30 + 36 * 2);
        }

        float logTimer = 0f;
        protected override void drawInternal()
        {
            Array.Copy(trails.Data, GameWindow.Buffer.Data, trails.Data.Length);
            trails.Clear(Color.Black.ToArgb());

            // Update
            float delta = FpsCounter.DeltaTimeF;

            HandleWindowDrag();

            for (int i = 0; i < objects.Count; i++)
                objects[i].UpdateVelocity(objects, delta);

            for (int i = 0; i < objects.Count; i++)
                objects[i].UpdatePos(delta);

            logTimer += delta;
            if (logTimer > 1f) {
                logTimer %= 1f;
                Console.CursorVisible = false;
                Console.Clear();
                for (int i = 0; i < objects.Count; i++) {
                    Console.CursorTop = i;
                    Console.CursorLeft = 0;
                    Console.WriteLine($"ID: {i}, X: {MathPlus.Round(objects[i].Pos.X, 2)}, Y: {MathPlus.Round(objects[i].Pos.Y, 2)}");
                }
                Console.CursorTop = objects.Count;
                Console.CursorLeft = 0;
                Console.WriteLine($"Casched text: {ui.CaschedTextCount}");
            }

            // Draw
            collision = false;
            for (int i = 0; i < objects.Count; i++) {
                objects[i].Draw();
                if (collision) {
                    float minDistance = float.MaxValue;
                    GravityObject hit = null;
                    for (int j = 0; j < objects.Count; j++)
                        if (j != i) {
                            float dist = (objects[i].Pos - objects[j].Pos).SqrtLength();
                            if (dist < minDistance && dist < objects[i].Radius + objects[j].Radius + 25f) {
                                minDistance = dist;
                                hit = objects[j];
                            }
                        }

                    if (hit == null) {
                        ui.RemoveElement(objects[i].infoTextId);
                        objects.RemoveAt(i);
                        continue;
                    }
                    else if (hit.Radius > objects[i].Radius) {
                        float diff = (float)Math.Abs(hit.Mass - objects[i].Mass);
                        hit.velocity += objects[i].velocity / diff;
                        ui.RemoveElement(objects[i].infoTextId);
                        objects.RemoveAt(i);
                    }
                    else {
                        float diff = (float)Math.Abs(hit.Mass - objects[i].Mass);
                        objects[i].velocity += hit.velocity / diff;
                        ui.RemoveElement(hit.infoTextId);
                        objects.Remove(hit);
                    }

                    SetConsolePosAndSize();
                }
                collision = false;
            }

            if (ui.CaschedTextCount > 3000)
                ui.ClearCaschedText();

            try {
                ui.Draw();
            } catch { }

            DrawTopbar();
        }

        private void DrawTopbar()
        {
            // White bar
            Fill(0, 0, GameWindow.Width - 61, barHeight, Color.White.ToArgb());

            // Window title
            ui.selectedFontIndex = 1;
            ui.RenderAndCaschText($"{GameWindow.Title} | FPS: {SystemPlus.MathPlus.Round(FpsCounter.FpsGlobal, 1)}", 30, Color.Black.ToArgb(), 10, 5);
            ui.selectedFontIndex = 0;

            // Get mouse pos
            int mx = Cursor.Position.X - GameWindow.position.X;
            int my = Cursor.Position.Y - GameWindow.position.Y;
            Vector2I mp = new Vector2I(mx, my);

            // Close button
            Color cbbc = Color.FromArgb(220, 220, 220); // Close button background color
            int cblc = Universe.CLR_BLACK; // Close button line color
            if (mp.X > GameWindow.Width - 31 && mp.Y <= barHeight && mp.X < GameWindow.Width && mp.Y > 0) {
                cbbc = Color.Red;
                cblc = Universe.CLR_WHITE;
            }

            Fill(GameWindow.Width - 31, 0, GameWindow.Width, barHeight, cbbc);
            DrawLine(GameWindow.Width - 26, 5, GameWindow.Width - 6, 25, cblc);
            DrawLine(GameWindow.Width - 26, 25, GameWindow.Width - 6, 5, cblc);

            // Minimize button
            Color mbbc = Color.FromArgb(220, 220, 220); // Minimize button background color
            int mblc = Universe.CLR_BLACK; // Minimize button line color
            if (mp.X > GameWindow.Width - 61 && mp.Y <= barHeight && mp.X < GameWindow.Width - 31 && mp.Y > 0) {
                mbbc = Color.FromArgb(160, 160, 160);
                mblc = Universe.CLR_WHITE;
            }

            Fill(GameWindow.Width - 61, 0, GameWindow.Width - 31, barHeight, mbbc);
            DrawLine(GameWindow.Width - 56, 15, GameWindow.Width - 36, 15, mblc);
        }

        private void HandleWindowDrag()
        {
            Vector2I mps = GameWindow.Input.mousePosScreen;
            Vector2D mousePos = new Vector2D(Cursor.Position.X, Cursor.Position.Y);
            bool leftDown = (GameWindow.Input.mouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (!leftDown)
                dragging = false;
            if (leftDown && dragging && mps.Y <= 30 && mps.X > 0 && mps.X < GameWindow.Width && mps.Y > 0 && GameWindow.WindowState != WindowState.Minimized) {
                Vector2D diff = mousePos - oldMousePos;
                if (diff != Vector2D.Zero) {
                    windowPos += diff;
                    SetConsolePosAndSize();
                }
            }
            oldMousePos = mousePos;

            GameWindow.positionD = windowPos;
        }

        private void Close()
        {
            ui?.ClearCaschedText();
            base.Exit();
            Environment.Exit(0);
        }

        public new void FillCircle(Vector2I pos, int radius, Color color)
            => base.FillCircle(pos, radius, color);
        public new void FillCircle(Vector2I pos, int radius, int color)
            => base.FillCircle(pos, radius, color);

        public void DrawPoint(Vector2I pos, int color)
            => Draw(pos, color);
        public new void DrawLine(Vector2I pos1, Vector2I pos2, int color)
            => base.DrawLine(pos1, pos2, color);

        public void DrawTrail(Vector2I pos1, Vector2I pos2, int color)
            => DrawTrail(pos1.X, pos1.Y, pos2.X, pos2.Y, color);
        public void DrawTrail(int x1, int y1, int x2, int y2, int color)
        {
            int x, y, dx, dy, dx1, dy1, px, py, xe, ye, i;
            dx = x2 - x1; dy = y2 - y1;
            dx1 = Math.Abs(dx); dy1 = Math.Abs(dy);
            px = 2 * dy1 - dx1; py = 2 * dx1 - dy1;
            if (dy1 <= dx1) {
                if (dx >= 0) { x = x1; y = y1; xe = x2; }
                else { x = x2; y = y2; xe = x1; }

                trails.Write(x, y, color);

                for (i = 0; x < xe; i++) {
                    x = x + 1;
                    if (px < 0)
                        px = px + 2 * dy1;
                    else {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) y = y + 1; else y = y - 1;
                        px = px + 2 * (dy1 - dx1);
                    }
                    trails.Write(x, y, color);
                }
            }
            else {
                if (dy >= 0) { x = x1; y = y1; ye = y2; }
                else { x = x2; y = y2; ye = y1; }

                trails.Write(x, y, color);

                for (i = 0; y < ye; i++) {
                    y = y + 1;
                    if (py <= 0)
                        py = py + 2 * dx1;
                    else {
                        if ((dx < 0 && dy < 0) || (dx > 0 && dy > 0)) x = x + 1; else x = x - 1;
                        py = py + 2 * (dx1 - dy1);
                    }
                    trails.Write(x, y, color);
                }
            }
        }
    }
}
