using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.Memory
{
    public static class MemoryObjectExtensions
    {
        public static Ptr<T> GetPointer<T>(this T self) where T : MemoryObject
        {
            return new Ptr<T>(self.Memory, self.Address);
        }

        public static TCast Cast<TCast>(this MemoryObject self)
            where TCast : MemoryObject
        {
            return MemoryObjectFactory.Create<TCast>(self.Memory, self.Address);
        }

        public static T Snapshot<T>(this T self) where T : MemoryObject
            => Snapshot(self, SnapshotBehavior.ReplaceExistingSnapshot);

        public static T Snapshot<T>(this T self, SnapshotBehavior behavior) where T : MemoryObject
        {
            var clone = self.Memory.Reader.Read<T>(self.Address);
            clone.TakeSnapshot(behavior);
            return clone;
        }
    }
}
