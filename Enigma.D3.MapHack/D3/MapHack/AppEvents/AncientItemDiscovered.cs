using Enigma.D3.ApplicationModel;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack.AppEvents
{
    internal class AncientItemDiscovered
    {
        public AncientItemDiscovered(ACD acd, string slug, string name)
        {
            ACD = acd ?? throw new ArgumentNullException(nameof(acd));
            Slug = slug;
            Name = name;
        }

        public ACD ACD { get; }
        public string Slug { get; }
        public string Name { get; }
    }
}
