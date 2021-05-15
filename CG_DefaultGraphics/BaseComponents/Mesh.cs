using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;

namespace CG_DefaultGraphics.BaseComponents
{
    public class Mesh : Component
    {
        public Model model;
        public Texture texture;
        public Material material = Material.Default;
    }
    public class Material
    {
        public static Material Default { get { return new Material(Color4.White, Color4.White, Color4.White, 1.0f); } }
        public Color4 ambient;
        public Color4 diffuse;
        public Color4 specular;
        public float metallic;
        public Material()
        {
            ambient = Color4.White;
            diffuse = Color4.White;
            specular = Color4.White;
            metallic = 32f;
        }
        public Material(Color4 ambient, Color4 diffuse, Color4 specular, float metallic)
        {
            this.ambient = ambient;
            this.diffuse = diffuse;
            this.specular = specular;
            this.metallic = metallic;
        }
    }
}
