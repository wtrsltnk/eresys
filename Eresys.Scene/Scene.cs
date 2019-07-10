using Eresys.Practises.Logging;
using System.Collections;
using System.Collections.Generic;

namespace Eresys
{
    public class Scene
    {
        private readonly List<SceneObject> _objects;

        public ILogger Logger { get; set; } = new ConsoleLogger();

        public Player player { get; set; }

        public SceneObject this[int index] => _objects[index];

        public int Count => _objects.Count;

        public Scene()
        {
            Logger.Log(LogLevels.Info, "Initialising Scene...");
            _objects = new List<SceneObject>();
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
            foreach (var obj in _objects)
            {
                obj.Change();
            }
            //Kernel.Graphics.Camera = player.Camera;
            foreach (var obj in _objects)
            {
                if (obj.Visible)
                {
                    obj.Render(graphics, player.Camera);
                }
            }
        }
}
}
