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
    public class MapMarkerAcdPortal : MapMarkerAcd
    {
        public MapMarkerAcdPortal(ACD acd, Func<ACD, bool> isVisible)
            : base(acd, isVisible) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCircle(20, null, Brushes.Yellow, 4);
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.GizmoType.ToString().Contains("Portal") &&
                 acd.GizmoType != GizmoType.TownPortal &&
                 acd.GizmoType != GizmoType.PortalDestination &&
                 acd.GizmoType != GizmoType.HearthPortal &&
                 acd.TeamID != 0;
        }

        public static bool IsStillInterested(ACD acd)
        {
            return MapMarkerOptions.Instance.ShowPortals;
        }
    }
}
