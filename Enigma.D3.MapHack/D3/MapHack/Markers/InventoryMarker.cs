using Enigma.D3.AttributeModel;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel.Core;
using Enigma.Wpf;
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
            
            // If the item was created through cube, the ACD starts of as whatever item was the source.
            // If there is no ancient rank on a backpack item, try to update it. If value changed, re-create control.
            if (_rank == 0 && Acd.ItemLocation == ItemLocation.PlayerBackpack)
            {
                _rank = Attributes.AncientRank.GetValue(AttributeReader.Instance, Acd.FastAttribGroupID);
                if (_rank == 0)
                {
                    IsVisible = IsVisibleInInventory = IsVisibleInStash = false;
                    return;
                }
                else Execute.OnUIThread(() => Control = CreateControl());
            }

            IsVisibleInInventory = ItemLocation.PlayerBackpack <= Acd.ItemLocation && Acd.ItemLocation < ItemLocation.Stash;
            IsVisibleInStash = Acd.ItemLocation == ItemLocation.Stash && GetStashIndex() == Minimap.Instance.SelectedStashIndex;
            
            var isVisible = true;
            switch (Acd.ItemLocation)
            {
                case ItemLocation.PlayerBackpack:
                    X = 46 + Acd.ItemSlotX * 56;
                    Y = 628 + Acd.ItemSlotY * 55.5;
                    break;

                case ItemLocation.Stash:
                    X = 102 + Acd.ItemSlotX * 64;
                    Y = 252 + (Acd.ItemSlotY % 10) * 64;
                    break;

                case ItemLocation.PlayerHead:
                    X = 390;
                    Y = 194;
                    break;
                
                case ItemLocation.PlayerTorso:
                    X = 390;
                    Y = 270;
                    break;
                
                case ItemLocation.PlayerRightHand:
                    X = 500;
                    Y = 470;
                    break;
                
                case ItemLocation.PlayerLeftHand:
                    X = 282;
                    Y = 470;
                    break;
                
                case ItemLocation.PlayerHands:
                    X = 282;
                    Y = 316;
                    break;
                
                case ItemLocation.PlayerWaist:
                    X = 390;
                    Y = 384;
                    break;
                
                case ItemLocation.PlayerFeet:
                    X = 390;
                    Y = 512;
                    break;
                
                case ItemLocation.PlayerShoulders:
                    X = 308;
                    Y = 216;
                    break;
                
                case ItemLocation.PlayerLegs:
                    X = 390;
                    Y = 420;
                    break;
                
                case ItemLocation.PlayerBracers:
                    X = 498;
                    Y = 316;
                    break;
                
                case ItemLocation.PlayerLeftFinger:
                    X = 283;
                    Y = 416;
                    break;
                
                case ItemLocation.PlayerRightFinger:
                    X = 499;
                    Y = 416;
                    break;
                
                case ItemLocation.PlayerNeck:
                    X = 469;
                    Y = 236;
                    break;

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
