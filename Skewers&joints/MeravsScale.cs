using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using System.Drawing;
using Rhino.Geometry;
using System.Reflection;
using System.Drawing.Imaging;
using System.Resources;


namespace MoP
{
    public class MeravsScale : GH_Component
    {
        public MeravsScale()
          : base("MeravsScale", "MS",
              "Scales a geometry by a given factor from a specified center point",
              "Category", "Subcategory") {}

        public override Guid ComponentGuid => new Guid("5ff70a7e6f744b6e8d6c370498d3296f");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Base geometry", GH_ParamAccess.item);
            pManager.AddPointParameter("Center", "C", "Center of scaling", GH_ParamAccess.item, Point3d.Origin);
            pManager.AddNumberParameter("Factor", "S", "Scaling factor", GH_ParamAccess.item, 1.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Scaled geometry", GH_ParamAccess.item);
            pManager.AddTextParameter("Transform", "X", "Transformation Info", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geometry = null;
            double scaleFactor = 1.0;
            Point3d centerPoint = Point3d.Origin;

            if (!DA.GetData(0, ref geometry)) return;
            if (!DA.GetData(1, ref centerPoint)) return;
            if (!DA.GetData(2, ref scaleFactor)) return;

          
            Transform scaleTransform = Transform.Scale(centerPoint, scaleFactor);
            GeometryBase scaledGeometry = geometry.Duplicate();
            scaledGeometry.Transform(scaleTransform);

            DA.SetData(0, scaledGeometry);
            DA.SetData(1, scaleFactor);
        }


    }

   
    public class ScaleInfo : GH_AssemblyInfo
    {
        public override string Name => "MeravsScale";

        public override Bitmap Icon => null;
      
        public override string Description => "A component to scale geometries in Grasshopper";

        public override Guid Id => new Guid("226e4057-c4dd-4527-92d5-29997175c59f");

        public override string AuthorName => "Merav";

        public override string AuthorContact => "";
    }

    
}
