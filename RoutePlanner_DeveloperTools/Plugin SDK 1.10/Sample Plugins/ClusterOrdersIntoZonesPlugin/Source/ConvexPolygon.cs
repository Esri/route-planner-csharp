using System;


namespace ClusterOrdersIntoZonesPlugin
{
    class ConvexPolygon
    {
        public static Point3[] getConvexPolygon(Point3[] points)
        {
            
            int num = points.Length;
            sort(ref points,0,points.Length-1);
            
            Point3 left = points[0];
            Point3 right = points[num - 1];
            
            
           Circular lower = new Circular(left);
            Circular upper = new Circular(left);

            for (int i = 0; i < num; i++)
            {
                double result = Point3.check(left, right, points[i]);
                if (result > 0)
                    upper = upper.Append(new Circular(points[i]));
                else if (result < 0)
                    lower = lower.Prepend(new Circular(points[i]));
            }
            lower = lower.Prepend(new Circular(right));
            upper = upper.Append(new Circular(right)).Next;
            
            
            minimize(lower);
            minimize(upper);

            
            if (lower.Last.Point.Equals(upper.Point))
                lower.Last.Remove();

            if (upper.Last.Point.Equals(lower.Point))
                upper.Last.Remove();
        
            
            Point3[] final = new Point3[lower.Length() + upper.Length() + 1];

            lower.Duplicate(ref final, 0);
            upper.Duplicate(ref final, lower.Length());
            final[lower.Length() + upper.Length()] = final[0];
            increase1pc(ref final);

            return final;
        }
        
        private static void sort(ref Point3[] Points, int start, int end)
        {
            if (start < end)
            {
                int i = start;
                int j = end;
                Point3 x = Points[(i + j) / 2];

                do
                {
                    while (Points[i].Smaller(x)) 
                        i++;

                    while (x.Smaller(Points[j])) 
                        j--;
                    
                    if (i <= j)
                    {
                        Point3 temp = Points[i];
                        Points[i] = Points[j];
                        Points[j] = temp;
                        
                        i++; j--;
                    }
                } while (i <= j);

                sort(ref Points, start, j);
                sort(ref Points, i, end);
            }
        }

        private static void minimize(Circular start)
        {
            Circular a = start;
            Circular end = start.Last;

            bool finished = false;

            while (a.Next != start || !finished)
            {
                if (a.Next == end)
                    finished = true;
                if (Point3.check(a.Point, a.Next.Point, a.Next.Next.Point) < 0) 
                    a = a.Next;
                else
                {                                       
                    a.Next.Remove();
                    a = a.Last;
                }
            }
        }

        private static void increase1pc(ref Point3[] points)
        {
            Point3 cent = new Point3(0, 0);
            for (int i = 0; i < points.Length; i++)
            {
                cent.x += points[i].x;
                cent.y += points[i].y;
            }

            cent.x = cent.x / points.Length;
            cent.y = cent.y / points.Length;

            for (int i = 0; i < points.Length; i++)
            {
                points[i].x += (points[i].x - cent.x) * .01;
                points[i].y += (points[i].y - cent.y) * .01;
            }

        }


    }

    class Point3 
    {
        public double x;
        public double y;

        public Point3(double X, double Y)
        {
            x = X; 
            y = Y;
        }

        public bool Equals(Point3 point)
        {
            if (x == point.x && y == point.y)
                return true;
            else
                return false;
        }

        public bool Smaller(Point3 point)
        {
            if ((x < point.x) || (x == point.x && y < point.y))
                return true;
            else
                return false;
        }

        public static double check(Point3 point1, Point3 point2, Point3 point3)
        {
            return point1.x * (point2.y - point3.y) + point2.x * (point3.y - point1.y) + point3.x * (point1.y - point2.y);
        }
    }

    class Circular
    {
        public Circular Last;
        public Circular Next;     
        
        public Point3 Point;

        public Circular(Point3 point)
        {
            Point = point;
            Next = this;
            Last = this;
        }

        public void Remove()
        {
            Next.Last = Last; 
            Last.Next = Next;
            Next = null;
            Last = null;
        }

        public Circular Prepend(Circular node)
        {
            node.Next = this;
            node.Last = Last;
            Last.Next = node;
            Last = node;

            return node;
        }

        public Circular Append(Circular node)
        {
            node.Last = this;
            node.Next = Next;
            Next.Last = node;
            Next = node;

            return node;
        }

        public int Length()
        {
            int num = 0;
            Circular node = this;
            do
            {
                num++;
                node = node.Next;
            } while (node != this);

            return num;
        }

        public void Duplicate(ref Point3[] Points, int i)
        {
            Circular node = this;
            do
            {
                Points[i++] = node.Point;
                node = node.Next;
            } while (node != this);
        }
    }
}
