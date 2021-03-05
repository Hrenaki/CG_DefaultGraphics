using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CG_DefaultGraphics.BaseComponents
{
    public abstract class Component
    {
        public GameObject gameObject;
        public virtual void update()
        {

        }
    }
}
