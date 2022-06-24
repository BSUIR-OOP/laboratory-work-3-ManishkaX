namespace Serializer.Models
{
    public class Polygon : Figure
    {
        public List<Point> Points { get; set; }


        public Polygon(List<Point> points) : base(points[0])
        {
            Points = new List<Point>();
            foreach (var item in points)
                Points.Add(item);
        }

        public Polygon() : base(new Point(0, 0))
        {
            
        }
    }
}
