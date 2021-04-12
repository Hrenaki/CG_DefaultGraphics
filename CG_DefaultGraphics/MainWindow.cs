using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using CG_DefaultGraphics.BaseComponents;
using CG_DefaultGraphics.Components;
using OpenTK.Input;
using System.Diagnostics;

namespace CG_DefaultGraphics
{
    public class MainWindow : GameWindow
    {
        private List<GameObject> objects = new List<GameObject>();
        private Camera camera;
        private Shader shader;
        public MainWindow() : base(1920, 1080, GraphicsMode.Default, "Computer graphics")
        {
            WindowState = WindowState.Maximized;
            
            CursorVisible = false;
            CursorGrabbed = true;
        }
        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.CullFace(CullFaceMode.Back);

            shader = AssetsManager.LoadShader("default", "Assets\\Shaders\\default.vsh", "Assets\\Shaders\\default.fsh");

            GameObject cameraObject = new GameObject();
            cameraObject.transform.position.Z = -5;
            cameraObject.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitY, (float)Math.PI / 4f);
            Controller controller = (Controller)cameraObject.addComponent<Controller>();
            controller.speed = 10f;
            camera = (Camera)cameraObject.addComponent<Camera>();
            camera.FOV = (float)(85.0 / 180.0 * Math.PI);
            camera.resolution = (float)Width / (float)Height;
            camera.near = 0.01f;
            camera.far = 100f;
            objects.Add(cameraObject);

            GameObject cube = new GameObject();
            Mesh cubeMesh = (Mesh)cube.addComponent<Mesh>();
            //diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", true)["diamondwhite_dmesh"];
            cubeMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj")["cube"];
            cubeMesh.texture = AssetsManager.LoadTexture("Assets\\Textures\\template.png");
            objects.Add(cube);

            GameObject lightObj = new GameObject();
            lightObj.transform.position.X = -5.0f;
            lightObj.transform.position.Y = 5.0f;
            lightObj.transform.position.Z = -5.0f;
            Light light = (Light)lightObj.addComponent<Light>();
            light.Radius = 20;
            light.Brightness = 1.0f;
            light.Intensity = 0.0f;
            light.Angle = 10.0f * (float)Math.PI / 180.0f;
            light.type = LightType.Point;
            objects.Add(lightObj);

            for (int i = 0; i < 30; i++)
            {
                GameObject lightObj2 = new GameObject();
                lightObj2.addComponent<Light>();
                light.Radius = 20;
                light.Brightness = 1.0f;
                light.Intensity = 0.0f;
                light.Angle = 10.0f * (float)Math.PI / 180.0f;
                light.type = LightType.Point;
                objects.Add(lightObj2);
            }

            GameObject ambientObj = new GameObject();
            Light ambient = (Light)ambientObj.addComponent<Light>();
            ambient.Brightness = 0.2f;
            ambient.type = LightType.Ambient;
            objects.Add(ambientObj);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shader.id);

            Matrix4 proj = camera.proj;
            Matrix4 view = camera.view;
            Matrix4 model;
            GL.UniformMatrix4(shader.locations["proj"], false, ref proj);
            GL.UniformMatrix4(shader.locations["view"], false, ref view);

            List<Light> lights = new List<Light>();
            foreach (GameObject obj in objects)
                lights.AddRange(obj.getComponents<Light>().Cast<Light>());

            int loc_lightsPositions = shader.locations["lights[0].position"];
            int loc_lightsDirections = shader.locations["lights[0].direction"];
            int loc_lightsCoeffs = shader.locations["lights[0].coeffs"];
            int loc_lightsColors = shader.locations["lights[0].color"];

            for (int i = 0; i < lights.Count; i++)
            {
                Vector4 position = new Vector4(lights[i].gameObject.transform.position, (int)lights[i].type <= 1 ? 0.0f : 1.0f);
                Vector4 direction = new Vector4(lights[i].gameObject.transform.forward, (int)lights[i].type % 2 == 1 ? 1.0f : 0.0f);
                GL.Uniform4(loc_lightsPositions + i * 4, position);
                GL.Uniform4(loc_lightsDirections + i * 4, direction);
                GL.Uniform4(loc_lightsCoeffs + i * 4, lights[i].Radius, lights[i].Brightness, lights[i].Intensity, lights[i].Angle);
                GL.Uniform3(loc_lightsColors + i * 4, lights[i].color.R, lights[i].color.G, lights[i].color.B);
            }
            GL.Uniform1(shader.locations["lightsCount"], lights.Count);
            GL.Uniform3(shader.locations["camPos"], camera.gameObject.transform.position);

            foreach (GameObject obj in objects)
            {
                Mesh[] meshs = obj.getComponents<Mesh>().Cast<Mesh>().ToArray();
                if (meshs.Length > 0)
                {
                    model = obj.transform.model;
                    GL.UniformMatrix4(shader.locations["model"], true, ref model);
                    foreach (Mesh mesh in meshs)
                    {
                        if (mesh.texture != null)
                            GL.BindTexture(TextureTarget.Texture2D, mesh.texture.id);
                        if (mesh.model != null)
                        {
                            GL.BindVertexArray(mesh.model.VAO);
                            GL.DrawElements(PrimitiveType.Triangles, mesh.model.v_i.Count * 3, DrawElementsType.UnsignedInt, 0);
                        }
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                }
            }

            GL.BindVertexArray(0);

            SwapBuffers();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Time.DeltaTime = e.Time;
            Input.OnUpdateFrame();

            foreach (GameObject obj in objects)
                foreach (Component component in obj.components)
                    component.update();

            if (Input.IsKeyPressed(Key.Escape))
                Exit();
        }
        protected override void OnUnload(EventArgs e)
        {

        }
    }
}
