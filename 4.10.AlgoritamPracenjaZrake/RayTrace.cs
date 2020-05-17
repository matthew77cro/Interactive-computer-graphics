using IRGLinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritamPracenjaZrake
{
    
    public class Light
    {
        public IVector position;
        public double[] rgb = new double[] { 0, 0, 0 };
    }

    public abstract class SceneObject
    {
        // parametri prednje strane objekta
        public double[] fambRGB = new double[] { 0, 0, 0 };
        public double[] fdifRGB = new double[] { 0, 0, 0 };
        public double[] frefRGB = new double[] { 0, 0, 0 };
        public double fn;
        public double fkref;
        // parametri straznje strane objekta
        public double[] bambRGB = new double[] { 0, 0, 0 };
        public double[] bdifRGB = new double[] { 0, 0, 0 };
        public double[] brefRGB = new double[] { 0, 0, 0 };
        public double bn;
        public double bkref;

        // Apstraktne metode koje d=ce definirati konkretni modeli
        public abstract void UpdateIntersection(Intersection inters, IVector start, IVector d);
        public abstract IVector GetNormalInPoint(IVector point);
    }

    public class Patch : SceneObject
    {
        public IVector center;
        public IVector v1;
        public IVector v2;
        public IVector normal;
        public double w;
        public double h;

        public override IVector GetNormalInPoint(IVector point)
        {
            return normal;
        }

        public override void UpdateIntersection(Intersection inters, IVector start, IVector d)
        {

            double dScalarNormal = d.ScalarProduct(normal);
            if (dScalarNormal == 0)
                return;

            IVector solution = (new Matrix(3, 3, new double[,] { { v1[0], v2[0], -d[0] }, { v1[1], v2[1], -d[1] }, { v1[2], v2[2], -d[2] } }, true).NInvert() * (start - center).ToColumnMatrix(true)).ToVector(true);

            double lambda = solution[2];

            double w2 = w / 2;
            double h2 = h / 2;

            if (lambda < 0 || solution[0] < -w2 || solution[0] > w2 || solution[1] < -h2 || solution[1] > h2)
                return;

            bool front = dScalarNormal < 0;

            if (inters.obj == null || inters.lambda > lambda)
            {
                inters.obj = this;
                inters.lambda = lambda;
                inters.point = start + lambda * d;
                inters.front = front;
            }

        }
    }

    public class Sphere : SceneObject
    {
        public IVector center;
        public double radius;

        public override IVector GetNormalInPoint(IVector point)
        {
            return (point - center).Normalize();
        }

        public override void UpdateIntersection(Intersection inters, IVector start, IVector d)
        {
            IVector centerStart = start - center;
            double a = d.ScalarProduct(d);
            double b = 2 * d.ScalarProduct(centerStart);
            double c = centerStart.ScalarProduct(centerStart) - radius * radius;

            double dsc = b * b - 4 * a * c;

            if (dsc < 0)
                return;

            double lambda1 = (-b + Math.Sqrt(dsc)) / (2 * a);
            double lambda2 = (-b - Math.Sqrt(dsc)) / (2 * a);
            double lambda;

            if (lambda1 <= 0 && lambda2 <= 0)
                return;

            if (lambda1 <= 0)
                lambda = lambda2;
            else if (lambda2 <= 0)
                lambda = lambda1;
            else
                lambda = lambda1 < lambda2 ? lambda1 : lambda2;

            bool front = centerStart.Norm() >= radius;

            if (inters.obj == null || inters.lambda > lambda)
            {
                inters.obj = this;
                inters.lambda = lambda;
                inters.point = start + lambda * d;
                inters.front = front;
            }
        }
    }

    public class RTScene
    {
        // Parametri iz datoteke
        public IVector eye;
        public IVector view;
        public IVector viewUp;
        public double h;
        public double xAngle;
        public double yAngle;
        public double[] gaIntensity = new double[] { 0, 0, 0 };
        public List<Light> sources = new List<Light>();
        public List<SceneObject> objects = new List<SceneObject>();

        // Izracunati paramteri
        public IVector xAxis;
        public IVector yAxis;
        public double l;
        public double r;
        public double b;
        public double t;

        private RTScene()
        {

        }

        // Metoda koja racuna xAxis, yAxis, l, r, b i t
        private void ComputeKS()
        {
            IVector go = -view;
            xAxis = (viewUp * go).Normalize();
            yAxis = (go * xAxis).Normalize();
            r = h * Math.Tan(xAngle * Math.PI / 360);
            l = r;
            t = h * Math.Tan(yAngle * Math.PI / 360);
            b = t;
        }

        // Metoda kojaucitava scenu i na kraju poziva computeKS();
        public static RTScene UcitajScenu(string path)
        {
            var fs = new StreamReader(path);
            RTScene scene = new RTScene();

            while (!fs.EndOfStream)
            {
                string line = fs.ReadLine().Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                string[] lineSplit = System.Text.RegularExpressions.Regex.Split(line, @"\s+");

                if (lineSplit[0] == "e")
                {
                    scene.eye = new Vector(true, true, new double[] { double.Parse(lineSplit[1]), double.Parse(lineSplit[2]), double.Parse(lineSplit[3]) });
                }
                else if (lineSplit[0] == "v")
                {
                    scene.view = new Vector(false, true, new double[] { double.Parse(lineSplit[1]), double.Parse(lineSplit[2]), double.Parse(lineSplit[3]) });
                }
                else if (lineSplit[0] == "vu")
                {
                    scene.viewUp = new Vector(false, true, new double[] { double.Parse(lineSplit[1]), double.Parse(lineSplit[2]), double.Parse(lineSplit[3]) });
                }
                else if (lineSplit[0] == "h")
                {
                    scene.h = double.Parse(lineSplit[1]);
                }
                else if (lineSplit[0] == "xa")
                {
                    scene.xAngle = double.Parse(lineSplit[1]);
                }
                else if (lineSplit[0] == "ya")
                {
                    scene.yAngle = double.Parse(lineSplit[1]);
                }
                else if (lineSplit[0] == "ga")
                {
                    scene.gaIntensity[0] = double.Parse(lineSplit[1]);
                    scene.gaIntensity[1] = double.Parse(lineSplit[2]);
                    scene.gaIntensity[2] = double.Parse(lineSplit[3]);
                }
                else if (lineSplit[0] == "i")
                {
                    var l = new Light();
                    l.position = new Vector(true, true, new double[] { double.Parse(lineSplit[1]), double.Parse(lineSplit[2]), double.Parse(lineSplit[3]) });
                    l.rgb[0] = double.Parse(lineSplit[4]);
                    l.rgb[1] = double.Parse(lineSplit[5]);
                    l.rgb[2] = double.Parse(lineSplit[6]);
                    scene.sources.Add(l);
                }
                else if (lineSplit[0] == "o")
                {
                    // kugla: cx cy cz r ar ag ab dr dg db rr rg rb n kref
                    // krpica cx cy cz v1x v1y v1z v2x v2y v2z wi he ar ag ab dr dg db rr rg rb n kref
                    //                                               ar ag ab dr dg db rr rg rb n kref

                    if (lineSplit[1] == "s")
                    {
                        var s = new Sphere();
                        s.center = new Vector(true, true, new double[] { double.Parse(lineSplit[2]), double.Parse(lineSplit[3]), double.Parse(lineSplit[4]) });
                        s.radius = double.Parse(lineSplit[5]);

                        s.fambRGB[0] = double.Parse(lineSplit[6]);
                        s.fambRGB[1] = double.Parse(lineSplit[7]);
                        s.fambRGB[2] = double.Parse(lineSplit[8]);
                        s.fdifRGB[0] = double.Parse(lineSplit[9]);
                        s.fdifRGB[1] = double.Parse(lineSplit[10]);
                        s.fdifRGB[2] = double.Parse(lineSplit[11]);
                        s.frefRGB[0] = double.Parse(lineSplit[12]);
                        s.frefRGB[1] = double.Parse(lineSplit[13]);
                        s.frefRGB[2] = double.Parse(lineSplit[14]);
                        s.fn = double.Parse(lineSplit[15]);
                        s.fkref = double.Parse(lineSplit[16]);

                        s.bambRGB[0] = double.Parse(lineSplit[6]);
                        s.bambRGB[1] = double.Parse(lineSplit[7]);
                        s.bambRGB[2] = double.Parse(lineSplit[8]);
                        s.bdifRGB[0] = double.Parse(lineSplit[9]);
                        s.bdifRGB[1] = double.Parse(lineSplit[10]);
                        s.bdifRGB[2] = double.Parse(lineSplit[11]);
                        s.brefRGB[0] = double.Parse(lineSplit[12]);
                        s.brefRGB[1] = double.Parse(lineSplit[13]);
                        s.brefRGB[2] = double.Parse(lineSplit[14]);
                        s.bn = double.Parse(lineSplit[15]);
                        s.bkref = double.Parse(lineSplit[16]);

                        scene.objects.Add(s);
                    }
                    else if (lineSplit[1] == "p")
                    {
                        var p = new Patch();
                        p.center = new Vector(true, true, new double[] { double.Parse(lineSplit[2]), double.Parse(lineSplit[3]), double.Parse(lineSplit[4]) });
                        p.v1 = new Vector(false, true, new double[] { double.Parse(lineSplit[5]), double.Parse(lineSplit[6]), double.Parse(lineSplit[7]) });
                        p.v2 = new Vector(false, true, new double[] { double.Parse(lineSplit[8]), double.Parse(lineSplit[9]), double.Parse(lineSplit[10]) });
                        p.normal = (p.v1 * p.v2).Normalize();
                        p.w = double.Parse(lineSplit[11]);
                        p.h = double.Parse(lineSplit[12]);

                        p.fambRGB[0] = double.Parse(lineSplit[13]);
                        p.fambRGB[1] = double.Parse(lineSplit[14]);
                        p.fambRGB[2] = double.Parse(lineSplit[15]);
                        p.fdifRGB[0] = double.Parse(lineSplit[16]);
                        p.fdifRGB[1] = double.Parse(lineSplit[17]);
                        p.fdifRGB[2] = double.Parse(lineSplit[18]);
                        p.frefRGB[0] = double.Parse(lineSplit[19]);
                        p.frefRGB[1] = double.Parse(lineSplit[20]);
                        p.frefRGB[2] = double.Parse(lineSplit[21]);
                        p.fn = double.Parse(lineSplit[22]);
                        p.fkref = double.Parse(lineSplit[23]);

                        p.bambRGB[0] = double.Parse(lineSplit[24]);
                        p.bambRGB[1] = double.Parse(lineSplit[25]);
                        p.bambRGB[2] = double.Parse(lineSplit[26]);
                        p.bdifRGB[0] = double.Parse(lineSplit[27]);
                        p.bdifRGB[1] = double.Parse(lineSplit[28]);
                        p.bdifRGB[2] = double.Parse(lineSplit[29]);
                        p.brefRGB[0] = double.Parse(lineSplit[30]);
                        p.brefRGB[1] = double.Parse(lineSplit[31]);
                        p.brefRGB[2] = double.Parse(lineSplit[32]);
                        p.bn = double.Parse(lineSplit[33]);
                        p.bkref = double.Parse(lineSplit[34]);

                        scene.objects.Add(p);
                    }

                }

            }

            scene.ComputeKS();

            fs.Close();

            return scene;
        }
    }

    public class Intersection
    {
        public SceneObject obj; // najblizi objekt s kojim se zraka sjece
        public double lambda; // Za koji se labda to dogada?
        public bool front; // Je li to sjeciste na prednjoj strani objekta?
        public IVector point; // U kojoj je tocki to sjeciste ?
    }

}
