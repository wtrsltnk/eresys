using Eresys.Math;
using System.Collections;

namespace Eresys
{
    public class DummyControls : IControls
    {
        public ICollection GetActiveKeys()
        {
            return new ArrayList();
        }

        public Point3D GetMousePosition()
        {
            return new Point3D();
        }

        public bool IsKeyActive(Key key)
        {
            return false;
        }

        public bool IsMouseButtonActive(MouseButton button)
        {
            return false;
        }

        public void Update()
        { }
    }
}
