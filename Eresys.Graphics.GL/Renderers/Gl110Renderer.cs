using OpenGL;

namespace Eresys.Graphics.GL.Renderers
{
    public class Gl110Renderer : IGlRenderer
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
            // Setup model-view matrix (as previously)

            // Model-view matrix selector
            Gl.MatrixMode(MatrixMode.Modelview);
            // Load (reset) to identity
            Gl.LoadIdentity();
            // Multiply with rotation matrix (around Z axis)
            Gl.Rotate(_Angle, 0.0f, 0.0f, 1.0f);

            // Draw triangle using immediate mode

            // Setup & enable client states to specify vertex arrays, and use Gl.DrawArrays instead of Gl.Begin/End paradigm
            using (MemoryLock vertexArrayLock = new MemoryLock(_ArrayPosition))
            using (MemoryLock vertexColorLock = new MemoryLock(_ArrayColor))
            {
                // Note: the use of MemoryLock objects is necessary to pin vertex arrays since they can be reallocated by GC
                // at any time between the Gl.VertexPointer execution and the Gl.DrawArrays execution

                // Set current client memory pointer for position: a vertex of 2 floats
                Gl.VertexPointer(2, VertexPointerType.Float, 0, vertexArrayLock.Address);
                // Position is used for drawing
                Gl.EnableClientState(EnableCap.VertexArray);

                // Set current client memory pointer for color: a vertex of 3 floats
                Gl.ColorPointer(3, ColorPointerType.Float, 0, vertexColorLock.Address);
                // Color is used for drawing
                Gl.EnableClientState(EnableCap.ColorArray);

                // Note: enabled client state and client memory pointers are a GL state, and theorically they could be
                // set only at creation time. However, memory should be pinned for application lifetime.

                // Start drawing triangles (3 vertices -> 1 triangle)
                // Note: vertex attributes are streamed from client memory to GPU
                Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
            }
        }

        public void Update()
        {
            // Change triangle rotation
            _Angle = (_Angle + 0.1f) % 45.0f;
        }
    }
}
