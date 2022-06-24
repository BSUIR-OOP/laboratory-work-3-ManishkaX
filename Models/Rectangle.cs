namespace Serializer.Models
{
    public class Rectangle : Figure
    {
        public double Width { get; set; }

        public double Height { get; set; }


        public Rectangle(Point center, double width, double height) : base(center)
        {
            Width = width;
            Height = height;
        }

        public Rectangle() : base(new Point(0, 0))
        {
            
        }
    }
}
