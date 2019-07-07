using Eresys.Graphics.GL.Models;
using OpenGL;

namespace Eresys.Graphics.GL.Renderers
{
    public class GlEs2Renderer : IGlRenderer
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

        private readonly string[] _VertexSourceGLES2 = {
            "uniform mat4 uMVP;\n",
            "attribute vec2 aPosition;\n",
            "attribute vec3 aColor;\n",
            "varying vec3 vColor;\n",
            "void main() {\n",
            "	gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);\n",
            "	vColor = aColor;\n",
            "}\n"
        };

        private readonly string[] _FragmentSourceGLES2 = {
            "precision mediump float;\n",
            "varying vec3 vColor;\n",
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

        #endregion

        public void Create()
        {
            // Create program
            _Program = new GlProgram(_VertexSourceGLES2, _FragmentSourceGLES2);
        }

        public void Destroy()
        {
            _Program?.Dispose();
        }

        public void Render()
        {
            Matrix4x4f projection = Matrix4x4f.Ortho2D(0.0f, 1.0f, 0.0f, 1.0f);
            Matrix4x4f modelview = Matrix4x4f.RotatedZ(_Angle);

            Gl.UseProgram(_Program.ProgramName);

            using (MemoryLock arrayPosition = new MemoryLock(_ArrayPosition))
            using (MemoryLock arrayColor = new MemoryLock(_ArrayColor))
            {
                Gl.VertexAttribPointer((uint)_Program.LocationPosition, 2, VertexAttribType.Float, false, 0, arrayPosition.Address);
                Gl.EnableVertexAttribArray((uint)_Program.LocationPosition);

                Gl.VertexAttribPointer((uint)_Program.LocationColor, 3, VertexAttribType.Float, false, 0, arrayColor.Address);
                Gl.EnableVertexAttribArray((uint)_Program.LocationColor);

                Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, projection * modelview);

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
