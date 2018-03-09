using Enigma.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enigma.D3.MemoryModel.Core
{
    public class SceneRevealInfo : MemoryObject
    {
        public static int SizeOf => SymbolTable.PlatformSize(0x3C, 0x44);

        public int x00_SceneSnoId { get { return Read<int>(0x00); } }
        public int x04_SceneId_ { get { return Read<int>(0x04); } }
        public int x08_WorldId_ { get { return Read<int>(0x08); } }
        public int x0C_TextureSnoId { get { return Read<int>(0x0C); } }
        public float x10_MinX { get { return Read<float>(0x10); } }
        public float x14_MinY { get { return Read<float>(0x14); } }
        public float x18_MaxX { get { return Read<float>(0x18); } }
        public float x1C_MaxY { get { return Read<float>(0x1C); } }
        public int x20_TextureSnoId_ { get { return Read<int>(0x20); } }
        public byte[] OpacityMask { get { return this.PlatformRead<Ptr<byte>>(0x24, 0x28).ToArray(OpacityMaskWidth * OpacityMaskHeight); } } // 8 bits-per-channel grayscale channel
        public int OpacityMaskWidth { get { return this.PlatformRead<int>(0x28, 0x30); } }
        public int OpacityMaskHeight { get { return this.PlatformRead<int>(0x2C, 0x34); } }
        public int FrameCounter { get { return this.PlatformRead<int>(0x30, 0x38); } }
        public int IsFullyVisible { get { return this.PlatformRead<int>(0x34, 0x3C); } }
        public int _Unknown { get { return this.PlatformRead<int>(0x38, 0x40); } }

        public byte[] GetPixelBuffer(ref byte[] buffer, out int width, out int height)
        {
            width = OpacityMaskWidth;
            height = OpacityMaskHeight;
            int size = width * height;
            if (buffer.Length != size)
                Array.Resize(ref buffer, size);

            if (IsFullyVisible == 1)
            {
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = 0xFF;
            }
            else
            {
                Memory.Reader.ReadBytes(Read<int>(0x24), buffer);
            }
            return buffer;
        }
    }
}
