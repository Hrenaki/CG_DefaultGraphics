using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK;

namespace CG_DefaultGraphics.BaseComponents
{
    public enum LightType
    {
        Ambient = 0,
        Directional = 1,
        Point = 2,
        Spot = 3
    }
    public class Light : Component
    {
        public LightType type = LightType.Point;
        public Color4 color = Color4.White;
        private float radius = 1.0f;
        public float Radius { 
            get => radius; 
            set 
            { 
                if (value < 0.0f) 
                    throw new ArgumentOutOfRangeException("Radius", "Radius can't be negative"); 
                radius = value; 
            } }
        private float brightness = 1.0f;
        public float Brightness { 
            get => brightness; 
            set 
            { 
                if (value < 0.0f || value > 1.0f) 
                    throw new ArgumentOutOfRangeException("Brightness", "Brightness can't be negative or more than 1"); 
                brightness = value; 
            } }
        private float intensity = 0.0f;
        public float Intensity { 
            get => intensity; 
            set 
            { 
                if (value < 0.0f || value > 1.0f) 
                    throw new ArgumentOutOfRangeException("Intensity", "Intensity can't be negative or more than 1"); 
                intensity = value; 
            } }
        private float angle = (float)Math.PI / 3.0f;
        public float Angle
        {
            get => angle;
            set
            {
                if (value < 0.0f || value > Math.PI)
                    throw new ArgumentOutOfRangeException("Angle", "Angle can't be negative or more than PI");
                angle = value;
            }
        }
        public Matrix4 view
        {
            get
            {
                Vector3 r = gameObject.transform.right;
                Vector3 u = gameObject.transform.up;
                Vector3 f = gameObject.transform.forward;
                Vector3 p = -gameObject.transform.position;
                Matrix4 mat = new Matrix4(r.X, u.X, f.X, 0f,
                                          r.Y, u.Y, f.Y, 0f,
                                          r.Z, u.Z, f.Z, 0f,
                                          Vector3.Dot(p, r), Vector3.Dot(p, u), Vector3.Dot(p, f), 1f);
                return mat;
            }
        }
    }
}
