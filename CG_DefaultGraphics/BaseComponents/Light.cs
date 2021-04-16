using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Graphics;
using OpenTK;

namespace CG_DefaultGraphics.BaseComponents
{
    //public enum LightType
    //{
    //    Ambient = 0,
    //    Directional = 1,
    //    Point = 2,
    //    Spot = 3
    //}
    public abstract class Light : Component
    {
        public Color4 color = Color4.White;
        protected float brightness = 1.0f;
        public float Brightness
        {
            get => brightness;
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException("Brightness", "Brightness can't be negative or more than 1");
                brightness = value;
            }
        }
    }
    public class AmbientLight : Light
    {

    }
    public class DirectionalLight : Light
    {
        private float radius = 1.0f;
        public float Radius
        {
            get => radius;
            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("Radius", "Radius can't be negative");
                radius = value;
            }
        }
        public static readonly int SHADOW_SIZE = 4096;
        public int FBO { get; private set; } = 0;
        public int shadowTex { get; private set; } = 0;
        public Matrix4 lightSpace
        {
            get
            {
                Vector3 r = gameObject.transform.right / radius;
                Vector3 u = gameObject.transform.up / radius;
                Vector3 f = gameObject.transform.forward / radius;
                Vector3 p = -Camera.Current.gameObject.transform.position;
                Matrix4 mat = new Matrix4(r.X, r.Y, r.Z, Vector3.Dot(p, r),
                                          u.X, u.Y, u.Z, Vector3.Dot(p, u),
                                          f.X, f.Y, f.Z, Vector3.Dot(p, f),
                                          0f, 0f, 0f, 1f);
                return mat;
            }
        }
        public DirectionalLight()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            shadowTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadowTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowTex, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
    public class SpotLight : Light
    {
        private float radius = 1.0f;
        public float Radius
        {
            get => radius;
            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("Radius", "Radius can't be negative");
                radius = value;
            }
        }
        private float intensity = 0.0f;
        public float Intensity
        {
            get => intensity;
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException("Intensity", "Intensity can't be negative or more than 1");
                intensity = value;
            }
        }
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
        public static readonly float NEAR = 0.001f;
        public static readonly int SHADOW_SIZE = 2048;
        public int FBO { get; private set; } = 0;
        public int shadowTex { get; private set; } = 0;
        public Matrix4 lightSpace
        {
            get
            {
                float ctg = 1f / (float)Math.Tan(angle / 2f);
                Vector3 r = gameObject.transform.right * ctg;
                Vector3 u = gameObject.transform.up * ctg;
                Vector3 f = gameObject.transform.forward;
                Vector3 p = -gameObject.transform.position;
                float val = (radius + NEAR) / (radius - NEAR);
                Matrix4 mat = new Matrix4(r.X, r.Y, r.Z, Vector3.Dot(p, r),
                                          u.X, u.Y, u.Z, Vector3.Dot(p, u),
                                          f.X * val, f.Y * val, f.Z * val, Vector3.Dot(p, f) * val - 2f * radius * NEAR / (radius - NEAR),
                                          f.X, f.Y, f.Z, Vector3.Dot(p, f));
                return mat;
            }
        }
        public SpotLight()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            shadowTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, shadowTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowTex, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
    public class PointLight : Light
    {
        private float radius = 1.0f;
        public float Radius
        {
            get => radius;
            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("Radius", "Radius can't be negative");
                radius = value;
            }
        }
        private float intensity = 0.0f;
        public float Intensity
        {
            get => intensity;
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException("Intensity", "Intensity can't be negative or more than 1");
                intensity = value;
            }
        }
        public static readonly float NEAR = 0.001f;
        public static readonly int SHADOW_SIZE = 1024;
        public int FBO { get; private set; } = 0;
        public int shadowCube { get; private set; } = 0;
        public Matrix4[] lightSpaces
        {
            get
            {
                float ctg = 1f / (float)Math.Tan(Math.PI / 4f);
                Vector3 r = gameObject.transform.right;
                Vector3 u = gameObject.transform.up;
                Vector3 f = gameObject.transform.forward;
                Vector3 p = -gameObject.transform.position;
                Matrix4 proj = new Matrix4(ctg, 0f, 0f, 0f,
                                           0f, ctg, 0f, 0f,
                                           0f, 0f, (radius + NEAR) / (radius - NEAR), -2f * radius * NEAR / (radius - NEAR),
                                           0f, 0f, 1f, 0f);
                Matrix4[] matrixes = new Matrix4[6];
                matrixes[0] = new Matrix4(r.X, r.Y, r.Z, Vector3.Dot(r, p),
                                          u.X, u.Y, u.Z, Vector3.Dot(u, p),
                                          f.X, f.Y, f.Z, Vector3.Dot(f, p),
                                          0f, 0f, 0f, 1f) * proj;
                matrixes[1] = new Matrix4(-r.X, -r.Y, -r.Z, -Vector3.Dot(r, p),
                                          u.X, u.Y, u.Z, Vector3.Dot(u, p),
                                          -f.X, -f.Y, -f.Z, -Vector3.Dot(f, p),
                                          0f, 0f, 0f, 1f) * proj;
                matrixes[2] = new Matrix4(-f.X, -f.Y, -f.Z, -Vector3.Dot(f, p),
                                          u.X, u.Y, u.Z, Vector3.Dot(u, p),
                                          r.X, r.Y, r.Z, Vector3.Dot(r, p),
                                          0f, 0f, 0f, 1f) * proj;
                matrixes[3] = new Matrix4(f.X, f.Y, f.Z, Vector3.Dot(f, p),
                                          u.X, u.Y, u.Z, Vector3.Dot(u, p),
                                          -r.X, -r.Y, -r.Z, Vector3.Dot(-r, p),
                                          0f, 0f, 0f, 1f) * proj;
                matrixes[4] = new Matrix4(-r.X, -r.Y, -r.Z, -Vector3.Dot(r, p),
                                          f.X, f.Y, f.Z, Vector3.Dot(f, p),
                                          u.X, u.Y, u.Z, Vector3.Dot(u, p),
                                          0f, 0f, 0f, 1f) * proj;
                matrixes[5] = new Matrix4(r.X, r.Y, r.Z, Vector3.Dot(r, p),
                                          f.X, f.Y, f.Z, Vector3.Dot(f, p),
                                          -u.X, -u.Y, -u.Z, -Vector3.Dot(u, p),
                                          0f, 0f, 0f, 1f) * proj;
                return matrixes;
            }
        }
        public PointLight()
        {
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            shadowCube = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, shadowCube);
            for (int i = 0; i < 6; i++)
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.DepthComponent, SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, shadowCube, 0);

            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
