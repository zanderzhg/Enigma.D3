using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.DataTypes
{
	public struct GBID
	{
		public static implicit operator uint(GBID gbid) => gbid.Value;

		public static implicit operator GBID(uint value) => new GBID { Value = value };

		public uint Value;

        public override int GetHashCode() => (int)HashUtils.Fnv32((int)Value);

        public override string ToString()
			=> Value == unchecked((uint)-1) ? "NONE" : Value.ToString("X8");
	}
}
