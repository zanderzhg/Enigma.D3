using Enigma.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MapHack
{
    internal class SkillBarSlot : NotifyingObject
    {
        private string _text;

        public string Text
        {
            get => _text;
            set { _text = value; Refresh(nameof(Text)); }
        }

        public int Index { get; set; }
    }
}
