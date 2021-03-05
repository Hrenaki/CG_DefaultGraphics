using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_DefaultGraphics.BaseComponents
{
    public enum LightType
    {
        Directional = 0,
        Point = 1
    }
    public class Light : Component
    {
        public LightType type;
        public float radius;
        public float brightness;
        public float smoothness;
    }
}
