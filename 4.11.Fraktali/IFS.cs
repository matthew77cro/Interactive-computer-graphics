using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace IFS
{
    class Program
    {
        static void Main(string[] args)
        {

            new Window(LoadFromFile(args[0])).Run();
        }

        static IFSParams LoadFromFile(string path)
        {
            StreamReader reader = new StreamReader(path);

            IFSParams ifs = new IFSParams();

            string line = reader.ReadLine().Trim();
            while (line.StartsWith("#"))
                line = reader.ReadLine().Trim();
            ifs.pointsNumber = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[0]);

            line = reader.ReadLine().Trim();
            while (line.StartsWith("#"))
                line = reader.ReadLine().Trim();
            ifs.limit = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[0]);

            line = reader.ReadLine().Trim();
            while (line.StartsWith("#"))
                line = reader.ReadLine().Trim();
            ifs.eta1 = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[0]);
            ifs.eta2 = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[1]);

            line = reader.ReadLine().Trim();
            while (line.StartsWith("#"))
                line = reader.ReadLine().Trim();
            ifs.eta3 = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[0]);
            ifs.eta4 = int.Parse(System.Text.RegularExpressions.Regex.Split(line, @"\s+")[1]);

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine().Trim();

                if (line.StartsWith("#"))
                    continue;

                string[] lineSplit = System.Text.RegularExpressions.Regex.Split(line, @"\s+");

                ifs.table.Add(new double[] {
                    double.Parse(lineSplit[0]),
                    double.Parse(lineSplit[1]),
                    double.Parse(lineSplit[2]),
                    double.Parse(lineSplit[3]),
                    double.Parse(lineSplit[4]),
                    double.Parse(lineSplit[5]),
                    double.Parse(lineSplit[6])
                });
            }

            reader.Close();

            return ifs;
        }
    }

    class Window : GameWindow
    {

        IFSParams param;

        public Window(IFSParams param)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV4Z10")
        {
            this.param = param;   
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

        private int Zaokruzi(double d)
        {
            if (d >= 0) return (int)(d + 0.5);
            return (int)(d - 0.5);
        }

        void RenderScene()
        {
            Random rnd = new Random();

            GL.PointSize(1.0f);
            GL.Color3(0.0f, 0.7f, 0.3f);
            GL.Begin(PrimitiveType.Points);
            double x0, y0;
            for (int brojac = 0; brojac < param.pointsNumber; brojac++)
            {
                // pocetna tocka:
                x0 = 0;
                y0 = 0;
                //iterativna primjena :
                for (int iter = 0; iter < param.limit; iter++)
                {
                    double p = rnd.NextDouble();

                    double pSum = 0;
                    int ptr = 0;
                    while (true)
                    {
                        pSum += param.table[ptr][6];
                        if (p < pSum)
                            break;
                        ptr++;
                    }

                    double[] t = param.table[ptr];
                    double x = t[0] * x0 + t[1] * y0 + t[4];
                    double y = t[2] * x0 + t[3] * y0 + t[5];

                    x0 = x; 
                    y0 = y;
                }
                // crtanje konacne tocke
                GL.Vertex2(Zaokruzi(x0 * param.eta1 + param.eta2), Zaokruzi(y0 * param.eta3 + param.eta4));
            }
            GL.End();
        }

    }

    class IFSParams
    {
        public int pointsNumber;
        public int limit;
        public int eta1;
        public int eta2;
        public int eta3;
        public int eta4;
        public List<double[]> table = new List<double[]>();
    }

}
