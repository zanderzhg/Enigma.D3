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
        private bool _isVisibleInInventory;
        private bool _isVisibleInStash;
        private int _rank;
        private int _quality;

        private static Brush _legendaryBrush;
        private static Brush _setBrush;
        private static Brush _primalBrush;

        public InventoryMarker(ACD acd)
            : base(acd, x => true)
        {
            _rank = Attributes.AncientRank.GetValue(AttributeReader.Instance, acd.FastAttribGroupID);
            _quality = Attributes.ItemQualityLevel.GetValue(AttributeReader.Instance, acd.FastAttribGroupID);
            var attribs = AttributeReader.Instance.GetAttributes(acd.FastAttribGroupID);
            if (attribs.Any(x => x.Key.Id == AttributeId.SetItemCount && x.Value > 0))
                _quality = (int)ItemQuality.Set;
        }

        public bool IsVisibleInInventory
        {
            get { return _isVisibleInInventory; }
            set
            {
                if (_isVisibleInInventory != value)
                {
                    _isVisibleInInventory = value;
                    Refresh(nameof(IsVisibleInInventory));
                }
            }
        }

        public bool IsVisibleInStash
        {
            get { return _isVisibleInStash; }
            set
            {
                if (_isVisibleInStash != value)
                {
                    _isVisibleInStash = value;
                    Refresh(nameof(IsVisibleInStash));
                }
            }
        }

        public override object CreateControl()
        {
            if (_legendaryBrush == null)
            {
                _legendaryBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.DarkOrange, 0.5);
                _setBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.Green, 0.5);
                _primalBrush = ControlHelper.CreateAnimatedBrush(Colors.Black, Colors.Red, 0.5);
            }
            
            var brush = _rank >= 2 ? _primalBrush : (_quality == (int)ItemQuality.Legendary ? _legendaryBrush : _setBrush);
            return ControlHelper.CreateCircle(10, brush, Brushes.Black, 1);
        }

        public override void Update(int worldId, Point3D origo)
        {
            if (MapMarkerOptions.Instance.ShowAncientRank == false)
            {
                IsVisible = false;
                IsVisibleInInventory = false;
                IsVisibleInStash = false;
                return;
            }

            IsVisibleInInventory = Acd.ItemLocation == ItemLocation.PlayerBackpack;// <= Acd.ItemLocation && Acd.ItemLocation < ItemLocation.Stash;
            IsVisibleInStash = Acd.ItemLocation == ItemLocation.Stash && GetStashIndex() == Minimap.Instance.SelectedStashIndex;
            
            var isVisible = true;
            switch (Acd.ItemLocation)
            {
                case ItemLocation.PlayerBackpack:
                    X = 46 + Acd.ItemSlotX * 56;
                    Y = 628 + Acd.ItemSlotY * 56;
                    break;

                case ItemLocation.Stash:
                    X = 102 + Acd.ItemSlotX * 64;
                    Y = 252 + (Acd.ItemSlotY % 10) * 64;
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

        private int GetStashIndex()
            => Acd.ItemSlotY / 10;
    }
}
