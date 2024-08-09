using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace MoP
{
    public class PintoGraph : GH_Component
    {
        private Stopwatch stopwatch;
        private bool running = false;
        private double runtime = 0;
        private List<Point3d> pathPoints = new List<Point3d>(); // To store positions of point P

        public PintoGraph()
          : base("Pintograph", "Pintograph", "Simulates a pintograph drawing machine", "Category", "Subcategory")
        {
            stopwatch = new Stopwatch();
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Start", "Start", "Start the simulation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset the simulation", GH_ParamAccess.item); // Add Reset button
            pManager.AddNumberParameter("Runtime", "Runtime", "Time to run the script for in seconds", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "Distance", "Distance between the centers of the two disks", GH_ParamAccess.item);
            pManager.AddNumberParameter("Radii", "Radii", "List of radii of the disks", GH_ParamAccess.list);
            pManager.AddNumberParameter("Speeds", "Speeds", "List of speeds of the disks (in RPS)", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Directions", "Directions", "List of rotation directions of the disks (true for clockwise, false for counterclockwise)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rod Lengths", "Rod Lengths", "List of rod lengths", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Geometry", "G", "Generated pintograph geometry", GH_ParamAccess.list);
            pManager.AddPointParameter("First Intersection", "I1", "First rod intersection point", GH_ParamAccess.item);
            pManager.AddPointParameter("Second Intersection", "I2", "Second rod intersection point", GH_ParamAccess.item);
            pManager.AddPointParameter("Current Position", "P", "Current position of point P", GH_ParamAccess.item);
            pManager.AddCurveParameter("Path", "Path", "Trace of point P over time", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool start = false;
            bool reset = false; // Variable for the reset button
            double d = 0;
            List<double> radii = new List<double>();
            List<double> speeds = new List<double>();
            List<bool> directions = new List<bool>();
            List<double> lengths = new List<double>();

            if (!DA.GetData(0, ref start)) return;
            if (!DA.GetData(1, ref reset)) return; // Get reset data
            if (!DA.GetData(2, ref runtime)) return;
            if (!DA.GetData(3, ref d)) return;
            if (!DA.GetDataList(4, radii)) return;
            if (!DA.GetDataList(5, speeds)) return;
            if (!DA.GetDataList(6, directions)) return;
            if (!DA.GetDataList(7, lengths)) return;

            // Handle Reset Button Logic
            if (reset)
            {
                running = false;
                stopwatch.Reset();
                pathPoints.Clear(); // Clear previous path points
                DA.SetData(4, null); // Clear the path output
                return;
            }

            // Validate list lengths
            if (radii.Count != 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Radii list must contain exactly 2 values.");
                return;
            }
            if (speeds.Count != 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Speeds list must contain exactly 2 values.");
                return;
            }
            if (directions.Count != 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Directions list must contain exactly 2 values.");
                return;
            }
            if (lengths.Count != 7)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Rod Lengths list must contain exactly 7 values.");
                return;
            }

            // Handle Start Button Logic
            if (start && !running)
            {
                stopwatch.Restart();
                running = true;
            }

            // Check if simulation should stop
            if (!running || stopwatch.Elapsed.TotalSeconds > runtime)
            {
                running = false;
                stopwatch.Stop();
                // Output the path even after simulation stops
                if (pathPoints.Count > 1)
                {
                    Polyline pathPolyline = new Polyline(pathPoints);
                    DA.SetData(4, pathPolyline);
                }
                return;
            }

            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            // Extract individual values from the lists
            double r1 = radii[0];
            double r2 = radii[1];
            double s1 = speeds[0];
            double s2 = speeds[1];
            bool direction1 = directions[0];
            bool direction2 = directions[1];
            double l1 = lengths[0];
            double l2 = lengths[1];
            double l3 = lengths[2];
            double l4 = lengths[3];
            double l5 = lengths[4];
            double l6 = lengths[5];
            double l7 = lengths[6];

            // Convert speeds from RPS to radians per second
            double omega1 = s1 * 2 * Math.PI * (direction1 ? -1 : 1);
            double omega2 = s2 * 2 * Math.PI * (direction2 ? -1 : 1);

            // Disk centers
            Point3d p1 = new Point3d(0, 0, 0);
            Point3d p2 = new Point3d(d, 0, 0);

            // Calculate points A and B on the edges of the circles
            Point3d A = new Point3d(p1.X + r1 * Math.Cos(omega1 * elapsedSeconds), p1.Y + r1 * Math.Sin(omega1 * elapsedSeconds), p1.Z);
            Point3d B = new Point3d(p2.X + r2 * Math.Cos(omega2 * elapsedSeconds), p2.Y + r2 * Math.Sin(omega2 * elapsedSeconds), p2.Z);

            // Find intersection point H of rods from A and B
            Point3d H = FindIntersection(A, l1, B, l2, 0);

            // Calculate points C and D
            Point3d C = H + (l4 / l1) * (H - A);
            Point3d D = H + (l3 / l2) * (H - B);

            // Calculate points E and P
            Point3d E = FindIntersection(C, l5, D, l6, 1);
            Point3d P = E + (l7 / l5) * (E - C);

            // Store the current position of P
            pathPoints.Add(P);

            // Create the circles
            Circle circle1 = new Circle(p1, r1);
            Circle circle2 = new Circle(p2, r2);

            // Create the rods
            Line rod1 = new Line(A, C);
            Line rod2 = new Line(B, D);
            Line rod3 = new Line(H, C);
            Line rod4 = new Line(H, D);
            Line rod5 = new Line(C, P);
            Line rod6 = new Line(D, E);

            List<Curve> geometry = new List<Curve>
            {
                circle1.ToNurbsCurve(),
                circle2.ToNurbsCurve(),
                rod1.ToNurbsCurve(),
                rod2.ToNurbsCurve(),
                rod3.ToNurbsCurve(),
                rod4.ToNurbsCurve(),
                rod5.ToNurbsCurve(),
                rod6.ToNurbsCurve()
            };

            // Output data
            DA.SetDataList(0, geometry);
            DA.SetData(1, H); // First intersection
            DA.SetData(2, E); // Second intersection
            DA.SetData(3, P); // Current position of P

            // Output the path as a polyline if there are enough points
            if (pathPoints.Count > 1)
            {
                Polyline pathPolyline = new Polyline(pathPoints);
                DA.SetData(4, pathPolyline);
            }

            // Expire the solution to continuously update
            this.ExpireSolution(true);
        }

        private Point3d FindIntersection(Point3d P1, double L1, Point3d P2, double L2, int direction)
        {
            // Using the law of cosines to find intersection point
            double d = P1.DistanceTo(P2);

            // Check for valid triangle
            if (d > L1 + L2 || d < Math.Abs(L1 - L2) || d == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid rod lengths or positions for intersection.");
                return Point3d.Unset;
            }

            double a = (L1 * L1 - L2 * L2 + d * d) / (2 * d);
            double h = Math.Sqrt(L1 * L1 - a * a);

            Point3d P3 = P1 + a * (P2 - P1) / d;

            double offsetX = h * (P2.Y - P1.Y) / d;
            double offsetY = h * (P2.X - P1.X) / d;

            if (direction == 0)
            {
                return new Point3d(P3.X + offsetX, P3.Y - offsetY, 0);
            }
            else
            {
                return new Point3d(P3.X - offsetX, P3.Y + offsetY, 0);
            }
        }

        protected override System.Drawing.Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("6b5b5ab7-b10a-4e87-a6ae-b9b5a1f5b91f");
    }
}
