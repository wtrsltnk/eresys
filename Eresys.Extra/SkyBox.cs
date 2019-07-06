using Eresys.Math;

namespace Eresys
{
    public class SkyBox : SceneObject
    {
        public SkyBox(IGraphics graphics, float width, float height, float depth, Texture texture)
        {
            var x = width / 2.0f;
            var y = height / 2.0f;
            var z = depth / 2.0f;

            var vp = new VertexPool(NumVertices);

            vp[0] = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.0000f, 0.5f));
            vp[1] = new Vertex(new Point3D(-x, y, -z), new Point2D(0.0000f, 0.0f));
            vp[2] = new Vertex(new Point3D(-x, -y, z), new Point2D(0.3333f, 0.5f));
            vp[3] = new Vertex(new Point3D(-x, y, z), new Point2D(0.3333f, 0.0f));
            vp[4] = new Vertex(new Point3D(x, -y, z), new Point2D(0.6666f, 0.5f));
            vp[5] = new Vertex(new Point3D(x, y, z), new Point2D(0.6666f, 0.0f));
            vp[6] = new Vertex(new Point3D(x, -y, -z), new Point2D(1.0000f, 0.5f));
            vp[7] = new Vertex(new Point3D(x, y, -z), new Point2D(1.0000f, 0.0f));

            vp[8] = new Vertex(new Point3D(-x, -y, z), new Point2D(0.0000f, 1.0f));
            vp[9] = new Vertex(new Point3D(x, -y, z), new Point2D(0.0000f, 0.5f));
            vp[10] = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.3333f, 1.0f));
            vp[11] = new Vertex(new Point3D(x, -y, -z), new Point2D(0.3333f, 0.5f));
            vp[12] = new Vertex(new Point3D(-x, y, -z), new Point2D(0.6666f, 1.0f));
            vp[13] = new Vertex(new Point3D(x, y, -z), new Point2D(0.6666f, 0.5f));
            vp[14] = new Vertex(new Point3D(-x, y, z), new Point2D(1.0000f, 1.0f));
            vp[15] = new Vertex(new Point3D(x, y, z), new Point2D(1.0000f, 0.5f));

            /*
			vp[0]  = new Vertex(new Point3D(-x, -y, -z), new Point2D(1.0000f, 0.5f));
			vp[1]  = new Vertex(new Point3D(-x,  y, -z), new Point2D(1.0000f, 0.0f));
			vp[2]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(0.6666f, 0.5f));
			vp[3]  = new Vertex(new Point3D(-x,  y,  z), new Point2D(0.6666f, 0.0f));
			vp[4]  = new Vertex(new Point3D( x, -y,  z), new Point2D(0.3333f, 0.5f));
			vp[5]  = new Vertex(new Point3D( x,  y,  z), new Point2D(0.3333f, 0.0f));
			vp[6]  = new Vertex(new Point3D( x, -y, -z), new Point2D(0.0000f, 0.5f));
			vp[7]  = new Vertex(new Point3D( x,  y, -z), new Point2D(0.0000f, 0.0f));

			vp[8]  = new Vertex(new Point3D(-x, -y,  z), new Point2D(1.0000f, 1.0f));
			vp[9]  = new Vertex(new Point3D( x, -y,  z), new Point2D(1.0000f, 0.5f));
			vp[10] = new Vertex(new Point3D(-x, -y, -z), new Point2D(0.6666f, 1.0f));
			vp[11] = new Vertex(new Point3D( x, -y, -z), new Point2D(0.6666f, 0.5f));
			vp[12] = new Vertex(new Point3D(-x,  y, -z), new Point2D(0.3333f, 1.0f));
			vp[13] = new Vertex(new Point3D( x,  y, -z), new Point2D(0.3333f, 0.5f));
			vp[14] = new Vertex(new Point3D(-x,  y,  z), new Point2D(0.0000f, 1.0f));
			vp[15] = new Vertex(new Point3D( x,  y,  z), new Point2D(0.0000f, 0.5f));
			*/

            VertexPoolIndex = graphics.AddVertexPool(vp);

            TextureIndex = graphics.AddTexture(texture);
        }

        public override void Render(IGraphics graphics, CameraSceneObject camera)
        {
            graphics.Lighting = false;

            var oldMatrix = graphics.WorldMatrix;
            graphics.WorldMatrix = Matrix.Translation(camera.Position.x, camera.Position.y, camera.Position.z);

            graphics.Filtering = false;

            graphics.RenderTriangleStrip(VertexPoolIndex, 0, 8, TextureIndex, -1);
            graphics.RenderTriangleStrip(VertexPoolIndex, 8, 8, TextureIndex, -1);

            graphics.Filtering = true;

            graphics.WorldMatrix = oldMatrix;
        }

        private const ushort NumVertices = 16;

        public int VertexPoolIndex { get; set; }

        public int TextureIndex { get; set; }
    }
}
