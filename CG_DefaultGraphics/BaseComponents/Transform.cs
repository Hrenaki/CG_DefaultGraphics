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
        public Transform Parent;
        public Vector3 position;
        public Quaternion rotation;
        public Matrix4 model { get 
            {
                Matrix4 mat = Matrix4.CreateFromQuaternion(rotation);
                mat.M14 = position.X;
                mat.M24 = position.Y;
                mat.M34 = position.Z;
                return Parent == null ? mat : Parent.model * mat;
            }
        }
        public Vector3 forward { get { return rotation * Vector3.UnitZ; } }
        public Vector3 right { get { return rotation * Vector3.UnitX; } }
        public Vector3 up { get { return rotation * Vector3.UnitY; } }
        public Transform()
        {
            position = Vector3.Zero;
            rotation = Quaternion.Identity;
        }
    }
}
