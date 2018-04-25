using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdServerProp : MapMarkerAcd
    {
        public MapMarkerAcdServerProp(ACD acd)
               : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            return ControlHelper.CreateCross(8, Brushes.Gray, 2)
                .AnimateScale(0.5, 5, 0.5)
                .SpinRight(0.5)
                .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowPylonSpawnPoints);
        }

        public static bool IsInterested(ACD acd) => acd.ActorSNO == 0x68A92;
    }
}
