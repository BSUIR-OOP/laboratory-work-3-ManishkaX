namespace Serializer.Models
{
    public class Ellipse : Figure
    {
        public double AxisX { get; set; }

        public double AxisY { get; set; }


        public Ellipse(Point center, double axisX, double axisY) : base(center)
        {
            AxisX = axisX;
            AxisY = axisY;
        }
        
        public Ellipse(): base(new Point(0, 0))
        {
            
        }
    }
}
