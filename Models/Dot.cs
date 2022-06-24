namespace Serializer.Models
{
    public class Dot : Figure
    {
        public double X { get; set; }

        public double Y { get; set; }


        public Dot(double x, double y) : base(new Point(x, y))
        {
            X = x;
            Y = y;
        }
        
        public Dot(): base(new Point(0, 0))
        {
            
        }
    }
}
