namespace StainedGlassFilter.Classes
{
    class PixelValues
    {
        public int XPos { get; set; }
        public int YPos { get; set; }
        public byte RedValue { get; set; }
        public byte GreenValue { get; set; }
        public byte BlueValue { get; set; }

        public PixelValues(int xPos = 0, int yPos = 0, byte red = 0, byte green = 0, byte blue = 0)
        {
            XPos = xPos;
            YPos = yPos;
            RedValue = red;
            GreenValue = green;
            BlueValue = blue;
        }
    }
}
