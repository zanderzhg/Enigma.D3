using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerTrickle : MapMarkerBase
    {
        private readonly Trickle _trickle;

        public MapMarkerTrickle(Trickle trickle)
            : base(trickle.x00_Id)
        {
            _trickle = trickle;
        }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(20, Brushes.Red, 3);
        }

        public override void Update(int worldId, Point3D origo)
        {
            if (IsVisible = _trickle.x14_WorldId == worldId)
            {
                X = _trickle.x08_WorldPosX - origo.X;
                Y = _trickle.x0C_WorldPosY - origo.Y;
            }
        }
    }
}
