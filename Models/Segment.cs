namespace Serializer.Models
{
    public class Segment : Figure
    {
        public Point A { get; set; }

        public Point B { get; set; }


        public Segment(Point a, Point b): base(a)
        {
            A = a;
            B = b;
        }

        public Segment() : base(new Point(0, 0))
        {
            
        }
    }
}
