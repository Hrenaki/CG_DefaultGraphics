using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;

namespace CG_DefaultGraphics
{
    public class Model
    {
        public List<Vector3> v = null;
        public List<Vector2> t = null;
        public List<Vector3> n = null;
        public List<int[]> v_i = null;
        public List<int[]> t_i = null;
        public List<int[]> n_i = null;

        public int VAO = 0;
        public void updateVAO()
        {
            if (v == null || v_i == null)
                throw new Exception("Model can't be empty.");

            List<float> data = new List<float>();
            float[] curdata = new float[8];
            int polygonsCount = v_i.Count;
            uint[] indexes = new uint[3 * polygonsCount];
            for (int i = 0; i < polygonsCount; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    curdata[0] = v[v_i[i][j]].X;
                    curdata[1] = v[v_i[i][j]].Y;
                    curdata[2] = v[v_i[i][j]].Z;
                    if (t != null)
                    {
                        curdata[3] = t[t_i[i][j]].X;
                        curdata[4] = t[t_i[i][j]].Y;
                    }
                    else
                    {
                        curdata[3] = 0;
                        curdata[4] = 0;
                    }
                    if (n != null)
                    {
                        curdata[5] = n[n_i[i][j]].X;
                        curdata[6] = n[n_i[i][j]].Y;
                        curdata[7] = n[n_i[i][j]].Z;
                    }
                    else
                    {
                        Vector3 p1 = v[v_i[i][0]];
                        Vector3 p2 = v[v_i[i][1]];
                        Vector3 p3 = v[v_i[i][2]];
                        Vector3 normal = Vector3.Cross(p3 - p1, p2 - p1).Normalized();
                        curdata[5] = normal.X;
                        curdata[6] = normal.Y;
                        curdata[7] = normal.Z;
                    }
                    int curVertexesCount = data.Count / 8;
                    uint k;
                    for (k = 0; k < curVertexesCount; k++)
                    {
                        uint w;
                        for (w = 0; w < 8; w++)
                        {
                            if (data[(int)(k * 8 + w)] != curdata[w])
                                break;
                        }
                        if (w == 8)
                        {
                            indexes[i * 3 + j] = k;
                            break;
                        }
                    }
                    if (k == curVertexesCount)
                    {
                        indexes[i * 3 + j] = k;
                        data.AddRange(curdata);
                    }
                }
            }

            if (VAO != 0)
                GL.DeleteVertexArray(VAO);
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            int VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * 4, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * 4, 3 * 4);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * 4, 5 * 4);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BufferData(BufferTarget.ArrayBuffer, data.Count * 4, data.ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            int EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indexes.Length * 4, indexes, BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
    public class Texture
    {
        public Bitmap image;
        public int id;
        public Texture(Bitmap image)
        {
            this.image = image;

            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, new int[] { (int)TextureWrapMode.ClampToBorder });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, new int[] { (int)TextureWrapMode.ClampToBorder });

            int height = image.Height;
            int width = image.Width;
            byte[,] data = new byte[height, width * 4];
            Color color;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    color = image.GetPixel(j, i);
                    data[i, j * 4] = color.R;
                    data[i, j * 4 + 1] = color.G;
                    data[i, j * 4 + 2] = color.B;
                    data[i, j * 4 + 3] = color.A;
                }

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
    public class Shader
    {
        public int id;
        public Dictionary<string, int> locations;
        public Shader(int id)
        {
            this.id = id;
            locations = new Dictionary<string, int>();
            int uniformsCount;
            GL.GetProgram(id, GetProgramParameterName.ActiveUniforms, out uniformsCount);
            for (int i = 0; i < uniformsCount; i++)
            {
                string uniformName = GL.GetActiveUniform(id, i, out _, out _);
                if (uniformName.EndsWith("[0]"))
                    uniformName = uniformName.Substring(0, uniformName.Length - 3);
                locations[uniformName] = GL.GetUniformLocation(id, uniformName);
            }
        }
    }

    public static class AssetsManager
    {
        public static Dictionary<string, Model> Models = new Dictionary<string, Model>();
        public static Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();
        public static Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();
        public static Dictionary<string, Model> LoadModelsFile(string path, bool reverse = false)
        {
            StreamReader reader = new StreamReader(File.OpenRead(path));

            Dictionary<string, Model> models = new Dictionary<string, Model>();

            string modelName = Path.GetFileNameWithoutExtension(path);
            Model model = new Model();
            string line;
            int offset_v = 0;
            int offset_t = 0;
            int offset_n = 0;
            while ((line = reader.ReadLine()) != null)
            {
                string[] words = line.Replace('.', ',').Split(' ');
                if (words.Length == 0)
                    continue;
                switch (words[0])
                {
                    case "o":
                        if (model.v != null)
                        {
                            models[modelName] = model;
                            Models[modelName] = model;
                            offset_v += model.v.Count;
                            if (model.t != null)
                                offset_t += model.t.Count;
                            if (model.n != null)
                                offset_n += model.n.Count;
                            model = new Model();
                        }
                        modelName = line.Substring(2);
                        break;
                    case "v":
                        if (model.v == null)
                        {
                            model.v = new List<Vector3>();
                            model.v_i = new List<int[]>();
                        }
                        model.v.Add(new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3])));
                        break;
                    case "vt":
                        if (model.t == null)
                        {
                            model.t = new List<Vector2>();
                            model.t_i = new List<int[]>();
                        }
                        model.t.Add(new Vector2(float.Parse(words[1]), float.Parse(words[2])));
                        break;
                    case "vn":
                        if (model.n == null)
                        {
                            model.n = new List<Vector3>();
                            model.n_i = new List<int[]>();
                        }
                        model.n.Add(new Vector3(float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3])));
                        break;
                    case "f":
                        int vertexesCount = words.Length - 1;
                        int[] v_i = new int[vertexesCount];
                        int[] t_i = null;
                        int[] n_i = null;
                        if (model.t_i != null)
                            t_i = new int[vertexesCount];
                        if (model.n_i != null)
                            n_i = new int[vertexesCount];
                        if (reverse)
                            for (int i = 0; i < vertexesCount; i++)
                            {
                                string[] values = words[1 + i].Split('/');
                                v_i[vertexesCount - i - 1] = int.Parse(values[0]) - offset_v - 1;
                                if (t_i != null)
                                    t_i[vertexesCount - i - 1] = int.Parse(values[1]) - offset_t - 1;
                                if (n_i != null)
                                    n_i[vertexesCount - i - 1] = int.Parse(values[2]) - offset_n - 1;
                            }
                        else
                            for (int i = 0; i < vertexesCount; i++)
                            {
                                string[] values = words[1 + i].Split('/');
                                v_i[i] = int.Parse(values[0]) - offset_v - 1;
                                if (t_i != null)
                                    t_i[i] = int.Parse(values[1]) - offset_t - 1;
                                if (n_i != null)
                                    n_i[i] = int.Parse(values[2]) - offset_n - 1;
                            }
                        for (int i = 1; i < vertexesCount - 1; i++)
                        {
                            model.v_i.Add(new int[3] { v_i[0], v_i[i], v_i[i + 1] });
                            if (t_i != null)
                                model.t_i.Add(new int[3] { t_i[0], t_i[i], t_i[i + 1] });
                            if (n_i != null)
                                model.n_i.Add(new int[3] { n_i[0], n_i[i], n_i[i + 1] });
                        }
                        break;
                }
            }
            if (model.v != null)
            {
                models[modelName] = model;
                Models[modelName] = model;
            }
            foreach (Model mdl in models.Values)
                mdl.updateVAO();
            return models;
        }
        public static Shader LoadShader(string shaderName, string vertexShaderPath, string fragmentShaderPath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(vertexShaderPath)) || !File.Exists(vertexShaderPath))
                throw new FileNotFoundException("Vertex shader file not found", vertexShaderPath);
            if (!Directory.Exists(Path.GetDirectoryName(fragmentShaderPath)) || !File.Exists(fragmentShaderPath))
                throw new FileNotFoundException("Fragment shader file not found", fragmentShaderPath);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertexShaderPath));
            GL.CompileShader(vertexShader);
            int result;
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out result);
            if (result == 0)
                throw new Exception("Vertex shader compilation error: " + GL.GetShaderInfoLog(vertexShader));

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentShaderPath));
            GL.CompileShader(fragmentShader);
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out result);
            if (result == 0)
                throw new Exception("Fragment shader compilation error: " + GL.GetShaderInfoLog(fragmentShader));

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out result);
            if (result == 0)
                throw new Exception("Program linking error: " + GL.GetProgramInfoLog(program));

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            Shader shader = new Shader(program);
            Shaders[shaderName] = shader;
            return shader;
        }
        public static Texture LoadTexture(string path, string textureName = "")
        {
            if (textureName == "")
                textureName = Path.GetFileNameWithoutExtension(path);

            Texture texture = new Texture(new Bitmap(path));

            Textures[textureName] = texture;

            return texture;
        }
    }
}
