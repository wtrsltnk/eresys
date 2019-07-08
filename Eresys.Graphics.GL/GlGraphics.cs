using Eresys.Graphics.GL.Renderers;
using Khronos;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Eresys.Graphics.GL
{
    public class GlGraphics : IGraphics
    {
        public bool WireFrame { get; set; }
        public bool Lighting { get; set; }
        public bool AlphaBlending { get; set; }
        public bool TextureAlpha { get; set; }
        public byte Alpha { get; set; }
        public bool DepthBuffer { get; set; }
        public bool Filtering { get; set; }
        public Eresys.Camera Camera { get; set; }
        public Eresys.Math.Matrix WorldMatrix { get; set; }
        public float Brightness { get; set; }
        public float Contrast { get; set; }
        public float Gamma { get; set; }
        public bool FrameClearing { get; set; }

        /// <summary>
        /// Het hoofdvenster
        /// </summary>
        public static Form Form { get; private set; } = null;

        public event EventHandler Activated;
        public event EventHandler Deactivate;
        public event EventHandler Closed;

        private GlControl RenderControl;

        public void Startup()
        {
            Form = new Form();

            Form.Activated += Form_Activated;
            Form.Deactivate += Form_Deactivate;
            Form.FormClosed += Form_FormClosed;

            RenderControl = new GlControl();
            Form.SuspendLayout();
            // 
            // RenderControl
            // 
            RenderControl.Animation = true;
            RenderControl.AnimationTimer = false;
            RenderControl.BackColor = System.Drawing.Color.DimGray;
            RenderControl.ColorBits = 24u;
            RenderControl.DepthBits = 0u;
            RenderControl.Dock = System.Windows.Forms.DockStyle.Fill;
            RenderControl.Location = new System.Drawing.Point(0, 0);
            RenderControl.MultisampleBits = 0u;
            RenderControl.Name = "RenderControl";
            RenderControl.Size = new System.Drawing.Size(731, 428);
            RenderControl.StencilBits = 0u;
            RenderControl.TabIndex = 0;
            RenderControl.ContextCreated += new System.EventHandler<OpenGL.GlControlEventArgs>(this.RenderControl_ContextCreated);
            RenderControl.ContextDestroying += new System.EventHandler<OpenGL.GlControlEventArgs>(this.RenderControl_ContextDestroying);
            RenderControl.Render += new System.EventHandler<OpenGL.GlControlEventArgs>(this.RenderControl_Render);
            RenderControl.ContextUpdate += new System.EventHandler<OpenGL.GlControlEventArgs>(this.RenderControl_ContextUpdate);

            Form.Controls.Add(RenderControl);
            Form.ResumeLayout(false);

            Form.Show();
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Closed?.Invoke(sender, e);
        }

        private void Form_Deactivate(object sender, EventArgs e)
        {
            Deactivate?.Invoke(sender, e);
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            Activated?.Invoke(sender, e);
        }

        public int AddFont(string name, float size, bool bold, bool italic)
        {
            return 0;
        }

        public int AddTexture(Texture texture)
        {
            return 0;
        }

        private readonly Dictionary<int, VertexPool> _vertexPools = new Dictionary<int, VertexPool>());

        public int AddVertexPool(VertexPool vertexPool)
        {
            int newIndex = 0;
            while (_vertexPools.ContainsKey(newIndex))
            {
                newIndex++;
            }

            _vertexPools.Add(newIndex, vertexPool);

            return newIndex;
        }

        public void BeginFrame()
        {
            Gl.Viewport(0, 0, RenderControl.ClientSize.Width, RenderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Dispose()
        { }

        public void EndFrame()
        { }

        public void RemoveTexture(int textureIdx)
        { }

        public void RemoveVertexPool(int vertexPoolIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                return;
            }

            _vertexPools[vertexPoolIdx] = null;
        }


        public void RenderText(int fontIdx, Color color, Eresys.Math.Point2D position, string text)
        { }

        public void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth)
        { }

        public void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                throw new ArgumentException($"vertexpool {vertexPoolIdx} does not exist", nameof(vertexPoolIdx));
            }
        }

        public void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            if (!_vertexPools.ContainsKey(vertexPoolIdx))
            {
                throw new ArgumentException($"vertexpool {vertexPoolIdx} does not exist", nameof(vertexPoolIdx));
            }
        }

        public Texture TakeScreenshot()
        {
            return new Texture(256, 256, "screenshot.png");
        }

        private IGlRenderer _renderer;

        #region Event Handling

        /// <summary>
        /// Allocate GL resources or GL states.
        /// </summary>
        /// <param name="sender">
        /// The <see cref="object"/> that has rasied the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="GlControlEventArgs"/> that specifies the event arguments.
        /// </param>
        private void RenderControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            GlControl glControl = (GlControl)sender;

            // GL Debugging
            if (Gl.CurrentExtensions != null && Gl.CurrentExtensions.DebugOutput_ARB)
            {
                Gl.DebugMessageCallback(GLDebugProc, IntPtr.Zero);
                Gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }

            // Allocate resources and/or setup GL states
            switch (Gl.CurrentVersion.Api)
            {
                case KhronosVersion.ApiGl:
                    {
                        if (Gl.CurrentVersion >= Gl.Version_320)
                        {
                            _renderer = new Gl320Renderer();
                        }
                        else if (Gl.CurrentVersion >= Gl.Version_110)
                        {
                            _renderer = new Gl110Renderer();
                        }
                        else
                        {
                            _renderer = new Gl100Renderer();
                        }
                        break;
                    }
                case KhronosVersion.ApiGles2:
                    {
                        _renderer = new GlEs2Renderer();
                        break;
                    }
            }

            _renderer.Create();

            // Uses multisampling, if available
            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
            {
                Gl.Enable(EnableCap.Multisample);
            }
        }

        private void RenderControl_Render(object sender, GlControlEventArgs e)
        {
            Gl.Viewport(0, 0, RenderControl.ClientSize.Width, RenderControl.ClientSize.Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            _renderer.Render();
        }

        private void RenderControl_ContextUpdate(object sender, GlControlEventArgs e)
        {
            _renderer.Update();
        }

        private void RenderControl_ContextDestroying(object sender, GlControlEventArgs e)
        {
            _renderer.Destroy();
        }

        private static void GLDebugProc(DebugSource source, DebugType type, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string strMessage;

            // Decode message string
            unsafe
            {
                strMessage = Encoding.ASCII.GetString((byte*)message.ToPointer(), length);
            }

            Console.WriteLine($"{source}, {type}, {severity}: {strMessage}");
        }

        #endregion
    }
}
