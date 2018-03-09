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
    public class MapMarkerAcdPylon : MapMarkerAcd
    {
        public MapMarkerAcdPylon(ACD acd, Func<ACD, bool> isVisible)
            : base(acd, isVisible) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(8, Brushes.Orange, 2)
                .AnimateScale(0.5, 5, 0.5)
                .SpinRight(0.5)
                /*.BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests)*/;
        }

        public static bool IsInterested(ACD acd)
        {
            return acd.ActorType == ActorType.Gizmo &&
                acd.GizmoType == GizmoType.PowerUp;
        }

        public static bool IsStillInterested(ACD acd)
        {
            return MapMarkerOptions.Instance.ShowShrines &&
                Attributes.GizmoHasBeenOperated.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1;
            //Attributes.MinimapActive.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1; // This only indicates it should be shown, not when.
        }
    }
}
