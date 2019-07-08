using OpenGL;

namespace Eresys.Graphics.GL.Renderers
{
    public class Gl100Renderer : IGlRenderer
    {
        #region Common Data

        private static float _Angle = 0;

        /// <summary>
        /// Vertex position array.
        /// </summary>
        private static readonly float[] _ArrayPosition = new float[] {
            0.0f, 0.0f,
            1.0f, 0.0f,
            1.0f, 1.0f
        };

        /// <summary>
        /// Vertex color array.
        /// </summary>
        private static readonly float[] _ArrayColor = new float[] {
            1.0f, 0.0f, 0.0f,
            0.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f
        };

        #endregion

        public void Create()
        {
            // Setup projection matrix, only once since it remains fixed for the application lifetime

            // Projection matrix selector
            Gl.MatrixMode(MatrixMode.Projection);
            // Load (reset) to identity
            Gl.LoadIdentity();
            // Multiply with orthographic projection
            Gl.Ortho(0.0, 1.0, 0.0, 1.0, 0.0, 1.0);
        }

        public void Destroy()
        { }

        public void Render()
        {
            // Setup model-view matrix

            // Model-view matrix selector
            Gl.MatrixMode(MatrixMode.Modelview);
            // Load (reset) to identity
            Gl.LoadIdentity();
            // Multiply with rotation matrix (around Z axis)
            Gl.Rotate(_Angle, 0.0f, 0.0f, 1.0f);

            // Draw triangle using immediate mode (8 draw call)

            // Start drawing triangles
            Gl.Begin(PrimitiveType.Triangles);

            // Feed triangle data: color and position
            // Note: vertex attributes (color, texture coordinates, ...) are specified before position information
            // Note: vertex data is passed using method calls (performance killer!)
            Gl.Color3(1.0f, 0.0f, 0.0f); Gl.Vertex2(0.0f, 0.0f);
            Gl.Color3(0.0f, 1.0f, 0.0f); Gl.Vertex2(0.5f, 1.0f);
            Gl.Color3(0.0f, 0.0f, 1.0f); Gl.Vertex2(1.0f, 0.0f);

            // Triangles ends
            Gl.End();
        }

        public void Update()
        {
            // Change triangle rotation
            _Angle = (_Angle + 0.1f) % 45.0f;
        }
    }
}
