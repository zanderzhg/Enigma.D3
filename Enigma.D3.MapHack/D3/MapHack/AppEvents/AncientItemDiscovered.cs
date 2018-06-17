using Enigma.D3.ApplicationModel;
using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack.AppEvents
{
    public class AncientItemDiscovered
    {
        public AncientItemDiscovered(ACD acd, string slug, string name)
        {
            ACD = acd ?? throw new ArgumentNullException(nameof(acd));
            Slug = slug;
            Name = name;
        }

        public ACD ACD { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
    }
}
