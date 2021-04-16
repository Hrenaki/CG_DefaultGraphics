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
        private Shader shader;
        public MainWindow() : base(1920, 1080, GraphicsMode.Default, "Computer graphics")
        {
            Input.Init();

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

            shader = AssetsManager.LoadShader("default", new ShaderComponent("Assets\\Shaders\\default.vsh"), new ShaderComponent("Assets\\Shaders\\default.fsh"));
            AssetsManager.LoadShader("light_directional", new ShaderComponent("Assets\\Shaders\\light_directional.vsh"), new ShaderComponent("Assets\\Shaders\\light_directional.fsh"));
            AssetsManager.LoadShader("light_point", new ShaderComponent("Assets\\Shaders\\light_point.vsh"), new ShaderComponent("Assets\\Shaders\\light_point.gsh"), new ShaderComponent("Assets\\Shaders\\light_point.fsh"));
            AssetsManager.LoadShader("quad", new ShaderComponent("Assets\\Shaders\\quad.vsh"), new ShaderComponent("Assets\\Shaders\\quad.fsh"));

            GameObject cameraObject = new GameObject();
            cameraObject.transform.position = new Vector3(0f, 2f, -4f);
            cameraObject.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI / 6f);
            Controller controller = (Controller)cameraObject.addComponent<Controller>();
            controller.speed = 10f;
            Camera camera = (Camera)cameraObject.addComponent<Camera>();
            camera.FOV = (float)(85.0 / 180.0 * Math.PI);
            camera.resolution = (float)Width / (float)Height;
            camera.near = 0.01f;
            camera.far = 100f;
            objects.Add(cameraObject);
            camera.MakeCurrent();

            AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj");
            //Model bigCube = AssetsManager.Models["cube"];
            //AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj");
            //AssetsManager.Models["bigCube"] = bigCube;

            GameObject cube = new GameObject();
            Mesh cubeMesh = (Mesh)cube.addComponent<Mesh>();
            //cubeMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", 1.0f, true)["diamondwhite_dmesh"];
            cubeMesh.model = AssetsManager.Models["cube"];
            cubeMesh.texture = AssetsManager.LoadTexture("Assets\\Textures\\template.png");
            objects.Add(cube);

            AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj", 5f, true);

            GameObject cube2 = new GameObject();
            Mesh cubeMesh2 = (Mesh)cube2.addComponent<Mesh>();
            //diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", true)["diamondwhite_dmesh"];
            cubeMesh2.model = AssetsManager.Models["cube"];
            cubeMesh2.texture = AssetsManager.Textures["template"];
            objects.Add(cube2);

            //GameObject lightObj = new GameObject();
            //lightObj.transform.position.X = -5.0f;
            //lightObj.transform.position.Y = 5.0f;
            //lightObj.transform.position.Z = -5.0f;
            //Light light = (Light)lightObj.addComponent<Light>();
            //light.Radius = 20;
            //light.Brightness = 1.0f;
            //light.Intensity = 0.0f;
            //light.Angle = 10.0f * (float)Math.PI / 180.0f;
            //light.type = LightType.Point;
            //objects.Add(lightObj);

            GameObject ambientObj = new GameObject();
            AmbientLight ambient = (AmbientLight)ambientObj.addComponent<AmbientLight>();
            ambient.Brightness = 0.2f;
            objects.Add(ambientObj);

            GameObject directionalObj = new GameObject();
            directionalObj.transform.position = new Vector3(0f, 2f, -4f);
            directionalObj.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI / 6f);
            SpotLight directional = (SpotLight)directionalObj.addComponent<SpotLight>();
            directional.Brightness = 0.4f;
            directional.Radius = 20f;
            directional.Intensity = 1f;
            directional.color = Color4.DarkOrange;
            directional.Angle = (float)Math.PI / 180f * 100f;
            objects.Add(directionalObj);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            renderLights();
            renderScene();
        }
        private void renderLights()
        {
            //GL.CullFace(CullFaceMode.Front);

            List<Light> lights = new List<Light>();
            foreach (GameObject obj in objects)
                lights.AddRange(obj.getComponents<Light>().Cast<Light>());

            Shader shader_directional = AssetsManager.Shaders["light_directional"];
            Shader shader_point = AssetsManager.Shaders["light_point"];

            Matrix4 model;

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] is AmbientLight)
                    continue;

                if (lights[i] is SpotLight)
                {
                    SpotLight curLight = lights[i] as SpotLight;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, SpotLight.SHADOW_SIZE, SpotLight.SHADOW_SIZE);

                    Matrix4 lightSpace = curLight.lightSpace;

                    GL.UseProgram(shader_directional.id);

                    GL.UniformMatrix4(shader_directional.locations["lightSpace"], true, ref lightSpace);
                }
                else if (lights[i] is DirectionalLight)
                {
                    DirectionalLight curLight = lights[i] as DirectionalLight;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, SpotLight.SHADOW_SIZE, SpotLight.SHADOW_SIZE);

                    Matrix4 lightSpace = curLight.lightSpace;

                    GL.UseProgram(shader_directional.id);

                    GL.UniformMatrix4(shader_directional.locations["lightSpace"], true, ref lightSpace);
                } else
                {
                    PointLight curLight = lights[i] as PointLight;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, SpotLight.SHADOW_SIZE, SpotLight.SHADOW_SIZE);

                    GL.UseProgram(shader_point.id);

                    Matrix4[] lightSpaces = curLight.lightSpaces;
                    float[] data = new float[16 * 6];
                    for (int mat = 0; i < 6; i++)
                        for (int y = 0; y < 4; y++)
                            for (int x = 0; x < 4; x++)
                                data[mat * 16 + y * 4 + x] = lightSpaces[mat][y, x];

                    GL.UniformMatrix4(shader_point.locations["lightSpaces"], 6, true, data);
                    GL.Uniform3(shader_point.locations["lightPos"], curLight.gameObject.transform.position);
                    GL.Uniform1(shader_point.locations["far"], curLight.Radius);
                }

                foreach (GameObject obj in objects)
                {
                    Mesh[] meshs = obj.getComponents<Mesh>().Cast<Mesh>().ToArray();
                    if (meshs.Length > 0)
                    {
                        model = obj.transform.model;
                        GL.UniformMatrix4(shader_directional.locations["model"], true, ref model);
                        foreach (Mesh mesh in meshs)
                        {
                            if (mesh.model != null)
                            {
                                GL.BindVertexArray(mesh.model.VAO);
                                GL.DrawElements(PrimitiveType.Triangles, mesh.model.v_i.Count * 3, DrawElementsType.UnsignedInt, 0);
                            }
                        }
                    }
                }
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //renderDepthBuffer((lights[1] as SpotLight).shadowTex, SpotLight.NEAR, (lights[1] as SpotLight).Radius);
        }
        private void renderScene()
        {
            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);
            GL.Viewport(0, 0, Width, Height);

            GL.UseProgram(shader.id);

            List<Light> lights = new List<Light>();
            foreach (GameObject obj in objects)
                lights.AddRange(obj.getComponents<Light>().Cast<Light>());

            //int loc_lightsPositions = shader.locations["lights[0].position"];
            //int loc_lightsDirections = shader.locations["lights[0].direction"];
            //int loc_lightsCoeffs = shader.locations["lights[0].coeffs"];
            //int loc_lightsColors = shader.locations["lights[0].color"];
            //int loc_lightSpaces = shader.locations["lights[0].lightSpace"];
            //int loc_shadowMap = shader.locations["lights[0].shadowTex"];
            ////int loc_shadowCube = shader.locations["lights[0].shadowCube"];
            //int loc_offset = 6;

            int ambientLights = 0;
            int spotLights = 0;
            int directionalLights = 0;
            int pointLights = 0;

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] is SpotLight)
                {
                    SpotLight curLight = lights[i] as SpotLight;

                    Matrix4 lightSpace = curLight.lightSpace;
                    //float[] data = new float[16 * 6];
                    //for (int y = 0; y < 4; y++)
                    //    for (int x = 0; x < 4; x++)
                    //        data[y * 4 + x] = lightSpace[y, x];
                    //
                    //GL.UniformMatrix4(loc_lightSpaces + i * loc_offset, 6, true, data);
                    GL.UniformMatrix4(shader.locations["spotLights[" + spotLights.ToString() + "].lightSpace"], true, ref lightSpace);

                    GL.Uniform1(shader.locations["spotLights[" + spotLights.ToString() + "].shadowTex"], i + 1);
                    GL.ActiveTexture(TextureUnit.Texture1 + i);
                    GL.BindTexture(TextureTarget.Texture2D, curLight.shadowTex);

                    GL.Uniform3(shader.locations["spotLights[" + spotLights.ToString() + "].position"], curLight.gameObject.transform.position);
                    GL.Uniform3(shader.locations["spotLights[" + spotLights.ToString() + "].direction"], curLight.gameObject.transform.forward);
                    GL.Uniform1(shader.locations["spotLights[" + spotLights.ToString() + "].radius"], curLight.Radius);
                    GL.Uniform1(shader.locations["spotLights[" + spotLights.ToString() + "].brightness"], curLight.Brightness);
                    GL.Uniform1(shader.locations["spotLights[" + spotLights.ToString() + "].intensity"], curLight.Intensity);
                    GL.Uniform1(shader.locations["spotLights[" + spotLights.ToString() + "].angle"], curLight.Angle);
                    GL.Uniform3(shader.locations["spotLights[" + spotLights.ToString() + "].color"], lights[i].color.R, lights[i].color.G, lights[i].color.B);

                    spotLights++;
                }
                else if (lights[i] is DirectionalLight)
                {
                    DirectionalLight curLight = lights[i] as DirectionalLight;

                    Matrix4 lightSpace = curLight.lightSpace;
                    GL.UniformMatrix4(shader.locations["directionalLights[" + directionalLights.ToString() + "].lightSpace"], true, ref lightSpace);

                    GL.Uniform1(shader.locations["directionalLights[" + directionalLights.ToString() + "].shadowTex"], i + 1);
                    GL.ActiveTexture(TextureUnit.Texture1 + i);
                    GL.BindTexture(TextureTarget.Texture2D, curLight.shadowTex);

                    GL.Uniform3(shader.locations["directionalLights[" + directionalLights.ToString() + "].direction"], curLight.gameObject.transform.forward);
                    GL.Uniform1(shader.locations["directionalLights[" + directionalLights.ToString() + "].radius"], curLight.Radius);
                    GL.Uniform1(shader.locations["directionalLights[" + directionalLights.ToString() + "].brightness"], curLight.Brightness);
                    GL.Uniform3(shader.locations["directionalLights[" + directionalLights.ToString() + "].color"], lights[i].color.R, lights[i].color.G, lights[i].color.B);

                    directionalLights++;
                }
                else if (lights[i] is PointLight)
                {
                    PointLight curLight = lights[i] as PointLight;

                    Matrix4[] lightSpaces = curLight.lightSpaces;
                    float[] data = new float[16 * 6];
                    for (int mat = 0; i < 6; i++)
                        for (int y = 0; y < 4; y++)
                            for (int x = 0; x < 4; x++)
                                data[mat * 16 + y * 4 + x] = lightSpaces[mat][y, x];

                    GL.UniformMatrix4(shader.locations["pointLights[" + pointLights.ToString() + "].lightSpace"], 6, true, data);

                    //GL.Uniform1(loc_shadowCube + i * loc_offset, i + 1);
                    GL.ActiveTexture(TextureUnit.Texture1 + i);
                    GL.BindTexture(TextureTarget.TextureCubeMap, curLight.shadowCube);

                    GL.Uniform3(shader.locations["pointLights[" + pointLights.ToString() + "].position"], curLight.gameObject.transform.position);
                    GL.Uniform1(shader.locations["pointLights[" + pointLights.ToString() + "].radius"], curLight.Radius);
                    GL.Uniform1(shader.locations["pointLights[" + pointLights.ToString() + "].brightness"], curLight.Brightness);
                    GL.Uniform1(shader.locations["pointLights[" + pointLights.ToString() + "].intensity"], curLight.Intensity);
                    GL.Uniform3(shader.locations["pointLights[" + pointLights.ToString() + "].color"], lights[i].color.R, lights[i].color.G, lights[i].color.B);

                    pointLights++;
                }
                else
                {
                    AmbientLight curLight = lights[i] as AmbientLight;

                    GL.Uniform1(shader.locations["ambientLights[" + ambientLights.ToString() + "].brightness"], curLight.Brightness);
                    GL.Uniform3(shader.locations["ambientLights[" + ambientLights.ToString() + "].color"], lights[i].color.R, lights[i].color.G, lights[i].color.B);

                    ambientLights++;
                }
            }
            GL.Uniform1(shader.locations["ambientLightsCount"], ambientLights);
            GL.Uniform1(shader.locations["directionalLightsCount"], directionalLights);
            GL.Uniform1(shader.locations["spotLightsCount"], spotLights);
            GL.Uniform1(shader.locations["pointLightsCount"], pointLights);

            Matrix4 proj = Camera.Current.proj;
            Matrix4 view = Camera.Current.view;
            Matrix4 model;
            GL.UniformMatrix4(shader.locations["proj"], true, ref proj);
            GL.UniformMatrix4(shader.locations["view"], true, ref view);

            GL.Uniform3(shader.locations["camPos"], Camera.Current.gameObject.transform.position);
            GL.Uniform1(shader.locations["spot_near"], SpotLight.NEAR);

            GL.Uniform1(shader.locations["tex"], 0);
            GL.ActiveTexture(TextureUnit.Texture0);

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
        private void renderDepthBuffer(int depthBuffer, float near, float far)
        {
            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);
            GL.Viewport(0, 0, Width, Height);

            GL.UseProgram(AssetsManager.Shaders["quad"].id);

            float[] data = new float[] { -1f, 1f, 0f, 0f,
                                         -1f, -1f, 0f, 1f,
                                         1f, -1f, 1f, 1f,
                                         1f, 1f, 1f, 0f };

            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * 4, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * 4, 2 * 4);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * 4, data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.Uniform1(AssetsManager.Shaders["quad"].locations["near"], near);
            GL.Uniform1(AssetsManager.Shaders["quad"].locations["far"], far);

            GL.BindTexture(TextureTarget.Texture2D, depthBuffer);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.BindVertexArray(0);
            GL.DeleteBuffer(VBO);
            GL.DeleteVertexArray(VAO);

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
