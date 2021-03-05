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
        public float FOV;
        public float resolution;
        public float near;
        public float far;
        public Matrix4 view { get
            {
                //return Matrix4.LookAt(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward, gameObject.transform.up);
                //Matrix4 mat = new Matrix4(new Vector4(gameObject.transform.right, 0f),
                //                          new Vector4(gameObject.transform.up, 0f),
                //                          new Vector4(gameObject.transform.forward, 0f),
                //                          new Vector4(-gameObject.transform.position, 1f));
                //return mat;
                Matrix4 translation = new Matrix4(1, 0, 0, gameObject.transform.position.X,
                                                  0, 1, 0, gameObject.transform.position.Y,
                                                  0, 0, 1, gameObject.transform.position.Z,
                                                  0, 0, 0, 1);
                Matrix4 rot = gameObject.transform.rotation.createRotation();//Matrix4.CreateFromQuaternion(gameObject.transform.rotation);
                return (translation * rot).Inverted();
            } 
        }
        public Matrix4 proj { get
            {
                //return Matrix4.CreatePerspectiveFieldOfView(FOV, resolution, near, far);
                float ctg = 1f / (float)Math.Tan(FOV / 2f);
                return new Matrix4(ctg / resolution, 0f, 0f, 0f,
                                   0f, ctg, 0f, 0f,
                                   0f, 0f, (far + near) / (far - near), 1f,
                                   0f, 0f, -2f * far * near / (far - near), 0f);
            }
        }
    }
}
