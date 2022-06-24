using System.Drawing;

namespace Serializer.Models
{
    public abstract class Figure
    {

        public double OutlineThickness { get; set; }

        public Point Point { get; set; }


        protected Figure(Point point)
        {
            Point = point;
            OutlineThickness = 1;
        }

        protected Figure()
        {
            
        }
    }
}
