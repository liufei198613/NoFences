using System;
using System.Drawing;

namespace NoFences.Components
{
    public interface IComponent
    {
        string Id { get; }
        Model.ComponentType Type { get; }
        Rectangle Bounds { get; set; }
        bool Visible { get; set; }

        void Initialize();
        void Render(System.Drawing.Graphics g, Rectangle bounds);
        void HandleMouseDown(Point location);
        void HandleMouseMove(Point location);
        void HandleMouseUp(Point location);
        Size GetPreferredSize();
        void SaveState();
        void LoadState();
    }
}
