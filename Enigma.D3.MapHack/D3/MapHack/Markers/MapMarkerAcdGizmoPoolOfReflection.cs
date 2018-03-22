using Enigma.D3.AttributeModel;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Enigma.D3.MapHack.Markers
{
    public class MapMarkerAcdGizmoPoolOfReflection : MapMarkerAcd
    {
        public MapMarkerAcdGizmoPoolOfReflection(ACD acd)
            : base(acd, IsInterested) { }

        public override object CreateControl()
        {
            return new System.Windows.Controls.TextBlock
            {
                Text = "XP",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Yellow,
                Background = Brushes.Black,
                Padding = new Thickness(2),
                Opacity = 0.6
            }
            .RevertMinimapTransform()
            .BindVisibilityTo(MapMarkerOptions.Instance, x => x.ShowPoolsOfReflection);
        }

        public static bool IsInterested(ACD acd)
        {
            return Attributes.GizmoCharges.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 0 &&
                Attributes.GizmoHasBeenOperated.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1;
        }
    }
}
