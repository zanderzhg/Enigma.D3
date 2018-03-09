using Enigma.D3.AttributeModel;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Enigma.D3.MapHack.Markers
{
    public class InventoryMarker : MapMarkerAcd
    {
        private int _rank;
        private int _quality;

        private static Brush _legendaryBrush;
        private static Brush _setBrush;

        public InventoryMarker(ACD acd)
            : base(acd, x => true)
        {
            _rank = Attributes.AncientRank.GetValue(AttributeReader.Instance, acd.FastAttribGroupID);
            _quality = Attributes.ItemQualityLevel.GetValue(AttributeReader.Instance, acd.FastAttribGroupID);
            var attribs = AttributeReader.Instance.GetAttributes(acd.FastAttribGroupID);
            if (attribs.Any(x => x.Key.Id == AttributeId.SetItemCount && x.Value > 0))
                _quality = (int)ItemQuality.Set;
        }

        public override object CreateControl()
        {
            if (_legendaryBrush == null)
            {
                _legendaryBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkOrange, 0.5);
                _setBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.Green, 0.5);
            }

            var panel = new StackPanel();
            for (int i = 0; i < _rank; i++)
            {
                panel.Children.Add(ControlHelper.CreateCircle(10,
                    _quality == (int)ItemQuality.Legendary ? _legendaryBrush : _setBrush,
                    Brushes.Black, 1));
            }
            return panel;
        }

        public override void Update(int worldId, Point3D origo)
        {
            var isVisible = true;
            switch (Acd.ItemLocation)
            {
                case ItemLocation.PlayerBackpack:
                    X = 46 + Acd.ItemSlotX * 56;
                    Y = 628 + Acd.ItemSlotY * 56;
                    break;

                //case ItemLocation.PlayerHead:
                //    X = 390;
                //    Y = 190;
                //    break;
                //
                //case ItemLocation.PlayerTorso:
                //    X = 390;
                //    Y = 270;
                //    break;
                //
                //case ItemLocation.PlayerRightHand:
                //    break;
                //
                //case ItemLocation.PlayerLeftHand:
                //    break;
                //
                //case ItemLocation.PlayerHands:
                //    X = 282;
                //    Y = 316;
                //    break;
                //
                //case ItemLocation.PlayerWaist:
                //    X = 390;
                //    Y = 380;
                //    break;
                //
                //case ItemLocation.PlayerFeet:
                //    X = 390;
                //    Y = 512;
                //    break;
                //
                //case ItemLocation.PlayerShoulders:
                //    break;
                //
                //case ItemLocation.PlayerLegs:
                //    X = 390;
                //    Y = 420;
                //    break;
                //
                //case ItemLocation.PlayerBracers:
                //    X = 498;
                //    Y = 316;
                //    break;
                //
                //case ItemLocation.PlayerLeftFinger:
                //    break;
                //
                //case ItemLocation.PlayerRightFinger:
                //    break;
                //
                //case ItemLocation.PlayerNeck:
                //    break;

                default:
                    isVisible = false;
                    break;
            }
            IsVisible = isVisible;
        }
    }
}
