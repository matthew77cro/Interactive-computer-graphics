using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Collections;
using IRGLinearAlgebra;
using System.Collections.ObjectModel;

namespace Poligoni
{
    class Program
    {
        public static void Main(string[] args)
        {
            new Window(new WindowModel()).Run();
        }
    }

    class WindowModel
    {

        public enum State
        {
            POLY_DRAW,
            POINT_TESTING
        }

        public Polygon Poly { get; }
        public int MouseX { get; private set; }
        public int MouseY { get; private set; }
        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }
        public State ProgramState { get; set; }
        public bool Fill { get; set; }
        public bool Convex { get; set; }

        public WindowModel()
        {
            MouseX = 0;
            MouseY = 0;
            ProgramState = State.POLY_DRAW;
            Fill = false;
            Convex = false;
            Poly = new Polygon();
        }

        public void SetMouse(int x, int y)
        {
            MouseX = x;
            MouseY = y;
        }

        public void WindowResized(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
        }

        public int ConvertYCoord(int y)
        {
            return WindowHeight - y;
        }
    }

    class Window : GameWindow
    {

        private readonly WindowModel model;

        public Window(WindowModel model)
            : base(640, 480, OpenTK.Graphics.GraphicsMode.Default, "LV2Z4")
        {
            this.model = model;
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            model.SetMouse(e.X, model.ConvertYCoord(e.Y));
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'n':
                    if (model.ProgramState == WindowModel.State.POLY_DRAW)
                    {
                        model.ProgramState = WindowModel.State.POINT_TESTING;
                    }
                    else
                    {
                        model.Fill = false;
                        model.Convex = false;
                        model.ProgramState = WindowModel.State.POLY_DRAW;
                        model.Poly.Clear();
                    }
                    break;
                case 'p':
                    if (model.ProgramState == WindowModel.State.POLY_DRAW)
                        model.Fill = !model.Fill;
                    break;
                case 'k':
                    if (model.ProgramState != WindowModel.State.POLY_DRAW)
                        break;
                    if ((model.Poly.GetPolyType() == Polygon.PolyType.CONCAVE_ANTICLOCKWISE ||
                        model.Poly.GetPolyType() == Polygon.PolyType.CONCAVE_CLOCKWISE) 
                        && !model.Convex)
                    {
                        Console.WriteLine("[ERROR] Poly is not convex!");
                        break;
                    }
                    model.Convex = !model.Convex;
                    break;
                case 'i': // Info
                    Console.WriteLine(model.Poly.GetPolyType());
                    break;
                default:
                    break;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button != MouseButton.Left)
                return;
            switch(model.ProgramState)
            {
                case WindowModel.State.POLY_DRAW:
                    var pType = model.Poly.GetPolyType(model.MouseX, model.MouseY);
                    if (model.Convex && (pType == Polygon.PolyType.CONCAVE_ANTICLOCKWISE || pType == Polygon.PolyType.CONCAVE_CLOCKWISE))
                    {
                        Console.WriteLine("[ERROR] Cannot create concave polygon while convex check is on. Try turning it off by pressing 'k' on your keyboard.");
                        return;
                    }
                    model.Poly.Add(new Point(model.MouseX, model.MouseY));
                    break;
                case WindowModel.State.POINT_TESTING:
                    Console.WriteLine("Point : (" + model.MouseX + ", " + model.MouseY + ") -> relation to poly : " + model.Poly.PointPolyRelationship(model.MouseX, model.MouseY));
                    break;
                default:
                    break;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (model.Convex)
            {
                GL.ClearColor(0f, 1.0f, 0f, 1.0f);
            }
            else
            {
                GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            }
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
            GL.Ortho(0, Width - 1, Height - 1, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            model.WindowResized(Width, Height);
        }

        private void RenderScene()
        {
            GL.PointSize(1f);
            GL.Color3(0f, 0f, 0f);

            if (model.Fill)
            {
                /*GL.Begin(PrimitiveType.Polygon);
                foreach (Point p in model.Poly)
                {
                    GL.Vertex2(p.X, model.ConvertYCoord(p.Y));
                }
                if (model.ProgramState == State.POLY_DRAW)
                {
                    GL.Vertex2(model.MouseX, model.ConvertYCoord(model.MouseY));
                }
                GL.End();*/
                if(model.Poly.Count == 1)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Vertex2(model.Poly[0].X, model.ConvertYCoord(model.Poly[0].Y));
                    GL.Vertex2(model.MouseX, model.ConvertYCoord(model.MouseY));
                    GL.End();
                } 
                else
                {
                    FillPoly();
                }
            }
            else
            {
                GL.Begin(PrimitiveType.LineLoop);
                foreach(Point p in model.Poly.AllPoints())
                {
                    GL.Vertex2(p.X, model.ConvertYCoord(p.Y));
                }
                if (model.ProgramState == WindowModel.State.POLY_DRAW)
                {
                    GL.Vertex2(model.MouseX, model.ConvertYCoord(model.MouseY));
                }
                GL.End();
            }
        }
        
        private void FillPoly()
        {
            List<Point> vertices = model.Poly.getPointsClockwise();
            if (model.ProgramState == WindowModel.State.POLY_DRAW)
            {
                vertices.Add(new Point(model.MouseX, model.MouseY));
            }

            int xmin = vertices[0].X;
            int xmax = vertices[0].X;
            int ymin = vertices[0].Y;
            int ymax = vertices[0].Y;
            for (int i = 1; i < vertices.Count; i++)
            {
                if (xmin > vertices[i].X) xmin = vertices[i].X;
                if (xmax < vertices[i].X) xmax = vertices[i].X;
                if (ymin > vertices[i].Y) ymin = vertices[i].Y;
                if (ymax < vertices[i].Y) ymax = vertices[i].Y;
            }

            for (int y = ymin; y <= ymax; y++)
            {
                double L = xmin, D = xmax;
                for (int i = 0; i < vertices.Count; i++)
                {
                    Edge edge = new Edge(vertices[i].X, vertices[i].Y, vertices[(i + 1) % vertices.Count].X, vertices[(i + 1) % vertices.Count].Y);

                    if (edge.LineHomogCoef[0] == 0)
                    {
                        if (edge.Vertex1.Y == y)
                        {
                            if (edge.Vertex1.X < edge.Vertex2.X)
                            {
                                L = edge.Vertex1.X;
                                D = edge.Vertex2.X;
                            }
                            else
                            {
                                L = edge.Vertex2.X;
                                D = edge.Vertex1.X;
                            }
                            break;
                        }
                    }
                    else
                    {
                        double x = (-edge.LineHomogCoef[1] * y - edge.LineHomogCoef[2]) / edge.LineHomogCoef[0];
                        if (edge.Vertex1.Y < edge.Vertex2.Y) // ako je lijevi brid
                        {
                            if (L < x) L = x;
                        }
                        else
                        {
                            if (D > x) D = x;
                        }
                    }
                }

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex2((int)Math.Round(L), model.ConvertYCoord(y));
                GL.Vertex2((int)Math.Round(D), model.ConvertYCoord(y));
                GL.End();
            }

        }
    }

    class Point
    {
        public int X { get; }
        public int Y { get; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class Edge
    {
        public Point Vertex1 { get; }
        public Point Vertex2 { get; }
        public Vector LineHomogCoef { get; }

        public Edge(int x1, int y1, int x2, int y2)
        {
            Vertex1 = new Point(x1, y1);
            Vertex2 = new Point(x2, y2);
            LineHomogCoef = new Vector(true, true, new double[] { y1 - y2, -(x1 - x2), x1 * y2 - y1 * x2 });
        }
    }

    class Polygon
    {

        public enum PolyType
        {
            NONE,
            CONCAVE_CLOCKWISE,
            CONCAVE_ANTICLOCKWISE,
            CONVEX_CLOCKWISE,
            CONVEX_ANTICLOCKWISE
        }
        public enum PointPoly
        {
            NONE,
            ON_POLY,
            IN_POLY,
            OUT_POLY
        }

        private readonly List<Point> vertices = new List<Point>();

        public PolyType Type { get; private set; }
        
        public Point this[int i]
        {
            get
            {
                if (i < 0 || i >= vertices.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return vertices[i];
            }
        }
        public int Count { get { return vertices.Count; } }
        public long ChangeCount { get; private set; }

        public Polygon()
        {
            ChangeCount = long.MinValue;
            Type = PolyType.NONE;
        }

        public void Clear()
        {
            vertices.Clear();
            Type = PolyType.NONE;
            ChangeCount++;
        }

        public void Add(Point point)
        {
            if (point == null)
                throw new ArgumentNullException();
            vertices.Add(point);
            ChangeCount++;

            Type = CalculatePolyType(vertices);
        }

        public PolyType GetPolyType()
        {
            return Type;
        }

        public PolyType GetPolyType(int newVertexX, int newVertexY)
        {
            List<Point> vertices = this.vertices.GetRange(0, this.vertices.Count);
            vertices.Add(new Point(newVertexX, newVertexY));
            return CalculatePolyType(vertices);
        }

        public PointPoly PointPolyRelationship(int x, int y)
        {

            if (vertices.Count < 3)
                return PointPoly.NONE;

            Vector point = new Vector(new double[] { x, y, 1 });

            sbyte sign = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Edge edge = new Edge(vertices[i].X, vertices[i].Y, vertices[(i + 1) % vertices.Count].X, vertices[(i + 1) % vertices.Count].Y);
                double scalarProduct = point.ScalarProduct(edge.LineHomogCoef);

                if (scalarProduct == 0) return PointPoly.ON_POLY;

                sbyte s = scalarProduct < 0 ? (sbyte)-1 : (sbyte)1;
                if (sign == 0)
                    sign = s;
                else if (sign != s)
                    return PointPoly.OUT_POLY;
            }

            return PointPoly.IN_POLY;
        }

        public IEnumerable<Point> AllPoints()
        {
            long changeCount = ChangeCount;
            int pointer = 0;

            while(pointer < vertices.Count)
            {
                if(changeCount != ChangeCount)
                    throw new InvalidOperationException("Concurrent modification");

                yield return vertices[pointer];

                pointer++;
            }
        }

        public IEnumerable<Point> AllPointsClockwise()
        {
            long changeCount = ChangeCount;
            int pointer = (Type == PolyType.CONCAVE_CLOCKWISE || 
                            Type == PolyType.CONVEX_CLOCKWISE) ? 0 : vertices.Count - 1;

            while ((Type == PolyType.CONCAVE_CLOCKWISE ||
                            Type == PolyType.CONVEX_CLOCKWISE) ? pointer<vertices.Count : pointer >= 0)
            {
                if (changeCount != ChangeCount)
                    throw new InvalidOperationException("Concurrent modification");

                yield return vertices[pointer];

                pointer = (Type == PolyType.CONCAVE_CLOCKWISE ||
                            Type == PolyType.CONVEX_CLOCKWISE) ? pointer+1 : pointer-1;
            }
        }

        public List<Point> getPoints()
        {
            return vertices.GetRange(0, vertices.Count);
        }

        public List<Point> getPointsClockwise()
        {
            var vert = vertices.GetRange(0, vertices.Count);
            if (Type == PolyType.CONCAVE_ANTICLOCKWISE || Type == PolyType.CONVEX_ANTICLOCKWISE)
                vert.Reverse();
            return vert;
        }

        public static PolyType CalculatePolyType(in List<Point> vertices)
        {

            if (vertices.Count < 3)
                return PolyType.NONE;

            sbyte sign = 0;
            for(int i = 0; i < vertices.Count; i++)
            {
                Edge edge = new Edge(vertices[i].X, vertices[i].Y, vertices[(i + 1) % vertices.Count].X, vertices[(i + 1) % vertices.Count].Y);
                Point vertex = vertices[(i + 2) % vertices.Count];
                double scalarProduct = new Vector(new double[] { vertex.X, vertex.Y, 1 }).ScalarProduct(edge.LineHomogCoef);

                if (scalarProduct == 0) continue;
                
                sbyte s = scalarProduct < 0 ? (sbyte)-1 : (sbyte)1;
                if (sign == 0) 
                    sign = s;
                else if (sign != s) 
                    return sign == -1 ? PolyType.CONCAVE_CLOCKWISE : PolyType.CONCAVE_ANTICLOCKWISE;
            }

            return sign == -1 ? PolyType.CONVEX_CLOCKWISE : PolyType.CONVEX_ANTICLOCKWISE;

        }
    }

}
