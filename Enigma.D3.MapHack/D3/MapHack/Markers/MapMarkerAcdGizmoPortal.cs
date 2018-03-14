using Enigma.D3.AttributeModel;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdGizmoPortal : MapMarkerAcd
    {
        public MapMarkerAcdGizmoPortal(ACD acd)
            : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCircle(20, null, Brushes.Yellow, 4)
                .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowPortals);
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.TeamID != 0;
        }
    }
}
