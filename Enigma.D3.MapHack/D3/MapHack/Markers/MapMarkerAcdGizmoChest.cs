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
            if (Slug.IndexOf("_Chest_Rare", StringComparison.OrdinalIgnoreCase) != -1)
            {
                var grid = new System.Windows.Controls.Grid();
                grid.Children.Add(ControlHelper.CreateCross(9, Brushes.Orange, 3));
                grid.Children.Add(ControlHelper.CreateCross(8, Brushes.DarkGreen, 2));
                return grid
                    .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests)
                    .SpinRight(0.5)
                    .AnimateScale(0.5, 2, 0.5);
            }
            else if (Slug.IndexOf("_Chest_StartsClean", StringComparison.OrdinalIgnoreCase) != -1)
            {
                var grid = new System.Windows.Controls.Grid();
                grid.Children.Add(ControlHelper.CreateCross(9, Brushes.LightBlue, 3));
                grid.Children.Add(ControlHelper.CreateCross(8, Brushes.DarkGreen, 2));
                return grid
                    .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests)
                    .SpinRight(0.5)
                    .AnimateScale(0.5, 2, 0.5);
            }
            else if (Slug.IndexOf("_Chest", StringComparison.OrdinalIgnoreCase) != -1)
            {
                var grid = new System.Windows.Controls.Grid();
                grid.Children.Add(ControlHelper.CreateCross(9, Brushes.White, 3));
                grid.Children.Add(ControlHelper.CreateCross(8, Brushes.DarkGreen, 2));
                return grid
                    .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowChests)
                    .SpinRight(0.5)
                    .AnimateScale(0.5, 2, 0.5);
            }
            return ControlHelper.CreateCross(8, Brushes.DarkGreen, 2)
                .AnimateScale(0.5, 2, 0.5)
                .SpinRight(0.5)
                .BindVisibilityTo(MapMarkerOptions.Instance, a => a.ShowHiddenChests);
        }

        public static bool IsInterested(ACD acd)
        {
            return (acd.CollisionFlags & 0x400) == 0 &&
                Attributes.ChestOpen.GetValue(AttributeReader.Instance, acd.FastAttribGroupID) != 1;
        }
    }
}
