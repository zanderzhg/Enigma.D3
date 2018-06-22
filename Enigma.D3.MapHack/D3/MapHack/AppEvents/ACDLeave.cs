using Enigma.D3.MemoryModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack.AppEvents
{
    internal class ACDLeave
    {
        public ACDLeave(ACD acd)
        {
            ACD = acd ?? throw new ArgumentNullException(nameof(acd));
        }

        public ACD ACD { get; set; }
    }
}
