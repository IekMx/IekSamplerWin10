namespace IekOpcSamplerApp.Models
{
    class Point
    {
        public Point()
        {
        }

        public Point(int x, double y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public double Y { get; set; }
    }
}
