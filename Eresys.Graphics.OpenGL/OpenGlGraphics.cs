﻿using System;
using System.Windows.Forms;

namespace Eresys.Graphics.OpenGL
{
    public class OpenGlGraphics : IGraphics
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

        public void Startup()
        {
            Form = new Form();

            Form.Activated += Form_Activated;
            Form.Deactivate += Form_Deactivate;
            Form.FormClosed += Form_FormClosed;

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
            throw new System.NotImplementedException();
        }

        public int AddTexture(Texture texture)
        {
            throw new System.NotImplementedException();
        }

        public int AddVertexPool(VertexPool vertexPool)
        {
            throw new System.NotImplementedException();
        }

        public void BeginFrame()
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public void EndFrame()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveTexture(int textureIdx)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveVertexPool(int vertexPoolIdx)
        {
            throw new System.NotImplementedException();
        }

        public void RenderText(int fontIdx, Color color, Eresys.Math.Point2D position, string text)
        {
            throw new System.NotImplementedException();
        }

        public void RenderTexture(int textureIdx, float left, float top, float width, float height, float depth)
        {
            throw new System.NotImplementedException();
        }

        public void RenderTriangleFan(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            throw new System.NotImplementedException();
        }

        public void RenderTriangleStrip(int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx)
        {
            throw new System.NotImplementedException();
        }

        public Texture TakeScreenshot()
        {
            throw new System.NotImplementedException();
        }
    }
}
