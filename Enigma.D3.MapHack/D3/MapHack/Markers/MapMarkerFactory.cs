using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using Enigma.D3.Enums;
using Enigma.D3.MemoryModel.Core;
using Enigma.D3.AttributeModel;

namespace Enigma.D3.MapHack.Markers
{
    public static class MapMarkerFactory
    {
        public static IMapMarker Create(ACD acd)
        {
            switch (acd.ActorType)
            {
                case ActorType.Invalid:
                    break;
                case ActorType.Monster:
                    return new MapMarkerAcdMonster(acd);
                case ActorType.Gizmo:
                    switch (acd.GizmoType)
                    {
                        case GizmoType.Invalid:
                            break;
                        case GizmoType.Door:
                            break;
                        case GizmoType.Chest:
                            return new MapMarkerAcdGizmoChest(acd);
                        case GizmoType.Portal:
                            return new MapMarkerAcdGizmoPortal(acd);
                        case GizmoType.Waypoint:
                            break;
                        case GizmoType.Item:
                            break;
                        case GizmoType.Checkpoint:
                            break;
                        case GizmoType.Sign:
                            break;
                        case GizmoType.HealingWell:
                            break;
                        case GizmoType.PowerUp:
                            return new MapMarkerAcdGizmoPowerUp(acd);
                        case GizmoType.TownPortal:
                            break;
                        case GizmoType.HearthPortal:
                            break;
                        case GizmoType.Headstone:
                            break;
                        case GizmoType.PortalDestination:
                            break;
                        case GizmoType.BreakableChest:
                            return new MapMarkerAcdGizmoBreakableChest(acd);
                        case GizmoType.SharedStash:
                            break;
                        case GizmoType.Spawner:
                            break;
                        case GizmoType.PageOfFatePortal:
                            break;
                        case GizmoType.Trigger:
                            break;
                        case GizmoType.SecretPortal:
                            break;
                        case GizmoType.DestroyableObject:
                            return new MapMarkerAcdGizmoDestroyableObject(acd);
                        case GizmoType.BreakableDoor:
                            return new MapMarkerAcdGizmoBreakableDoor(acd);
                        case GizmoType.Switch:
                            return new MapMarkerAcdGizmoSwitch(acd);
                        case GizmoType.PressurePlate:
                            break;
                        case GizmoType.Gate:
                            break;
                        case GizmoType.DestroySelfWhenNear:
                            break;
                        case GizmoType.ActTransitionObject:
                            break;
                        case GizmoType.ReformingDestroyableObject:
                            break;
                        case GizmoType.Banner:
                            break;
                        case GizmoType.LoreChest:
                            return new MapMarkerAcdGizmoLoreChest(acd);
                        case GizmoType.BossPortal:
                            break;
                        case GizmoType.PlacedLoot:
                            break;
                        case GizmoType.SavePoint:
                            break;
                        case GizmoType.ReturnPointPortal:
                            break;
                        case GizmoType.DungeonPortal:
                            break;
                        case GizmoType.IdentifyAll:
                            break;
                        case GizmoType.ReturnPortal:
                            break;
                        case GizmoType.RecreateGameWithParty:
                            break;
                        case GizmoType.Mailbox:
                            break;
                        case GizmoType.LootRunSwitch:
                            break;
                        case GizmoType.PoolOfReflection:
                            return new MapMarkerAcdGizmoPoolOfReflection(acd);
                    }
                    break;
                case ActorType.ClientEffect:
                    break;
                case ActorType.ServerProp:
                    return new MapMarkerAcdServerProp(acd);
                case ActorType.Environment:
                    break;
                case ActorType.Critter:
                    break;
                case ActorType.Player:
                    break;
                case ActorType.Item:
                    return new MapMarkerAcdItem(acd);
                case ActorType.AxeSymbol:
                    break;
                case ActorType.Projectile:
                    break;
                case ActorType.CustomBrain:
                    break;
            }
            return null;
        }
    }
}
