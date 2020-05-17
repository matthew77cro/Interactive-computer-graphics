using IRGLinearAlgebra;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UklanjanjeSkrivenihPoligona2
{

    class Program
    {
        static void Main(string[] args)
        {

            if(args.Length != 1)
            {
                Console.WriteLine("One argument expected");
                Console.ReadLine();
                return;
            }

            ObjectModel model;
            try
            {
                var reader = new StreamReader(args[0]);
                model = ObjectModel.FromWavefront(reader);
                reader.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("One argument expected");
                return;
            }

            new Window(model).Run();

        }
    }

    class ObjectModel
    {
        private List<Vector> vertices = new List<Vector>();
        private List<Face3D> faces = new List<Face3D>();
        private List<Vector> planeCoef = new List<Vector>();
        private List<bool> visibility = new List<bool>();

        public Face3D this[int i]
        {
            get
            {
                if (i < 0 || i >= faces.Count)
                    throw new IndexOutOfRangeException();
                return faces[i];
            }
        }

        public int GetFaceCount()
        {
            return faces.Count;
        }

        public Vector GetVertex(int vertexId)
        {
            if (vertexId < 0 || vertexId >= vertices.Count)
                throw new ArgumentOutOfRangeException();
            return vertices[vertexId];
        }

        public void Normalize()
        {
            double xmin = vertices[0][0], xmax = vertices[0][0],
                ymin = vertices[0][1], ymax = vertices[0][1],
                zmin = vertices[0][2], zmax = vertices[0][2];

            foreach (Vector v in vertices)
            {
                if (v[0] < xmin) xmin = v[0];
                if (v[1] < ymin) ymin = v[1];
                if (v[2] < zmin) zmin = v[2];

                if (v[0] > xmax) xmax = v[0];
                if (v[1] > ymax) ymax = v[1];
                if (v[2] > zmax) zmax = v[2];
            }

            double xcenter = (xmin + xmax) / 2;
            double ycenter = (ymin + ymax) / 2;
            double zcenter = (zmin + zmax) / 2;

            double m = Math.Max(xmax - xmin, Math.Max(ymax - ymin, zmax - zmin));

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector v = vertices[i];
                vertices[i] = new Vector(true, true, new double[] { 2 * (v[0] - xcenter) / m, 2 * (v[1] - ycenter) / m, 2 * (v[2] - zcenter) / m });
            }

            CalculatePlaneCoef();
        }

        private void CalculatePlaneCoef()
        {
            planeCoef.Clear();

            foreach (Face3D f in faces)
            {
                Vector v1 = vertices[f[0]];
                Vector v2 = vertices[f[1]];
                Vector v3 = vertices[f[2]];

                Vector n = (Vector)((v2 - v1) * (v3 - v1));

                double d = -n[0] * v1[0] - n[1] * v1[1] - n[2] * v1[2];

                planeCoef.Add(new Vector(true, true, new double[] { n[0], n[1], n[2], d }));
            }
        }

        public IEnumerable<Face3D> Faces()
        {
            foreach(var face in faces)
            {
                yield return face;
            }
        }

        public static ObjectModel FromWavefront(StreamReader reader)
        {
            ObjectModel objmodel = new ObjectModel();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                string[] split = System.Text.RegularExpressions.Regex.Split(line, @"\s+");

                if (split[0] == "v")
                {
                    double x, y, z;
                    x = double.Parse(split[1]);
                    y = double.Parse(split[2]);
                    z = double.Parse(split[3]);
                    objmodel.vertices.Add(new Vector(true, true, new double[] { x, y, z }));
                }
                else if (split[0] == "f")
                {
                    int v1, v2, v3;
                    v1 = int.Parse(split[1]) - 1;
                    v2 = int.Parse(split[2]) - 1;
                    v3 = int.Parse(split[3]) - 1;
                    objmodel.faces.Add(new Face3D(v1, v2, v3));
                }
            }

            objmodel.CalculatePlaneCoef();

            return objmodel;
        }

        public class Face3D
        {

            private static readonly int DIMENSION = 3;

            private readonly int[] indexes = new int[DIMENSION];

            public int this[int i]
            {
                get
                {
                    if (i < 0 || i >= DIMENSION)
                        throw new IndexOutOfRangeException();
                    return indexes[i];
                }
            }

            public Face3D(int vertex1, int vertex2, int vertex3)
            {
                indexes[0] = vertex1;
                indexes[1] = vertex2;
                indexes[2] = vertex3;
            }

        }

        public bool IsVisible(int faceId)
        {
            return visibility[faceId];
        }

        public void DetermineFaceVisibilities1(IVector eye)
        {
            visibility.Clear();

            IVector eyeH = new Vector(false, true, new double[] { eye[0], eye[1], eye[2], 1 });
            
            foreach(var coef in planeCoef)
            {
                visibility.Add(coef.ScalarProduct(eyeH) > 0);
            }
        }

        public void DetermineFaceVisibilities2(IVector eye)
        {
            visibility.Clear();

            foreach(var face in faces)
            {
                IVector v1 = vertices[face[0]];
                IVector v2 = vertices[face[1]];
                IVector v3 = vertices[face[2]];

                IVector c = (v1 + v2 + v3).ScalarMultiply((double)1 / 3);
                IVector e = eye - c;
                IVector n = ((v2 - v1) * (v3 - v1)).Normalize();

                visibility.Add(n.ScalarProduct(e) > 0);
            }
        }

    }

    class Window : GameWindow
    {

        private readonly ObjectModel obj;

        private double angle = 18.4349488;
        private readonly double increment = 1;
        private readonly double r = 3.16227766;

        private CullingMode cullingMode = CullingMode.NO_CULL;

        public Window(ObjectModel obj)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV3Z7-2")
        {
            this.obj = obj;
            obj.Normalize();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 1.0f, 0.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.LoadIdentity();
            //crtanje scene :
            RenderScene();
            SwapBuffers();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    angle = 18.4349488;
                    break;
                case Key.R:
                    angle += increment;
                    break;
                case Key.L:
                    angle -= increment;
                    break;
                case Key.Number1:
                case Key.Keypad1:
                    cullingMode = CullingMode.NO_CULL;
                    break;
                case Key.Number2:
                case Key.Keypad2:
                    cullingMode = CullingMode.ALG1;
                    break;
                case Key.Number3:
                case Key.Keypad3:
                    cullingMode = CullingMode.ALG2;
                    break;
                case Key.Number4:
                case Key.Keypad4:
                    cullingMode = CullingMode.ALG3;
                    break;
            }
        }

        private void RenderScene()
        {
            IVector eye = new Vector(true, true, new double[] { r * Math.Cos(angle / 180 * Math.PI), 4.0f, r * Math.Sin(angle / 180 * Math.PI) });
            IMatrix tp = IRG.LookAtMatrix(eye,
                                          new Vector(true, true, new double[] { 0.0f, 0.0f, 0.0f }),
                                          new Vector(true, true, new double[] { 0.0f, 1.0f, 0.0f }));
            IMatrix pr = IRG.BuildFrustumMatrix(-0.5, 0.5, -0.5, 0.5, 1, 100);
            IMatrix m = tp.NMultiply(pr);

            if (cullingMode == CullingMode.ALG1)
            {
                obj.DetermineFaceVisibilities1(eye);
            }
            else if (cullingMode == CullingMode.ALG2)
            {
                obj.DetermineFaceVisibilities2(eye);
            }

            GL.PointSize(1.0f);
            GL.Color3(1.0f, 0.0f, 0.0f);

            for (int i = 0; i < obj.GetFaceCount(); i++)
            {
                var face = obj[i];
                if ((cullingMode == CullingMode.ALG1 || cullingMode == CullingMode.ALG2)
                    && !obj.IsVisible(i)) continue;

                IVector vertex1 = obj.GetVertex(face[0]);
                vertex1 = new Vector(false, true, new double[] {vertex1[0], vertex1[1], vertex1[2], 1 });
                vertex1 = vertex1.ToRowMatrix(false).NMultiply(m).ToVector(false).NFromHomogeneus();

                IVector vertex2 = obj.GetVertex(face[1]);
                vertex2 = new Vector(false, true, new double[] { vertex2[0], vertex2[1], vertex2[2], 1 });
                vertex2 = vertex2.ToRowMatrix(false).NMultiply(m).ToVector(false).NFromHomogeneus();

                IVector vertex3 = obj.GetVertex(face[2]);
                vertex3 = new Vector(false, true, new double[] { vertex3[0], vertex3[1], vertex3[2], 1 });
                vertex3 = vertex3.ToRowMatrix(false).NMultiply(m).ToVector(false).NFromHomogeneus();

                if (cullingMode == CullingMode.ALG3 && 
                    !IRG.isAntiClockwise(vertex1[0], vertex1[1], 
                                         vertex2[0], vertex2[1], 
                                         vertex3[0], vertex3[1])) continue;

                GL.Begin(PrimitiveType.LineLoop);
                GL.Vertex2(vertex1[0], vertex1[1]);
                GL.Vertex2(vertex2[0], vertex2[1]);
                GL.Vertex2(vertex3[0], vertex3[1]);
                GL.End();
            }
        }

    }

    enum CullingMode
    {
        NO_CULL,
        ALG1,
        ALG2,
        ALG3
    }

}
