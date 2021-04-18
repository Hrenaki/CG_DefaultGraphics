﻿using System;
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
using System.Xaml;

namespace CG_DefaultGraphics
{
    public class MainWindow : GameWindow
    {
        private List<GameObject> objects = new List<GameObject>();
        private Shader shader;
        private int HDRFBO;
        private int HDRTexture;
        private int quadVAO;
        private float exposure = 2.0f;
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

            HDRFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, HDRFBO);

            HDRTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, HDRTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, HDRTexture, 0);

            int HDRDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, HDRDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, HDRDepth);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Frame buffer is not complete.");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

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

            shader = AssetsManager.LoadShader("default", new ShaderComponent("Assets\\Shaders\\default.vsh"), new ShaderComponent("Assets\\Shaders\\default.fsh"));
            AssetsManager.LoadShader("light_directional", new ShaderComponent("Assets\\Shaders\\light_directional.vsh"), new ShaderComponent("Assets\\Shaders\\light_directional.fsh"));
            AssetsManager.LoadShader("light_point", new ShaderComponent("Assets\\Shaders\\light_point.vsh"), new ShaderComponent("Assets\\Shaders\\light_point.gsh"), new ShaderComponent("Assets\\Shaders\\light_point.fsh"));
            AssetsManager.LoadShader("postProcessing", new ShaderComponent("Assets\\Shaders\\postProcessing.vsh"), new ShaderComponent("Assets\\Shaders\\postProcessing.fsh"));

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

            //AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj");
            //Model bigCube = AssetsManager.Models["cube"];
            //AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj");
            //AssetsManager.Models["bigCube"] = bigCube;

            GameObject diamond = new GameObject();
            Mesh diamondMesh = (Mesh)diamond.addComponent<Mesh>();
            //cubeMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", 1.0f, true)["diamondwhite_dmesh"];
            diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", 1.0f, true)["diamondwhite_dmesh"];
            diamondMesh.texture = AssetsManager.LoadTexture("Assets\\Textures\\default_white.png", "", true);
            diamondMesh.material.metallic = 76.8f;
            diamondMesh.material.ambient = new Color4(0.0215f, 0.1745f, 0.0215f, 1f);
            diamondMesh.material.diffuse = new Color4(0.07568f, 0.61424f, 0.07568f, 1f);
            diamondMesh.material.specular = new Color4(0.633f, 0.727811f, 0.633f, 1f);
            objects.Add(diamond);

            GameObject cube2 = new GameObject();
            Mesh cubeMesh2 = (Mesh)cube2.addComponent<Mesh>();
            //diamondMesh.model = AssetsManager.LoadModelsFile("Assets\\Models\\diamonds.obj", true)["diamondwhite_dmesh"];
            cubeMesh2.model = AssetsManager.LoadModelsFile("Assets\\Models\\cube.obj", 5f, true)["cube"];
            cubeMesh2.texture = AssetsManager.LoadTexture("Assets\\Textures\\template.png", "", true);
            objects.Add(cube2);

            GameObject ambientObj = new GameObject();
            AmbientLight ambient = (AmbientLight)ambientObj.addComponent<AmbientLight>();
            ambient.Brightness = 0.1f;
            objects.Add(ambientObj);

            GameObject directionalObj = new GameObject();
            directionalObj.transform.position.Y = 2.0f;
            //directionalObj.transform.rotation = Quaternion.FromAxisAngle(Vector3.UnitX, (float)Math.PI / 6f);
            PointLight directional = (PointLight)directionalObj.addComponent<PointLight>();
            directional.Brightness = 0.5f;
            directional.Radius = 20f;
            directional.Intensity = 0f;
            directional.color = Color4.White;
            directionalObj.addComponent<AutoFlyAround>();
            //directional.Angle = (float)Math.PI / 180f * 100f;
            objects.Add(directionalObj);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            renderLights();
            renderScene();
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
            //renderDepthBuffer((lights[0] as PointLight).shadowCube, PointLight.NEAR, (lights[0] as PointLight).Radius);
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

            //SwapBuffers();
        }
        private void renderPostProcessing()
        {
            GL.ClearColor(new Color4(0.2f, 0.2f, 0.2f, 0.2f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.CullFace(CullFaceMode.Back);
            GL.Viewport(0, 0, Width, Height);

            Shader postProcessingShader = AssetsManager.Shaders["postProcessing"];

            GL.UseProgram(postProcessingShader.id);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, HDRTexture);

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
