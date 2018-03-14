using Enigma.D3.AttributeModel;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdGizmoChest : MapMarkerAcd
    {
        public MapMarkerAcdGizmoChest(ACD item)
            : base(item, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(8, Brushes.DarkGreen, 2)
                .AnimateScale(0.5, 2, 0.5)
                .SpinRight(0.5)
                .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests);
        }

        public static bool IsInterested(ACD acd)
        {
            return (acd.CollisionFlags & 0x400) == 0 &&
                Attributes.ChestOpen.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1;
        }
    }
}
