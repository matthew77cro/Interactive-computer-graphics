using IRGLinearAlgebra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tijela3D
{

    class Program
    {
        static void Main(string[] args)
        {

            Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>
            {
                ["load"] = new LoadCommand(),
                ["point"] = new PointCheckCommand(),
                ["normalize"] = new NormalizeCommand(),
                ["quit"] = new QuitCommand()
            };

            var result = CommandResult.CONITNUE;
            var context = new CommandContext();

            while(result == CommandResult.CONITNUE)
            {
                Console.Write("> ");
                string input = Console.ReadLine().Trim();

                if (!commands.ContainsKey(input))
                {
                    Console.WriteLine("Command \"" + input + "\" does not exist");
                    continue;
                }

                ICommand command = commands[input];
                result = command.Execute(context);
            }

        }
    }

    class CommandContext
    {

        public ObjectModel Model { get; set; }

    }

    interface ICommand
    {

        CommandResult Execute(CommandContext context);

    }

    enum CommandResult
    {
        CONITNUE,
        EXIT
    }

    class LoadCommand : ICommand
    {
        public CommandResult Execute(CommandContext context)
        {
            ObjectModel model;
            try
            {
                Console.WriteLine("Enter file path: ");
                Console.Write("> ");
                var reader = new StreamReader(Console.ReadLine());
                model = ObjectModel.FromWavefront(reader);
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType() + " : " + ex.Message);
                return CommandResult.CONITNUE;
            }

            context.Model = model;
            return CommandResult.CONITNUE;
        }
    }

    class PointCheckCommand : ICommand
    {
        public CommandResult Execute(CommandContext context)
        {
            if(context.Model == null)
            {
                Console.WriteLine("[ERROR] Model not loaded!");
                return CommandResult.CONITNUE;
            }

            try
            {
                Console.WriteLine("Point x y z coordinates :");
                Console.Write("> ");

                string[] input = System.Text.RegularExpressions.Regex.Split(Console.ReadLine().Trim(), @"\s+");
                double x = double.Parse(input[0]);
                double y = double.Parse(input[1]);
                double z = double.Parse(input[2]);

                Console.WriteLine(context.Model.PointObjectRelationship(x, y, z));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType() + " : " + ex.Message);
                return CommandResult.CONITNUE;
            }

            return CommandResult.CONITNUE;
        }
    }

    class NormalizeCommand : ICommand
    {
        public CommandResult Execute(CommandContext context)
        {
            if (context.Model == null)
            {
                Console.WriteLine("[ERROR] Model not loaded!");
                return CommandResult.CONITNUE;
            }

            context.Model.Normalize();
            Console.WriteLine(context.Model.DumpToOBJ());
            return CommandResult.CONITNUE;
        }
    }

    class QuitCommand : ICommand
    {
        public CommandResult Execute(CommandContext context)
        {
            return CommandResult.EXIT;
        }
    }

    class ObjectModel
    {

        private List<Vector> vertices = new List<Vector>();
        private List<Face3D> faces = new List<Face3D>();
        private List<Vector> planeCoef = new List<Vector>();

        public Face3D this[int i]
        {
            get
            {
                if (i < 0 || i >= faces.Count)
                    throw new IndexOutOfRangeException();
                return faces[i];
            }
        }

        public Vector GetVertex(int vertexId)
        {
            if (vertexId < 0 || vertexId >= vertices.Count)
                throw new ArgumentOutOfRangeException();
            return vertices[vertexId];
        }

        public PointObject PointObjectRelationship(double x, double y, double z)
        {

            Vector pointH = new Vector(x, y, z, 1);

            bool onObject = false;
            foreach(Vector coef in planeCoef)
            {
                double scalarProduct = pointH.ScalarProduct(coef);
                if (scalarProduct == 0)
                {
                    onObject = true;
                    continue;
                }
                else if(scalarProduct > 0)
                {
                    return PointObject.OUT_OBJECT;
                }
            }

            return onObject ? PointObject.ON_OBJECT : PointObject.IN_OBJECT;

        }

        public ObjectModel Copy()
        {
            ObjectModel objmodel = new ObjectModel
            {
                vertices = vertices.GetRange(0, vertices.Count),
                faces = faces.GetRange(0, faces.Count),
                planeCoef = planeCoef.GetRange(0, planeCoef.Count)
            };
            return objmodel;
        }

        public string DumpToOBJ()
        {
            StringBuilder sb = new StringBuilder();

            foreach(Vector v in vertices)
            {
                sb.AppendLine("v " + v[0] + " " + v[1] + " " + v[2]);
            }

            foreach (Face3D f in faces)
            {
                sb.AppendLine("f " + (f[0]+1) + " " + (f[1]+1) + " " + (f[2]+1));
            }

            return sb.ToString();
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
                else if (split[0] == "g")
                {
                    // DO NOTHING
                }
                else
                {
                    throw new FormatException();
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

        public enum PointObject
        {
            NONE,
            ON_OBJECT,
            IN_OBJECT,
            OUT_OBJECT
        }

    }

}
