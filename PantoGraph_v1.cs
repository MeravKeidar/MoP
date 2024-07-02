using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MoP
{
    public class PantoGraph_v1 : GH_Component
    {
        public PantoGraph_v1()
          : base("Pantograph", "Panto", "Simulates a pantograph drawing machine", "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Radius1", "R1", "Radius of the first disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Speed1", "S1", "Speed of the first disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius2", "R2", "Radius of the second disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Speed2", "S2", "Speed of the second disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time Steps", "T", "Number of time steps", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Pattern", "P", "Resulting drawing pattern", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double radius1 = 0;
            double speed1 = 0;
            double radius2 = 0;
            double speed2 = 0;
            double timeSteps = 0;

            if (!DA.GetData(0, ref radius1)) return;
            if (!DA.GetData(1, ref speed1)) return;
            if (!DA.GetData(2, ref radius2)) return;
            if (!DA.GetData(3, ref speed2)) return;
            if (!DA.GetData(4, ref timeSteps)) return;

            int steps = (int)Math.Round(timeSteps); // Round and cast to int

            List<Point3d> points = new List<Point3d>();

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps * 2 * Math.PI;
                Point3d point1 = new Point3d(radius1 * Math.Cos(speed1 * t), radius1 * Math.Sin(speed1 * t), 0);
                Point3d point2 = new Point3d(radius2 * Math.Cos(speed2 * t), radius2 * Math.Sin(speed2 * t), 0);

                // Combine the positions to create the pattern point
                Point3d patternPoint = point1 + point2;

                points.Add(patternPoint);
            }

            Polyline pattern = new Polyline(points);
            DA.SetData(0, pattern);
        }

        public override Guid ComponentGuid => new Guid("362a53e0-a0d5-4347-9393-bfa44d102ff7");
    }
}
