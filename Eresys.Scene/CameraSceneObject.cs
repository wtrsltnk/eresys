using Eresys.Math;

namespace Eresys
{
    /// <remarks>
    /// Schaal: 1m = 40u --> 1u = 2.5cm
    /// breedte 19" scherm = 36cm -> 14.4u
    /// </remarks>
    public class CameraSceneObject : SceneObject
    {
        public const float FOV = 1.5707963f; // = 90°
        public const float SCREENWIDTH = 32.0f;
        public const float DISTANCE = 80000.0f;
        public const float ASPECT = 1.3333333f;

        public Camera Camera { get; set; }

        public override bool Visible
        {
            get { return false; }
        }

        public float NearClippingPlane => Camera.NearClippingPlane;

        public float FarClippingPlane => Camera.FarClippingPlane;

        public Matrix ViewMatrix => Camera.ViewMatrix;

        public Matrix ProjectionMatrix => Camera.ProjectionMatrix;

        public CameraSceneObject()
        {
            Camera = new Camera();
        }

        public bool InsideFrustum(Point3D p, float r)
        {
            return Camera.InsideFrustum(p, r);
        }

        protected override void OnChange()
        {
            base.OnChange();

            Camera.UpdateCamera(Position, Direction);
        }
    }
}
