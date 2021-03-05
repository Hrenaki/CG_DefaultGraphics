using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CG_DefaultGraphics.BaseComponents;
using OpenTK;
using OpenTK.Input;

namespace CG_DefaultGraphics.Components
{
    public class Controller : Component
    {
        public float speed = 1f;
        public override void update()
        {
            float curSpeed = speed * (float)Time.DeltaTime;
            if (Input.IsKeyDown(Key.ShiftLeft))
                curSpeed *= 5f;
            if (Input.IsKeyDown(Key.A))
                gameObject.transform.position -= gameObject.transform.right * curSpeed;
            if (Input.IsKeyDown(Key.D))
                gameObject.transform.position += gameObject.transform.right * curSpeed;
            if (Input.IsKeyDown(Key.S))
                gameObject.transform.position -= gameObject.transform.forward * curSpeed;
            if (Input.IsKeyDown(Key.W))
                gameObject.transform.position += gameObject.transform.forward * curSpeed;
            if (Input.IsKeyDown(Key.C))
                gameObject.transform.position -= Vector3.UnitY * curSpeed;
            if (Input.IsKeyDown(Key.Space))
                gameObject.transform.position += Vector3.UnitY * curSpeed;

            Console.WriteLine(gameObject.transform.position.ToString());

            Vector2 mouseDelta = Input.GetMouseDelta() / 1000f;
            gameObject.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitY, mouseDelta.X) * Quaternion.FromAxisAngle(gameObject.transform.right, mouseDelta.Y) * gameObject.transform.rotation;
        }
    }
}
