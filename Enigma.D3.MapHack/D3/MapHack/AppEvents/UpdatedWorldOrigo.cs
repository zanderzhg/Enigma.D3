using System.Windows.Media.Media3D;

namespace Enigma.D3.MapHack.AppEvents
{
    internal class UpdatedWorldOrigo
    {
        public UpdatedWorldOrigo(int world, Point3D origo)
        {
            World = world;
            Origo = origo;
        }

        public int World { get; }
        public Point3D Origo { get; }
    }
}