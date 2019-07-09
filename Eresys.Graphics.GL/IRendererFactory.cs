using Khronos;

namespace Eresys.Graphics.GL
{
    public interface IRendererFactory
    {
        IGlRenderer Create(KhronosVersion version);
    }
}
