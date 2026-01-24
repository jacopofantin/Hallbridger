        private Rect3D CalculateModelBounds(IfcStore model)
        {
            var context = new Xbim3DModelContext(model);
            context.CreateContext();

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var shapeInstance in context.ShapeInstances())
            {
                var bbox = shapeInstance.BoundingBox;

                minX = Math.Min(minX, bbox.Min.X);
                minY = Math.Min(minY, bbox.Min.Y);
                minZ = Math.Min(minZ, bbox.Min.Z);
                maxX = Math.Max(maxX, bbox.Max.X);
                maxY = Math.Max(maxY, bbox.Max.Y);
                maxZ = Math.Max(maxZ, bbox.Max.Z);
            }

            if (minX == double.MaxValue) // kein Shape gefunden
                return Rect3D.Empty;

            return new Rect3D(minX, minY, minZ, maxX - minX, maxY - minY, maxZ - minZ);
        }

        private void Adjust3dViewZoom()
        {
            var model = hall3dModelViewer.Model as IfcStore;
            if (model == null)
                return;

            var bounds = CalculateModelBounds(model);
            if (bounds.IsEmpty)
                return;

            double margin = 1.05; // 5% Rand
            double width = bounds.SizeX * margin;


            var center = new Point3D(
                bounds.X + bounds.SizeX / 2,
                bounds.Y + bounds.SizeY / 2,
                bounds.Z + bounds.SizeZ / 2);

            var camera = new PerspectiveCamera
            {
                Position = new Point3D(189.31325962662, -189.31325962662, 56.7939778879859),
                LookDirection = new Vector3D(-189.31325962662, 189.31325962662, -56.7939778879859),
                UpDirection = new Vector3D(0, 0, 1),
                FieldOfView = 61.0
            };

            hall3dModelViewer.SetCamera(camera);
        }