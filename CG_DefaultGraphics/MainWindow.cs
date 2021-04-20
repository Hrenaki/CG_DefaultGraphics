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
        private class FrameBuffer
        {
            public int id;
            public List<int> textures = new List<int>();
        }
        private List<GameObject> objects = new List<GameObject>();
        private Shader shader;
        private int HDRFBO;
        private int HDRTexture;
        private int bloomTexture;
        private int quadVAO;
        private float exposure = 2.0f;
        private List<FrameBuffer> FBOs = new List<FrameBuffer>();
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
            GL.CullFace(CullFaceMode.Back);

            setupFBOs();
            setupQuad();

            shader = AssetsManager.LoadShader("default", new ShaderComponent("Assets\\Shaders\\default.vsh"), new ShaderComponent("Assets\\Shaders\\default.fsh"));
            AssetsManager.LoadShader("light_directional", new ShaderComponent("Assets\\Shaders\\light_directional.vsh"), new ShaderComponent("Assets\\Shaders\\light_directional.fsh"));
            AssetsManager.LoadShader("light_point", new ShaderComponent("Assets\\Shaders\\light_point.vsh"), new ShaderComponent("Assets\\Shaders\\light_point.gsh"), new ShaderComponent("Assets\\Shaders\\light_point.fsh"));
            AssetsManager.LoadShader("postProcessing", new ShaderComponent("Assets\\Shaders\\postProcessing.vsh"), new ShaderComponent("Assets\\Shaders\\postProcessing.fsh"));
            AssetsManager.LoadShader("bloom", new ShaderComponent("Assets\\Shaders\\bloom.vsh"), new ShaderComponent("Assets\\Shaders\\bloom.fsh"));

            AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", 1.0f, true);
            AssetsManager.LoadTexture("Assets\\Textures\\default_white.png", "", true);
            AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj", 5f, true);
            AssetsManager.LoadTexture("Assets\\Textures\\template.png", "", true);

            Scene scene = AssetsManager.LoadScene("Assets\\Scenes\\scene1.xml");
            objects = scene.objects;
            scene.MainCamera.MakeCurrent();
        }
        private void setupFBOs()
        {
            HDRFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, HDRFBO);

            HDRTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, HDRTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, HDRTexture, 0);

            bloomTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, bloomTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, bloomTexture, 0);

            int HDRDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, HDRDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, HDRDepth);

            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            for (int i = 0; i < 2; i++)
            {
                FrameBuffer FBO = new FrameBuffer();
                FBO.id = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO.id);

                FBO.textures.Add(GL.GenTexture());
                GL.BindTexture(TextureTarget.Texture2D, FBO.textures[0]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FBO.textures[0], 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                FBOs.Add(FBO);
            }
        }
        private void setupQuad()
        {
            float[] data = new float[] { -1f, 1f, 0f, 1f,
                                         -1f, -1f, 0f, 0f,
                                         1f, -1f, 1f, 0f,
                                         1f, 1f, 1f, 1f };

            quadVAO = GL.GenVertexArray();
            GL.BindVertexArray(quadVAO);

            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * 4, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * 4, 2 * 4);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);

            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * 4, data, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            renderLights();
            renderScene();
            renderBloom();
            renderPostProcessing();
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

            Shader lightShader;

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] is AmbientLight)
                    continue;

                if (lights[i] is SpotLight)
                {
                    SpotLight curLight = lights[i] as SpotLight;
                    lightShader = shader_directional;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, SpotLight.SHADOW_SIZE, SpotLight.SHADOW_SIZE);

                    Matrix4 lightSpace = curLight.lightSpace;

                    GL.UseProgram(lightShader.id);

                    GL.UniformMatrix4(lightShader.locations["lightSpace"], true, ref lightSpace);
                }
                else if (lights[i] is DirectionalLight)
                {
                    DirectionalLight curLight = lights[i] as DirectionalLight;
                    lightShader = shader_directional;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, SpotLight.SHADOW_SIZE, SpotLight.SHADOW_SIZE);

                    Matrix4 lightSpace = curLight.lightSpace;

                    GL.UseProgram(lightShader.id);

                    GL.UniformMatrix4(lightShader.locations["lightSpace"], true, ref lightSpace);
                } else
                {
                    PointLight curLight = lights[i] as PointLight;
                    lightShader = shader_point;

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, curLight.FBO);
                    GL.Clear(ClearBufferMask.DepthBufferBit);

                    GL.Viewport(0, 0, PointLight.SHADOW_SIZE, PointLight.SHADOW_SIZE);

                    GL.UseProgram(lightShader.id);

                    Matrix4[] lightSpaces = curLight.lightSpaces;
                    for (int mat = 0; mat < 6; mat++)
                        GL.UniformMatrix4(lightShader.locations["lightSpaces"] + mat, true, ref lightSpaces[mat]);
                    GL.Uniform3(lightShader.locations["lightPos"], curLight.gameObject.transform.position);
                    GL.Uniform1(lightShader.locations["radius"], curLight.Radius);
                }

                foreach (GameObject obj in objects)
                {
                    Mesh[] meshs = obj.getComponents<Mesh>().Cast<Mesh>().ToArray();
                    if (meshs.Length > 0)
                    {
                        model = obj.transform.model;
                        GL.UniformMatrix4(lightShader.locations["model"], true, ref model);
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
        }
        private void renderScene()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, HDRFBO);

            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);
            GL.Viewport(0, 0, Width, Height);

            GL.UseProgram(shader.id);

            List<Light> lights = new List<Light>();
            foreach (GameObject obj in objects)
                lights.AddRange(obj.getComponents<Light>().Cast<Light>());

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

                    GL.Uniform1(shader.locations["pointLights[" + pointLights.ToString() + "].shadowCube"], i + 1);
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

            Matrix4 camSpace = Camera.Current.camSpace;
            Matrix4 model;
            GL.UniformMatrix4(shader.locations["camSpace"], true, ref camSpace);

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
                        GL.Uniform3(shader.locations["material.ambient"], mesh.material.ambient.R, mesh.material.ambient.G, mesh.material.ambient.B);
                        GL.Uniform3(shader.locations["material.diffuse"], mesh.material.diffuse.R, mesh.material.diffuse.G, mesh.material.diffuse.B);
                        GL.Uniform3(shader.locations["material.specular"], mesh.material.specular.R, mesh.material.specular.G, mesh.material.specular.B);
                        GL.Uniform1(shader.locations["material.metallic"], mesh.material.metallic);
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

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void renderBloom()
        {
            Shader blurShader = AssetsManager.Shaders["bloom"];
            GL.UseProgram(blurShader.id);
            int horizontal = 0;
            bool first = true;
            int i;
            GL.ActiveTexture(TextureUnit.Texture0);
            for (i = 0; i < 10; i++)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBOs[horizontal].id);
                GL.Uniform1(blurShader.locations["horizontal"], horizontal);
                GL.BindTexture(TextureTarget.Texture2D, first ? bloomTexture : FBOs[1 - horizontal].textures[0]);
                GL.BindVertexArray(quadVAO);
                GL.DrawArrays(PrimitiveType.Quads, 0, 4);
                horizontal = 1 - horizontal;
                first = false;
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            if (i % 2 == 0)
            {
                FrameBuffer tmp = FBOs[0];
                FBOs[0] = FBOs[1];
                FBOs[1] = tmp;
            }
        }
        private void renderPostProcessing()
        {
            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);
            GL.Viewport(0, 0, Width, Height);

            Shader postProcessingShader = AssetsManager.Shaders["postProcessing"];

            GL.UseProgram(postProcessingShader.id);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.Uniform1(postProcessingShader.locations["tex"], 1);
            GL.BindTexture(TextureTarget.Texture2D, HDRTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.Uniform1(postProcessingShader.locations["bloomTex"], 2);
            GL.BindTexture(TextureTarget.Texture2D, FBOs[0].textures[0]);

            GL.Uniform1(postProcessingShader.locations["exposure"], exposure);
            //GL.Uniform1(postProcessingShader.locations["time"], (float)Time.TotalTime);

            GL.BindVertexArray(quadVAO);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            SwapBuffers();

            //float[] pixels = new float[3 * Width * Height / 4];
            //GL.ReadPixels(Width / 4, Height / 4, Width / 2, Height / 2, PixelFormat.Rgb, PixelType.Float, pixels);
            //float total = 0f;
            //for (int i = 0; i < pixels.Length; i++)
            //    total += pixels[i];
            //total /= pixels.Length;
            //exposure += (0.4f - total) * 0.1f;
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Time.DeltaTime = e.Time;
            Time.TotalTime += e.Time;
            Input.OnUpdateFrame();

            foreach (GameObject obj in objects)
                foreach (Component component in obj.components)
                    component.update();

            if (Input.IsKeyDown(Key.E))
            {
                if (Input.IsKeyDown(Key.KeypadPlus))
                    exposure += (float)Time.DeltaTime * exposure;
                if (Input.IsKeyDown(Key.KeypadMinus))
                    exposure -= (float)Time.DeltaTime * exposure;
            }
            if (Input.IsKeyDown(Key.F))
            {
                if (Input.IsKeyDown(Key.KeypadPlus))
                    Camera.Current.FOV += (float)Time.DeltaTime * Camera.Current.FOV;
                if (Input.IsKeyDown(Key.KeypadMinus))
                    Camera.Current.FOV -= (float)Time.DeltaTime * Camera.Current.FOV;
            }

            if (Input.IsKeyPressed(Key.Escape))
                Exit();
        }
        protected override void OnUnload(EventArgs e)
        {

        }
    }
}
