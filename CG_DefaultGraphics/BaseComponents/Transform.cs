using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace CG_DefaultGraphics.BaseComponents
{
    public class Transform : Component
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Matrix4 model { get 
            {
                //return Matrix4.CreateScale(scale.X, scale.Y, scale.Z) * 
                //       Matrix4.CreateFromQuaternion(rotation) * 
                //       Matrix4.CreateTranslation(position.X, position.Y, position.Z); 
                Matrix4 translation = new Matrix4(1, 0, 0, position.X,
                                                  0, 1, 0, position.Y,
                                                  0, 0, 1, position.Z,
                                                  0, 0, 0, 1);
                Matrix4 rot = Matrix4.CreateFromQuaternion(rotation);
                return translation * rot;
            }
        }
        public Vector3 forward { get { return rotation * Vector3.UnitZ; } }
        public Vector3 right { get { return rotation * Vector3.UnitX; } }
        public Vector3 up { get { return rotation * Vector3.UnitY; } }
        public Transform()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
            scale = new Vector3(1f, 1f, 1f);
        }
    }
}
