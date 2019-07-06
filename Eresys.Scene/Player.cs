namespace Eresys
{
    /// <summary>
    /// Summary description for Player.
    /// </summary>
    public class Player : SceneObject
    {
        public override bool Visible
        {
            get { return false; }
        }

        public override bool Solid
        {
            get { return true; }
        }

        public CameraSceneObject Camera { get; private set; }

        public Player()
        {
            Camera = new CameraSceneObject();
        }

        protected override void OnChange()
        {
            Camera.Position = Position;
            Camera.Direction = Direction;
        }
    }
}
