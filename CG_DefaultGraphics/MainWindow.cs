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

namespace CG_DefaultGraphics
{
    public class MainWindow : GameWindow
    {
        private List<GameObject> objects = new List<GameObject>();
        private Camera camera;
        private Shader shader;
        public MainWindow() : base(1200, 920, GraphicsMode.Default, "Computer graphics")
        {
            //WindowState = WindowState.Maximized;
            CursorVisible = false;
            CursorGrabbed = true;
        }
        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            //GL.CullFace(CullFaceMode.Back);

            shader = AssetsManager.LoadShader("default", "Assets\\Shaders\\default.vsh", "Assets\\Shaders\\default.fsh");

            GameObject cameraObject = new GameObject();
            Controller controller = (Controller)cameraObject.addComponent<Controller>();
            controller.speed = 10f;
            camera = (Camera)cameraObject.addComponent<Camera>();
            camera.FOV = (float)(70.0 / 180.0 * Math.PI);
            camera.resolution = (float)Width / (float)Height;
            camera.near = 0.01f;
            camera.far = 100f;
            objects.Add(cameraObject);

            GameObject diamond = new GameObject();
            //diamond.transform.position.Z = 5f;
            Mesh diamondMesh = (Mesh)diamond.addComponent<Mesh>();
            diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", true)["diamondwhite_dmesh"];
            //diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj")["cube"];
            //diamondMesh.texture = AssetsManager.LoadTexture("Assets\\Textures\\template.png");
            objects.Add(diamond);
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
            GL.UniformMatrix4(shader.locations["view"], true, ref view);

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
