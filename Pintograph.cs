using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System;
using Rhino.Geometry.Intersect;

namespace MoP
{
    public class PintoGraph : GH_Component
    {
        public PintoGraph()
          : base("Pintograph", "Pinto", "Simulates a pintograph drawing machine", "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance", "d", "Distance between the centers of the two disks", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius1", "R1", "Radius of the first disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Speed1", "S1", "Speed of the first disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radius2", "R2", "Radius of the second disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Speed2", "S2", "Speed of the second disk", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time", "T", "Time parameter", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rod Length", "L1", "Rod 1 Length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rod Length", "L2", "Rod 2 Length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rod Length", "L3", "Rod 3 Length", GH_ParamAccess.item); 
            pManager.AddNumberParameter("Rod Length", "L4", "Rod 4 Length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rod Length", "L5", "Rod 5 Length", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rod Length", "L6", "Rod 6 Length", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Geometry", "G", "Generated pintograph geometry", GH_ParamAccess.list);
            pManager.AddPointParameter("Intersection", "I", " Rod intersection point", GH_ParamAccess.list);
            pManager.AddPointParameter("Path", "P", "Generated pintograph path", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double d = 0;
            double r1 = 0;
            double s1 = 0;
            double r2 = 0;
            double s2 = 0;
            double t = 0;
            double l1 = 0; // Length of the rod from A to H
            double l2 = 0; // Length of the rod from B to H
            double l3 = 0; // Length of the rod from H to C
            double l4 = 0; // Length of the rod from H to D
            double l5 = 0; // Length of the rod from C to E
            double l6 = 0; // Length of the rod from D to E

            if (!DA.GetData(0, ref d)) return;
            if (!DA.GetData(1, ref r1)) return;
            if (!DA.GetData(2, ref s1)) return;
            if (!DA.GetData(3, ref r2)) return;
            if (!DA.GetData(4, ref s2)) return;
            if (!DA.GetData(5, ref t)) return;
            if (!DA.GetData(6, ref l1)) return;
            if (!DA.GetData(7, ref l2)) return;
            if (!DA.GetData(8, ref l3)) return;
            if (!DA.GetData(9, ref l4)) return;
            if (!DA.GetData(10, ref l5)) return;
            if (!DA.GetData(11, ref l6)) return;

            // Disks centers
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(d, 0, 0);

            // Calculate points A and B on the edges of the circles
            Point3d A = new Point3d(p1.X + r1 * Math.Cos(s1 * t), p1.Y + r1 * Math.Sin(s1 * t), p1.Z);
            Point3d B = new Point3d(p2.X + r2 * Math.Cos(s2 * t), p2.Y + r2 * Math.Sin(s2 * t), p2.Z);


            // Find intersection point H of rods from A and B
            Point3d H = FindIntersection(A, l1, B, l2, 0);

            // Calculate points C and D
            Point3d C = H + (l4 / l1) * (H - A);
            Point3d D = H + (l3 / l2) * (H - B);


            Point3d E = FindIntersection(C, l5, D, l6, 1);

            // Create the circles
            Circle circle1 = new Circle(p1, r1);
            Circle circle2 = new Circle(p2, r2);

            // Create the rods
            Line rod1 = new Line(A, C);
            Line rod2 = new Line(B, D);
            Line rod3 = new Line(H, C);
            Line rod4 = new Line(H, D);
            Line rod5 = new Line(C, E);
            Line rod6 = new Line(D, E);

            List<Curve> geometry = new List<Curve>();
            geometry.Add(circle1.ToNurbsCurve());
            geometry.Add(circle2.ToNurbsCurve());
            geometry.Add(rod1.ToNurbsCurve());
            geometry.Add(rod2.ToNurbsCurve());
            geometry.Add(rod3.ToNurbsCurve());
            geometry.Add(rod4.ToNurbsCurve());
            geometry.Add(rod5.ToNurbsCurve());
            geometry.Add(rod6.ToNurbsCurve());

            DA.SetDataList(0, geometry);
            DA.SetData(1, H);
            DA.SetData(2, E);
        }

        private Point3d FindIntersection(Point3d P1, double L1, Point3d P2, double L2, int direction)
        {
            // Using the law of cosines to find intersection point
            double d = P1.DistanceTo(P2);
            double a = (L1 * L1 - L2 * L2 + d * d) / (2 * d);
            double h = Math.Sqrt(L1 * L1 - a * a);
            Point3d P = P1 + (a / d) * (P2 - P1);
            Vector3d offset = new Vector3d((P2.Y - P1.Y) * (h / d), (P1.X - P2.X) * (h / d), 0);
            Point3d E1 = P + offset;
            Point3d E2 = P - offset;

            if (direction == 0)
                return E1;
            else
                return E2;

        }



        public override Guid ComponentGuid => new Guid("362a53e0-a0d5-4347-9393-bfa44d102ff7");
    }
}