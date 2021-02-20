using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;

namespace CG_DefaultGraphics
{
    public class MainWindow : GameWindow
    {
        public MainWindow() : base(800, 600, GraphicsMode.Default, "Computer graphics")
        {
            WindowState = WindowState.Maximized;
        }
        protected override void OnLoad(EventArgs e)
        {

        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {

        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {

        }
        protected override void OnUnload(EventArgs e)
        {

        }
    }
}
