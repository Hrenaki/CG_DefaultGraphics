using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CG_DefaultGraphics
{
    public static class Extensions
    {
        public static Matrix4 createRotation(this Quaternion q)
        {
            return new Matrix4(1f - 2f * (q.Y * q.Y + q.Z * q.Z), 2f * (q.X * q.Y - q.W * q.Z), 2f * (q.X * q.Z + q.W * q.Y), 0f,
                               2f * (q.X * q.Y + q.W * q.Z), 1f - 2f * (q.X * q.X + q.Z * q.Z), 2f * (q.Y * q.Z - q.W * q.X), 0f,
                               2f * (q.X * q.Z - q.W * q.Y), 2f * (q.Y * q.Z + q.W * q.X), 1f - 2f * (q.X * q.X + q.Y * q.Y), 0f,
                               0f, 0f, 0f, 1f);
        }
    }
}
