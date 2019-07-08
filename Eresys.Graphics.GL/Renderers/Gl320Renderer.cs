using Eresys.Graphics.GL.Models;
using OpenGL;

namespace Eresys.Graphics.GL.Renderers
{
    public class Gl320Renderer : IGlRenderer
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

        private readonly string[] _VertexSourceGL = {
            "#version 150 compatibility\n",
            "uniform mat4 uMVP;\n",
            "in vec2 aPosition;\n",
            "in vec3 aColor;\n",
            "out vec3 vColor;\n",
            "void main() {\n",
            "	gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);\n",
            "	vColor = aColor;\n",
            "}\n"
        };

        private readonly string[] _FragmentSourceGL = {
            "#version 150 compatibility\n",
            "in vec3 vColor;\n",
            "void main() {\n",
            "	gl_FragColor = vec4(vColor, 1.0);\n",
            "}\n"
        };

        #endregion

        #region Common Shading

        // Note: abstractions for drawing using programmable pipeline.

        /// <summary>
        /// The program used for drawing the triangle.
        /// </summary>
        private GlProgram _Program;

        /// <summary>
        /// The vertex arrays used for drawing the triangle.
        /// </summary>
        private GlVertexArray _VertexArray;

        #endregion

        public void Create()
        {
            _Program = new GlProgram(_VertexSourceGL, _FragmentSourceGL);
            _VertexArray = new GlVertexArray(_Program, _ArrayPosition, _ArrayColor);
        }

        public void Destroy()
        {
            _Program?.Dispose();
            _VertexArray?.Dispose();
        }

        public void Render()
        {
            // Compute the model-view-projection on CPU
            Matrix4x4f projection = Matrix4x4f.Ortho2D(-1.0f, +1.0f, -1.0f, +1.0f);
            Matrix4x4f modelview = Matrix4x4f.Translated(-0.5f, -0.5f, 0.0f) * Matrix4x4f.RotatedZ(_Angle);

            // Select the program for drawing
            Gl.UseProgram(_Program.ProgramName);
            // Set uniform state
            Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, projection * modelview);
            // Use the vertex array
            Gl.BindVertexArray(_VertexArray.ArrayName);
            // Draw triangle
            // Note: vertex attributes are streamed from GPU memory
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        public void Update()
        {
            // Change triangle rotation
            _Angle = (_Angle + 0.1f) % 45.0f;
        }
    }
}
