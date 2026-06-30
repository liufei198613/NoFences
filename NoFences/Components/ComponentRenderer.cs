using System;
using System.Collections.Generic;
using System.Drawing;

namespace NoFences.Components
{
    public class ComponentRenderer
    {
        private List<IComponent> components = new List<IComponent>();

        public void AddComponent(IComponent component)
        {
            component.Initialize();
            components.Add(component);
        }

        public void RemoveComponent(string componentId)
        {
            var component = FindComponent(componentId);
            if (component != null)
            {
                components.Remove(component);
            }
        }

        public IComponent FindComponent(string componentId)
        {
            foreach (var component in components)
            {
                if (component.Id == componentId)
                    return component;
            }
            return null;
        }

        public IComponent GetComponentAt(Point location)
        {
            foreach (var component in components)
            {
                if (component.Visible && component.Bounds.Contains(location))
                    return component;
            }
            return null;
        }

        public void RenderAll(Graphics g)
        {
            foreach (var component in components)
            {
                if (component.Visible)
                {
                    component.Render(g, component.Bounds);
                }
            }
        }

        public List<IComponent> GetAllComponents()
        {
            return new List<IComponent>(components);
        }
    }
}
