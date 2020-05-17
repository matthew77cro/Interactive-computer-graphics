using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Mandelbrot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Window().Run();
        }
    }

    class Window : GameWindow
    {

        MandelbrotParams param;
        bool quadratic = true;
        bool color = false;
        Stack<MandelbrotParams> pStack = new Stack<MandelbrotParams>();

        public Window()
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV4Z10")
        {
            param = new MandelbrotParams
            {
                uMin = -2,
                uMax = 1,
                vMin = -1.2,
                vMax = 1.2,
                maxLimit = 128
            };
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Number1)
            {
                quadratic = true;
            }
            else if (e.Key == Key.Number2)
            {
                quadratic = false;
            }
            else if (e.Key == Key.B)
            {
                color = false;
            }
            else if (e.Key == Key.C)
            {
                color = true;
            }
            else if (e.Key == Key.X)
            {
                if (pStack.Count != 0)
                    param = pStack.Pop();
            }
            else if (e.Key == Key.Escape)
            {
                param = new MandelbrotParams
                {
                    uMin = -2,
                    uMax = 1,
                    vMin = -1.2,
                    vMax = 1.2,
                    maxLimit = 128
                };
                pStack.Clear();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left)
                return;

            pStack.Push(param);

            double u = (param.uMax - param.uMin) / 32;
            double v = (param.vMax - param.vMin) / 32;
            int ml = param.maxLimit;

            Complex mousePos = FromScreenToComplex(e.X, e.Y);

            param = new MandelbrotParams()
            {
                uMax = mousePos.re + u,
                uMin = mousePos.re - u,
                vMax = mousePos.im + v,
                vMin = mousePos.im - v,
                maxLimit = ml
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();
            //crtanje scene :
            RenderScene();
            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width - 1, 0, Height - 1, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void RenderScene()
        {

            GL.PointSize(1.0f);

            GL.Begin(PrimitiveType.Points);
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {

                    Complex c = FromScreenToComplex(i, j);
                    int n = DivergenceTest(c, param.maxLimit);
                    ColorScheme2(n);
                    GL.Vertex2(i, j);

                }
            }
            GL.End();

        }

        private Complex FromScreenToComplex(int x, int y)
        {
            return new Complex()
            {
                re = (x / (double) (Width - 1)) * (param.uMax - param.uMin) + param.uMin,
                im = (y / (double) (Height - 1)) * (param.vMax - param.vMin) + param.vMin
            };
        }

        private int DivergenceTest(Complex c, int limit)
        {            
            Complex z = new Complex();
            z.re = 0; z.im = 0;

            for (int i = 1; i <= limit; i++) {
                double next_re, next_im;
                if (quadratic)
                {
                    next_re = z.re * z.re - z.im * z.im + c.re;
                    next_im = 2 * z.re * z.im + c.im;
                }
                else
                {
                    next_re = z.re * z.re * z.re - 3 * z.re * z.im * z.im + c.re;
                    next_im = 3 * z.re * z.re * z.im - z.im * z.im * z.im + c.im;
                }
                z.re = next_re;
                z.im = next_im;
                double modul2 = z.re * z.re + z.im * z.im;
                if (modul2 > 4) return i;
            }

            return -1;
        }

        private void ColorScheme2(int n) 
        {
            if (!color)
            {
                if (n == -1)
                {
                    GL.Color3(0f, 0f, 0f);
                }
                else
                {
                    GL.Color3(1f, 1f, 1f);
                }
            }
            else
            {
                if (n == -1)
                {
                    GL.Color3(0f, 0f, 0f);
                }
                else if (param.maxLimit < 16)
                {
                    int r = (int)((n - 1) / (double)(param.maxLimit - 1) * 255 + 0.5);
                    int g = 255 - r;
                    int b = ((n - 1) % (param.maxLimit / 2)) * 255 / (param.maxLimit / 2);
                    GL.Color3((float)(r / 255f), (float)(g / 255f), (float)(b / 255f));
                }
                else
                {
                    int lim = param.maxLimit < 32 ? param.maxLimit : 32;
                    int r = (n - 1) * 255 / lim;
                    int g = ((n - 1) % (lim / 4)) * 255 / (lim / 4);
                    int b = ((n - 1) % (lim / 8)) * 255 / (lim / 8);
                    GL.Color3((float)(r / 255f), (float)(g / 255f), (float)(b / 255f));
                }
            }
        }

    }

    struct MandelbrotParams
    {
        public double uMin;
        public double uMax;
        public double vMin;
        public double vMax;
        public int maxLimit;
    }

    class Complex
    {
        public double re;
        public double im;
    }

}
