using OpenGL;

namespace Eresys.Graphics.GL
{
    public interface IGlRenderer
    {
        void Create();
        void Destroy();

        int AddVertexPool(VertexPool vertexPool);
        void RemoveVertexPool(int vertexPoolIdx);

        void RenderTriangleFan(float[] matrix, int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx);
        void RenderTriangleStrip(float[] matrix, int vertexPoolIdx, int first, int count, int textureIdx, int lightmapIdx);
    }
}
