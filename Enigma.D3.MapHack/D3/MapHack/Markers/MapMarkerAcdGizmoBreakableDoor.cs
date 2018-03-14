using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdGizmoBreakableDoor: MapMarkerAcd
    {
        public MapMarkerAcdGizmoBreakableDoor(ACD acd)
               : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(6, Brushes.Red, 1)
                .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowWreckables);
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.Hitpoints == 0.001f;
        }
    }
}
