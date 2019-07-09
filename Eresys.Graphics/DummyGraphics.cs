using Eresys.Math;
using System;
using System.Windows.Forms;

namespace Eresys
{
    public class DummyGraphics : IGraphics
    {
        public bool WireFrame { get; set; }
        public bool Lighting { get; set; }
        public bool AlphaBlending { get; set; }
        public bool TextureAlpha { get; set; }
        public byte Alpha { get; set; }
        public bool DepthBuffer { get; set; }
        public bool Filtering { get; set; }
        public Camera Camera { get; set; }
        public Matrix WorldMatrix { get; set; }
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
        public event EventHandler ContextCreated;
        public event EventHandler Update;
        public event EventHandler Render;
        public event EventHandler ContextDestroying;

        public void Startup()
        {
            Form = new Form();

            Form.Activated += Form_Activated;
            Form.Deactivate += Form_Deactivate;
            Form.FormClosed += Form_Closed;

            Form.Show();
        }

        private void Form_Closed(object sender, FormClosedEventArgs e)
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

        public int AddVertexPool(VertexPool vertexPool)
        {
            return 0;
        }

        public void BeginFrame()
        { }

        public void Dispose()
        {
            Form.Close();
        }

        public void EndFrame()
        { }

        public void RemoveTexture(int textureIdx)
        { }

        public void RemoveVertexPool(int vertexPoolIdx)
        { }

        public void RenderText(int fontIdx, Color color, Point2D position, string text)
        { }

        public void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth)
        { }

        public void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        { }

        public void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        { }

        public Texture TakeScreenshot()
        {
            return new Texture(256, 256, "Screenshot");
        }
    }
}
