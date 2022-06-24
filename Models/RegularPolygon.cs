namespace Serializer.Models
{
    public class RegularPolygon : Figure
    {
        public int SidesCount { get; set; }

        public double SideLength { get; set; }


        public RegularPolygon(Point center, int sidesCount, double sideLength) : base(center)
        {
            SidesCount = sidesCount;
            SideLength = sideLength;
        }

        public RegularPolygon() : base(new Point(0, 0))
        {
            
        }
    }
}
