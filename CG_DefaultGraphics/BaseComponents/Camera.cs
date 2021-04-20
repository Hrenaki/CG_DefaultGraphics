using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CG_DefaultGraphics.BaseComponents
{
    public class Camera : Component
    {
        public static Camera Current { get; private set; }

        public float FOV = (float)Math.PI / 2f;
        public float resolution;
        public float near;
        public float far;
        public Matrix4 camSpace { get
            {
                Vector3 r = gameObject.transform.right;
                Vector3 u = gameObject.transform.up;
                Vector3 f = gameObject.transform.forward;
                Vector3 p = -gameObject.transform.position;
                float ctg = 1f / (float)Math.Tan(FOV / 2f); ;
                Matrix4 view = new Matrix4(r.X, r.Y, r.Z, Vector3.Dot(p, r),
                                           u.X, u.Y, u.Z, Vector3.Dot(p, u),
                                           f.X, f.Y, f.Z, Vector3.Dot(p, f),
                                           0f, 0f, 0f, 1f);
                Matrix4 proj = new Matrix4(ctg / resolution, 0f, 0f, 0f,
                                           0f, ctg, 0f, 0f,
                                           0f, 0f, (far + near) / (far - near), -2f * far * near / (far - near),
                                           0f, 0f, 1f, 0f);
                return proj * view;
            } 
        }
        public void MakeCurrent()
        {
            Camera.Current = this;
        }
    }
}
