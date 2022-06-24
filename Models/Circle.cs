namespace Serializer.Models
{
    public class Circle : Figure
    {
        public double Radius { get; set; }


        public Circle(Point center, double radius) : base(center) =>
            Radius = radius;

        public Circle(): base(new Point(0, 0))
        {
            
        }
    }
}
