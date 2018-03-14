using Enigma.D3.AttributeModel;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdGizmoLoreChest : MapMarkerAcd
    {
        public MapMarkerAcdGizmoLoreChest(ACD item)
                : base(item, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(8, Brushes.Purple, 2)
                .AnimateScale(0.5, 2, 0.5)
                .SpinRight(0.5)
                .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests);
        }

        public static bool IsInterested(ACD acd)
        {
            return Attributes.ChestOpen.GetValue(AttributeReader.Instance, acd.FastAttribGroupID, 0xA0000) != 1;
        }
    }
}
