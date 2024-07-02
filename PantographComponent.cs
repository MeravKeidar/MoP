using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;


namespace MoP
{
    public class PantographComponent : GH_Component
    {
        public PantographComponent()
      : base("Pantograph", "Panto", "Simulates a pantograph drawing machine", "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Radius1", "R1", "Radius of the first disk", GH_ParamAccess.item, 5.0);
            pManager.AddNumberParameter("Speed1", "S1", "Speed of the first disk", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Radius2", "R2", "Radius of the second disk", GH_ParamAccess.item, 3.0);
            pManager.AddNumberParameter("Speed2", "S2", "Speed of the second disk", GH_ParamAccess.item, 2.0);
            pManager.AddNumberParameter("Time", "T", "Time parameter", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Pattern", "P", "Resulting drawing pattern", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double radius1 = 0;
            double speed1 = 0;
            double radius2 = 0;
            double speed2 = 0;
            double time = 0;

            if (!DA.GetData(0, ref radius1)) return;
            if (!DA.GetData(1, ref speed1)) return;
            if (!DA.GetData(2, ref radius2)) return;
            if (!DA.GetData(3, ref speed2)) return;
            if (!DA.GetData(4, ref time)) return;

            // Calculate positions based on time
            double t = time;
            Point3d point1 = new Point3d(radius1 * Math.Cos(speed1 * t), radius1 * Math.Sin(speed1 * t), 0);
            Point3d point2 = new Point3d(radius2 * Math.Cos(speed2 * t), radius2 * Math.Sin(speed2 * t), 0);

            // Combine the positions to create the pattern point
            Point3d patternPoint = point1 + point2;

            // Output the pattern point
            DA.SetDataList(0, new List<Point3d> { patternPoint });
        }

        public override Guid ComponentGuid => new Guid("94917c24-7423-4474-9a58-0f2815077c26");
    }

}