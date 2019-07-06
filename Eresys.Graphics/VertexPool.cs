namespace Eresys
{
    /// <summary>
    /// Stelt een lijst vertices voor. Deze kan mbv. Kernel.Graphics worden ge-upload naar de grafische hardware om gebruikt
    /// te worden bij het renderen.
    /// </summary>
    public class VertexPool
    {
        public Vertex[] Vertices { get; set; }

        public Vertex this[int index]
        {
            get { return Vertices[index]; }
            set { Vertices[index] = value; }
        }

        public int Size => Vertices.Length;

        public VertexPool(int size)
        {
            Vertices = new Vertex[size];
        }

        public object GetData()
        {
            return Vertices;
        }
    }
}
