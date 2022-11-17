using GameEngine.Maths.Vectors;
using GameEngine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniverseSimulator
{
    public static class ParticleSystem
    {
        private static List<Particle> particles = new List<Particle>();

        public static void Add(Particle p) => particles.Add(p);

        public static void Add(Vector2F _pos, Vector2F _velocity, float _lifeTime)
            => Add(new Particle(_pos, _velocity, _lifeTime));
        public static void Add(Vector2F _pos, Vector2F _velocity, float _lifeTime, int color)
            => Add(new Particle(_pos, _velocity, _lifeTime, color));

        public static void Update(float delta)
        {
            for (int i = 0; i < particles.Count; i++) {
                particles[i].position += particles[i].velocity * delta;
                particles[i].lidedFor += delta;
                if (particles[i].lidedFor > particles[i].lifeTime)
                    particles.RemoveAt(i);
            }
        }

        public static void Render(DirectBitmap buffer)
        {
            for (int i = 0; i < particles.Count; i++) {
                int x = (int)particles[i].position.X;
                int y = (int)particles[i].position.Y;
                if (x >= 0 && y >= 0 && x < buffer.Width && y < buffer.Height)
                    buffer.Write(x, y, particles[i].color);
            }
        }
    }

    public class Particle
    {
        public float lifeTime;
        public float lidedFor;
        public Vector2F velocity;
        public Vector2F position;
        public int color;

        public Particle(Vector2F _pos, Vector2F _velocity, float _lifeTime)
        {
            position = _pos;
            velocity = _velocity;
            lifeTime = _lifeTime;
            lidedFor = 0f;
            color = Universe.CLR_WHITE;
        }
        public Particle(Vector2F _pos, Vector2F _velocity, float _lifeTime, int _color)
        {
            position = _pos;
            velocity = _velocity;
            lifeTime = _lifeTime;
            lidedFor = 0f;
            color = _color;
        }
    }
}
