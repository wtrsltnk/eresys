using Eresys.Practises.Logging;
using System.Collections;

namespace Eresys
{
    public class Scene
    {
        private readonly ArrayList _objects;

        public ILogger Logger { get; set; } = new ConsoleLogger();

        public Player player { get; set; }

        public SceneObject this[int index] => (SceneObject)_objects[index];

        public int Count => _objects.Count;

        public Scene()
        {
            Logger.Log(LogLevels.Info, "Initialising Scene...");
            _objects = new ArrayList();
            player = null;
        }

        public void AddObject(SceneObject obj)
        {
            if (!_objects.Contains(obj))
                _objects.Add(obj);
        }

        public void RemoveObject(SceneObject obj)
        {
            _objects.Remove(obj);
        }

        public void Update(float interval)
        {
            // update objects
            for (int i = 0; i < _objects.Count; i++)
            {
                ((SceneObject)_objects[i]).Update(interval);
            }
        }

        public void Render(IGraphics graphics)
        {
            if (player == null) return;
            for (int i = 0; i < _objects.Count; i++)
            {
                ((SceneObject)_objects[i]).Change();
            }
            //Kernel.Graphics.Camera = player.Camera;
            for (int i = 0; i < _objects.Count; i++)
            {
                SceneObject obj = ((SceneObject)(_objects[i]));
                //Kernel.Graphics.WorldMatrix = obj.WorldMatrix;
                if (obj.Visible) obj.Render(graphics, player.Camera);
            }
        }
}
}
