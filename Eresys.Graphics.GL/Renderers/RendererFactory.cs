using Khronos;
using OpenGL;

namespace Eresys.Graphics.GL.Renderers
{
    public class RendererFactory : IRendererFactory
    {
        public IGlRenderer Create(KhronosVersion version)
        {
            switch (version.Api)
            {
                case KhronosVersion.ApiGl:
                    {
                        if (version >= Gl.Version_320)
                        {
                            return new Gl320Renderer();
                        }
                        else if (Gl.CurrentVersion >= Gl.Version_110)
                        {
                            return new Gl110Renderer();
                        }
                        break;
                    }
                case KhronosVersion.ApiGles2:
                    {
                        return new GlEs2Renderer();
                    }
            }

            return new Gl100Renderer();
        }
    }
}
