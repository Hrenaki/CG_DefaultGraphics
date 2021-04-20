using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CG_DefaultGraphics.BaseComponents;

namespace CG_DefaultGraphics
{
    public class GameObject
    {
        public Transform transform { get; }
        internal List<Component> components = new List<Component>();
        public GameObject()
        {
            transform = new Transform();
            transform.gameObject = this;
            components.Add(transform);
        }
        public Component addComponent<T>()
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
                throw new ArgumentException("Given type must be a component");
            Component component = Activator.CreateInstance(typeof(T)) as Component;
            component.gameObject = this;
            components.Add(component);
            return component;
        }
        public Component addComponent(Type t)
        {
            if (!t.IsSubclassOf(typeof(Component)))
                throw new ArgumentException("Given type must be a component");
            Component component = Activator.CreateInstance(t) as Component;
            component.gameObject = this;
            components.Add(component);
            return component;
        }
        public Component getComponent<T>()
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
                throw new ArgumentException("Given type must be a component");
            foreach (Component component in components)
                if (component is T)
                    return component;
            return null;
        }
        public Component[] getComponents<T>()
        {
            if (!typeof(T).IsSubclassOf(typeof(Component)))
                throw new ArgumentException("Given type must be a component");
            List<Component> curComponents = new List<Component>();
            foreach (Component component in components)
                if (component is T)
                    curComponents.Add(component);
            return curComponents.ToArray();
        }
    }
}
