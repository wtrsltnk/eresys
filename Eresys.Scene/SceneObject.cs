using Eresys.Math;
using Eresys.Practises.Profiling;

namespace Eresys
{
    /// <summary>
    /// Summary description for SceneObject.
    /// </summary>
    public class SceneObject
    {
        public Point3D Position
        {
            get { return position; }
            set { position = value; changed = true; }
        }

        public Point3D Direction
        {
            get { return direction; }
            set { direction = value; changed = true; }
        }

        public Vector Speed { get; set; }

        public Vector Rotation { get; set; }

        public virtual bool Visible { get; set; }

        public virtual bool Solid { get { return false; } }

        public IProfiler Profiler { get; set; } = new NullProfiler();

        public Matrix WorldMatrix { get; }

        public SceneObject()
        {
            this.position = new Point3D();
            this.direction = new Point3D();
            this.Speed = new Vector();
            this.Rotation = new Vector();
            this.changed = true;
            this.Visible = false;
            //Kernel.Scene.AddObject(this);
            WorldMatrix = new Matrix();
        }

        public SceneObject(Point3D position, Point3D direction)
        {
            this.position = position;
            this.direction = direction;
            this.Speed = new Vector();
            this.Rotation = new Vector();
            this.changed = true;
            //Kernel.Scene.AddObject(this);
        }

        public SceneObject(Point3D position, Point3D direction, Vector speed, Vector rotation)
        {
            this.position = position;
            this.direction = direction;
            this.Speed = speed;
            this.Rotation = rotation;
            this.changed = true;
            //Kernel.Scene.AddObject(this);
        }

        public virtual void Update(float interval)
        {
            Position += Speed * interval;
            Direction += Rotation * interval;
        }

        public virtual void Render(IGraphics graphics, CameraSceneObject camera)
        {
        }

        protected virtual void OnChange()
        {
            // mworld = ...
        }

        public void Change()
        {
            if (changed)
            {
                OnChange();
                changed = false;
            }
        }

        public virtual Vector CheckCollision(Point3D start, Vector movement, float sphere)
        {
            return movement;
        }

        protected bool changed;

        private Point3D position;
        private Point3D direction;
    }
}
