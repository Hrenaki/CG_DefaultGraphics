using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CG_DefaultGraphics.BaseComponents;


namespace CG_DefaultGraphics.Components
{
    public class AutoFlyAround : Component
    {
        public float speed = 60f;
        public float amplitude = 3f;
        private float timePassed = 0.0f;
        public override void update()
        {
            timePassed += (float)Time.DeltaTime;
            gameObject.transform.position.X = amplitude * (float)Math.Sin(timePassed * speed * (float)Math.PI / 180.0f);
            gameObject.transform.position.Z = amplitude * (float)Math.Cos(timePassed * speed * (float)Math.PI / 180.0f);
        }
    }
}
