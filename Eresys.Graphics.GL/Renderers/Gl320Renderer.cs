using Eresys.Graphics.GL.Models;
using OpenGL;
using System;
using System.Collections.Generic;

namespace Eresys.Graphics.GL.Renderers
{
    public class Gl320Renderer : IGlRenderer
    {
        #region Common Data
        
        private readonly string[] _VertexSourceGL = {
            "#version 150 compatibility\n",
            "uniform mat4 uMVP;\n",
            "in vec3 aPosition;\n",
            "in vec3 aColor;\n",
            "out vec3 vColor;\n",
            "void main() {\n",
            "	gl_Position = uMVP * vec4(aPosition, 1.0);\n",
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
        
        #endregion

        public void Create()
        {
            _Program = new GlProgram(_VertexSourceGL, _FragmentSourceGL);
        }

        public void Destroy()
        {
            _Program?.Dispose();
        }
        
        #region Vertex Pools
        private readonly Dictionary<int, GlVertexArray> _vertexPools = new Dictionary<int, GlVertexArray>();

        public int AddVertexPool(VertexPool vertexPool)
        {
            int newIndex = 0;
            while (_vertexPools.ContainsKey(newIndex))
            {
                newIndex++;
            }

            _vertexPools.Add(newIndex, GlVertexArray.FromVertexPool(_Program, vertexPool));

            return newIndex;
        }

        public void RemoveVertexPool(int vertexPoolIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                return;
            }

            _vertexPools[vertexPoolIdx] = null;
        }

        Matrix4x4f projection = Matrix4x4f.Ortho(-1000.0f, 1000.0f, -1000.0f, 1000.0f, -1000.0f, 1000.0f);

        public void RenderTriangleFan(float[] matrix, int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                throw new ArgumentException($"vertexpool {vertexPoolIdx} does not exist", nameof(vertexPoolIdx));
            }
            
            // Select the program for drawing
            Gl.UseProgram(_Program.ProgramName);
            // Set uniform state
            //Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, projection);
            Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, new Matrix4x4f(matrix));
            // Use the vertex array
            Gl.BindVertexArray(_vertexPools[vertexPoolIdx].ArrayName);
            // Draw triangle
            // Note: vertex attributes are streamed from GPU memory
            Gl.DrawArrays(PrimitiveType.TriangleFan, first, count);
        }

        public void RenderTriangleStrip(float[] matrix, int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                throw new ArgumentException($"vertexpool {vertexPoolIdx} does not exist", nameof(vertexPoolIdx));
            }
            
            // Select the program for drawing
            Gl.UseProgram(_Program.ProgramName);
            // Set uniform state
            //Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, projection);
            Gl.UniformMatrix4f(_Program.LocationMVP, 1, false, new Matrix4x4f(matrix));
            // Use the vertex array
            Gl.BindVertexArray(_vertexPools[vertexPoolIdx].ArrayName);
            // Draw triangle
            // Note: vertex attributes are streamed from GPU memory
            Gl.DrawArrays(PrimitiveType.TriangleStrip, first, count);
        }
        #endregion
    }
}
