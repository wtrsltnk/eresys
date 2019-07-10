using Eresys.Math;
using Khronos;
using OpenGL;
using System;
using System.Text;
using System.Windows.Forms;

namespace Eresys.Graphics.GL
{
    public class GlGraphics : IGraphics
    {
        #region Dependancies
        private readonly IRendererFactory _rendererFactory;
        #endregion

        public GlGraphics(
            IRendererFactory rendererFactory)
        {
            _rendererFactory = rendererFactory ?? throw new ArgumentNullException(nameof(rendererFactory));
        }

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

        public event EventHandler ContextCreated;
        public event EventHandler Update;
        public event EventHandler Render;
        public event EventHandler ContextDestroying;
        public event EventHandler Activated;
        public event EventHandler Deactivate;
        public event EventHandler Closed;

        private GlControl RenderControl;

        public void Startup()
        {
            Form = new Form();

            Form.Activated += (sender, e) => Activated?.Invoke(sender, e);
            Form.Deactivate += (sender, e) => Deactivate?.Invoke(sender, e);
            Form.FormClosed += (sender, e) => Closed?.Invoke(sender, e);

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
            RenderControl.Dock = DockStyle.Fill;
            RenderControl.Location = new System.Drawing.Point(0, 0);
            RenderControl.MultisampleBits = 0u;
            RenderControl.Name = "RenderControl";
            RenderControl.Size = new System.Drawing.Size(731, 428);
            RenderControl.StencilBits = 0u;
            RenderControl.TabIndex = 0;
            RenderControl.ContextCreated += RenderControl_ContextCreated;
            RenderControl.ContextDestroying += (sender, e) => ContextDestroying?.Invoke(sender, e);
            RenderControl.Render += (sender, e) => Render?.Invoke(sender, e);
            RenderControl.ContextUpdate += (sender, e) => Update?.Invoke(sender, e);
            RenderControl.SizeChanged += RenderControl_SizeChanged;

            Form.Controls.Add(RenderControl);
            Form.ResumeLayout(false);

            Form.Show();

            WorldMatrix = Matrix.OrthoRH(-1000.0f, 1000.0f, -1000.0f, 1000.0f, -1000.0f, 1000.0f);
            //WorldMatrix = Matrix.Translation(0, 0, 0);
        }

        private void RenderControl_SizeChanged(object sender, EventArgs e)
        {
            var aspect = (float)RenderControl.Width / (float)RenderControl.Height;
            WorldMatrix = Matrix.OrthoRH(-1000.0f * aspect, 1000.0f * aspect, -1000.0f, 1000.0f, -1000.0f, 1000.0f);
        }

        #region Event Handling

        private IGlRenderer _renderer;

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

            _renderer = _rendererFactory.Create(Gl.CurrentVersion);

            _renderer.Create();

            ContextCreated?.Invoke(sender, e);

            Gl.ClearColor(0.0f, 0.4f, 0.6f, 1.0f);

            // Uses multisampling, if available
            if (Gl.CurrentVersion != null && Gl.CurrentVersion.Api == KhronosVersion.ApiGl && glControl.MultisampleBits > 0)
            {
                Gl.Enable(EnableCap.Multisample);
            }
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

        public int AddFont(string name, float size, bool bold, bool italic)
        {
            return 0;
        }

        public int AddTexture(Texture texture)
        {
            return 0;
        }

        public int AddVertexPool(VertexPool vertexPool)
        {
            return _renderer.AddVertexPool(vertexPool);
        }

        public void RemoveVertexPool(int vertexPoolIdx)
        {
            _renderer.RemoveVertexPool(vertexPoolIdx);
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

        public void RenderText(int fontIdx, Color color, Point2D position, string text)
        { }

        public void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth)
        { }

        public void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            _renderer.RenderTriangleFan(WorldMatrix.ToFloatArray(), vertexPoolIdx, first, count, textureIdx, lightmapIdx);
        }

        public void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            _renderer.RenderTriangleStrip(WorldMatrix.ToFloatArray(), vertexPoolIdx, first, count, textureIdx, lightmapIdx);
        }

        public Texture TakeScreenshot()
        {
            return new Texture(256, 256, "screenshot.png");
        }
    }
}
