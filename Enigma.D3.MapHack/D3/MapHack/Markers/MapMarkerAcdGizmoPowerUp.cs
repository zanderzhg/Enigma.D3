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
    public class MapMarkerAcdGizmoPowerUp : MapMarkerAcd
    {
        public MapMarkerAcdGizmoPowerUp(ACD acd)
            : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(8, Brushes.Orange, 2)
                .AnimateScale(0.5, 5, 0.5)
                .SpinRight(0.5);
        }
        
        public static bool IsInterested(ACD acd)
        {
            return MapMarkerOptions.Instance.ShowShrines &&
                Attributes.GizmoState.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1 &&
                Attributes.GizmoDisabledByScript.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1 &&
                Attributes.GizmoHasBeenOperated.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1;
        }
    }
}
