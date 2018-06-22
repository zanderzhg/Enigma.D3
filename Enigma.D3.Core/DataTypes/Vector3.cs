using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Enigma.D3.DataTypes
{
    public struct Vector3
    {
        public static implicit operator Point3D(Vector3 vec) => new Point3D(vec.X, vec.Y, vec.Z);
        public static implicit operator Vector3D(Vector3 vec) => new Vector3D(vec.X, vec.Y, vec.Z);

        public float X;
        public float Y;
        public float Z;

        public override string ToString()
            => $"vec3({X}, {Y}, {Z})";
    }
}
