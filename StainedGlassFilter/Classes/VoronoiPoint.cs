using System;
using System.Collections.Generic;

namespace StainedGlassFilter.Classes
{
    class VoronoiPoint
    {
        public int XOffset { get; set; }
        public int YOffset { get; set; }
        public byte RedTotal { get; set; }
        public byte GreenTotal { get; set; }
        public byte BlueTotal { get; set; }
        public List<PixelValues> PixelCollection { get; private set; }
        public byte RedAvarage { get; private set; }
        public byte GreenAvarage { get; private set; }
        public byte BlueAvarage { get; private set; }

        public VoronoiPoint(int xOffset = 0, int yOffset = 0)
        {
            XOffset = xOffset;
            YOffset = yOffset;
            PixelCollection = new List<PixelValues>();
        }

        public void CalculateAvarages()
        {
            if(PixelCollection.Count > 0)
            {
                int temp = RedTotal / PixelCollection.Count;
                RedAvarage = Convert.ToByte(temp);
                temp = GreenTotal / PixelCollection.Count;
                GreenAvarage = Convert.ToByte(temp);
                temp = BlueTotal / PixelCollection.Count;
                BlueAvarage = Convert.ToByte(temp);
            }
        }

        public void AddPixel(PixelValues pixel)
        {
            RedTotal += pixel.RedValue;
            GreenTotal += pixel.GreenValue;
            BlueTotal += pixel.BlueValue;

            PixelCollection.Add(pixel);
        }
    }
}
