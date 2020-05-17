using System;
using System.Collections.Generic;
using IRGLinearAlgebra;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace AlgoritamPracenjaZrake
{
    class Program
    {
        static void Main(string[] args)
        {
            new Window(RTScene.UcitajScenu(args[0])).Run();

            /*
            // Intersection test :
            Sphere s = new Sphere();
            s.center = new Vector(new double[] { 2, 2.5, 0 });
            s.radius = 2.55;

            Intersection inter = new Intersection();
            s.UpdateIntersection(inter, new Vector(new double[] { -3, -2, 0 }), new Vector(new double[] { 4, 2, 1 }));

            // Should be : 1.53 0.26 1.13
            Console.WriteLine(inter.point[0] + " " + inter.point[1] + " " + inter.point[2]);

            inter = new Intersection();
            s.UpdateIntersection(inter, new Vector(new double[] { -3, -2, 0 }), new Vector(new double[] { 4.15, 0.84, 1 }));

            Console.WriteLine(inter.obj == null ? "OK" : "Error");

            Patch p = new Patch();
            p.center = new Vector(new double[] { -1, 1, 0 });
            p.v1 = new Vector(new double[] { -4.5, 1, 2 }).Normalize();
            p.v2 = new Vector(new double[] { -2, -1, 1 }).Normalize();
            p.normal = (p.v1 * p.v2).Normalize();
            p.w = 1;
            p.h = 1;

            inter = new Intersection();
            p.UpdateIntersection(inter, new Vector(new double[] { -2.5, 0, 0 }), new Vector(new double[] { 1, 1, 0.25 }));

            // Should be : -1.52 0.98 0.24
            Console.WriteLine(inter.point[0] + " " + inter.point[1] + " " + inter.point[2]);
            Console.ReadKey();
            */
        }
    }

    class Window : GameWindow
    {

        private const int MAX_DEPTH = 1;

        private RTScene scene;

        public Window(RTScene scene)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV4Z10")
        {
            this.scene = scene;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
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
            RayTrace();
            Console.WriteLine("Frame rendered");

        }

        private void RayTrace()
        {
            GL.Color3(1.0f, 1.0f, 1.0f);

            GL.Begin(PrimitiveType.Points);
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {

                    IVector rayStart = scene.eye;
                    IVector rayDir = GetDirectionForScreenCoords(i, j);

                    ColorVector cv = Follow(rayStart, rayDir, MAX_DEPTH);
                    GL.Color3(cv.r, cv.g, cv.b);

                    GL.Vertex2(i, j);

                }
            }
            GL.End();
        }

        private ColorVector Follow(IVector rayStart, IVector rayDir, int depth)
        {
            if (depth < 0)
                return new ColorVector();

            Intersection inter = new Intersection();
            foreach (var o in scene.objects)
            {
                o.UpdateIntersection(inter, rayStart, rayDir);
            }

            if (inter.obj != null)
            {
                return DetermineColor(inter, rayStart, rayDir, depth);
            }
            else
            {
                return new ColorVector();
            }
        }

        private ColorVector DetermineColor(Intersection inter, IVector rs, IVector rd, int depth)
        {
            ColorVector cv = new ColorVector();

            var normal = inter.obj.GetNormalInPoint(inter.point);

            cv.r += inter.obj.fambRGB[0] * scene.gaIntensity[0];
            cv.g += inter.obj.fambRGB[1] * scene.gaIntensity[1];
            cv.b += inter.obj.fambRGB[2] * scene.gaIntensity[2];

            foreach (var light in scene.sources)
            {

                IVector rayStart = inter.point;
                IVector dir = light.position - rayStart;

                double distance = dir.Norm();
                dir.Normalize();

                rayStart += 1E-4 * dir;

                Intersection i = new Intersection();
                foreach (var o in scene.objects)
                {
                    o.UpdateIntersection(i, rayStart, dir);
                }

                if (i.obj != null && i.lambda <= distance)
                    continue;

                double ldotn = dir.ScalarProduct(normal);

                cv.r += inter.obj.fdifRGB[0] * light.rgb[0] * ldotn;
                cv.g += inter.obj.fdifRGB[1] * light.rgb[1] * ldotn;
                cv.b += inter.obj.fdifRGB[2] * light.rgb[2] * ldotn;

                IVector reflected = 2 * normal.ScalarProduct(dir) * normal - dir;
                reflected.Normalize();
                rd.Normalize();
                double rvn = Math.Pow(reflected.ScalarProduct(-rd), inter.obj.fn);

                cv.r += inter.obj.frefRGB[0] * light.rgb[0] * rvn;
                cv.g += inter.obj.frefRGB[1] * light.rgb[1] * rvn;
                cv.b += inter.obj.frefRGB[2] * light.rgb[2] * rvn;

            }

            IVector refRayStart = inter.point;
            IVector refRayDir = (rd - 2 * normal.ScalarProduct(rd) * normal).Normalize();
            refRayStart += 1E-4 * refRayDir;
            ColorVector specularCV = Follow(refRayStart, refRayDir, depth - 1);
            cv.r += inter.obj.fkref * specularCV.r;
            cv.g += inter.obj.fkref * specularCV.g;
            cv.b += inter.obj.fkref * specularCV.b;

            return cv;
        }

        private IVector GetDirectionForScreenCoords(int x, int y)
        {
            double xIncrement = (scene.l + scene.r) / (Width - 1);
            double yIncrement = (scene.t + scene.b) / (Height - 1);

            var hView = scene.h * scene.view.NNormalize();
            var xDir = (x * xIncrement - scene.l) * scene.xAxis;
            var yDir = (y * yIncrement - scene.b) * scene.yAxis;

            var dir = hView + xDir;
            dir += yDir;
            return dir;
        }

    }

    class ColorVector
    {
        public double r;
        public double g;
        public double b;
    }

}
