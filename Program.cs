using Serializer.Editing;
using Serializer.Models;
using Serializer.Serialization;

namespace Serializer
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var list = new List<Figure>
            {
                new Circle(new Point(25, 50), 30),
                new Dot(15, 20),
                new Ellipse(new Point(120, 20), 40, 50),
                new Segment(new Point(20, 20), new Point(20, 50)),
                new Polygon(new List<Point>() { new Point(100, 25), new Point(75, 80), new Point(10, 110) }),
                new Rectangle(new Point(70, 120), 60, 30),
                new RegularPolygon(new Point(200, 120), 7, 15)
            };

            const string dirPath = @"F:\BsonFile";

            var editor = new ConsoleEditor(new PropertyHandler(), new BsonSerializer(), new FilesHandler(dirPath), list);
            editor.Start();
        }
    }
}