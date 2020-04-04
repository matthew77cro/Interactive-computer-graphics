using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace PrviOpenGL
{
    class Primjer1 : GameWindow
    {
        static void Main(string[] args)
        {
            GameWindow w = new Primjer1();
            w.Run();
        }

        public Primjer1() 
            : base(200, 200, OpenTK.Graphics.GraphicsMode.Default, "Primjer1")
        {
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();
            //crtanje scene :
            renderScene();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width - 1, Height - 1, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void renderScene()
        {
            GL.PointSize(1.0f);
            GL.Color3(0.0f, 1.0f, 1.0f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(0, 0);
            GL.Vertex2(2, 2);
            GL.Vertex2(4, 4);
            GL.End();
            GL.Begin(PrimitiveType.LineStrip);
            GL.Vertex2(50, 50);
            GL.Vertex2(150, 150);
            GL.Vertex2(50, 150);
            GL.Vertex2(50, 50);
            GL.End();
        }
    }
}
