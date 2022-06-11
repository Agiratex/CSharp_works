using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1
{
    public static class Globals
    {
        public static Random rand = new Random(1);
    }

    public static class Methods
    {
        public static Vector2 F1(double x, double y)
        {
            return new Vector2(Convert.ToSingle(x + y), Convert.ToSingle(x - y));
        }

        public static Vector2 F2(double x, double y)
        {
            return new Vector2(Convert.ToSingle(x), Convert.ToSingle(y));
        }

        public static Vector2 F3(double x, double y)
        {
            return new Vector2(Convert.ToSingle(Math.Pow(x, 2)), Convert.ToSingle(Math.Pow(y, 3)));
        }
    }

    public delegate System.Numerics.Vector2 FdblVector2(double x, double y);

    public struct DataItem
    {

        public double x { get; set;}
        public double y { get; set;}
        System.Numerics.Vector2 vec { get; set;}

        public DataItem(double x, double y, System.Numerics.Vector2 vec)
        {
            this.x = x;
            this.y = y;
            this.vec = vec;
        }

        public string ToLongString(string format = "")
        {
            return x.ToString(format) + " " + y.ToString(format) + " " + vec.ToString(format) + " " +
                Vector2.Distance(vec, new Vector2(0,0)).ToString(format);
        }

        public override string ToString()
        {
            return x.ToString() + " " + y.ToString() + " " + vec.ToString();
        }
    };

    abstract class V3Data
    {
        public string id { get; }
        public DateTime time { get; }
        public abstract int Count{ get; }
        public abstract double MaxDistance{ get; }

        public V3Data(string Id, DateTime Time)
        {
            id = Id;
            time = Time;
        }

        public abstract string ToLongString(string fomat = "");
        public override string ToString()
        {
            return id + " " + time.ToString();
        }
    };

    class V3DataList : V3Data
    {
        public System.Collections.Generic.List<DataItem> list_of_data { get; }

        public V3DataList(string Id, DateTime Time) : base(Id, Time )
        {
            list_of_data = new List<DataItem>(8);
        }

        public bool Add(DataItem newitem)
        {
            for (int i = 0; i < list_of_data.Count; i++)
            {
                if ((list_of_data[i].x == newitem.x) && (list_of_data[i].y == newitem.y)){
                    return false;
                }
            }
            list_of_data.Add(newitem);
            return true;
        }

        public int AddDefaults(int nitems, FdblVector2 F)
        {
            int counter = 0;
            for (int i = 0; i < nitems; i++)
            {
                double x = Globals.rand.NextDouble() * 100;
                double y = Globals.rand.NextDouble() * 100;
                DataItem newitem = new DataItem(x, y, F(x, y));
                if (Add(newitem))
                {
                    counter++;
                }
            }
            return counter;
        }

        public override int Count
        {
            get { return list_of_data.Count; }
        }

        public override double MaxDistance
        {
            get
            {
                double maxdistance = 0.0;
                for (int i = 0; i < list_of_data.Count; i++)
                {
                    for (int j = i + 1; j < list_of_data.Count; j++)
                    {
                        double distance = Math.Sqrt(Math.Pow(list_of_data[i].x - list_of_data[j].x, 2) +
                                Math.Pow(list_of_data[i].y - list_of_data[j].y, 2));
                        if (distance > maxdistance)
                        {
                            maxdistance = distance;
                        }
                    }
                }
            return maxdistance;
            }
        }

        public override string ToString()
        {
            return "V3DataList " +  base.ToString() + " " + list_of_data.Count.ToString();
        }

        public override string ToLongString(string format = "")
        {
            string str = ToString();
            for (int i = 0; i < list_of_data.Count; i++)
            {
                str += " " + list_of_data[i].ToString();
            }
            return str;
        }
    };

    class V3DataArray : V3Data
    {
        public int height { get; }
        public int width { get; }
        public double x_step { get; }
        public double y_step { get; }
        public System.Numerics.Vector2[,] array { get; }

        public V3DataArray(string Id, DateTime Time) : base(Id, Time)
        {
            height = 0;
            width = 0;
            x_step = 0.0;
            y_step = 0.0;
            array = new System.Numerics.Vector2 [0, 0];
        }

        public V3DataArray(string Id, DateTime Time, int Height, int Width, double X_step,
                double Y_step, FdblVector2 F) : base(Id, Time)
        {
            height = Height;
            width = Width;
            x_step = X_step;
            y_step = Y_step;
            array = new System.Numerics.Vector2[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    array[i,j] = F(j * x_step, i * y_step);
                }
            }
        }

        public override int Count
        {
            get{ return width * height; }
        }

        public override double MaxDistance
        {
            get { return Math.Sqrt(Math.Pow((width - 1) * x_step, 2) + Math.Pow((height - 1) * y_step, 2));}
        }

        public override string ToString()
        {
            return "V3DataArray " + base.ToString() + " " +
                    $"x_step = {x_step} y_step = {y_step} width = {width} height = {height}";
        }

        public override string ToLongString(string format = "")
        {
            string str = ToString() + '\n';
            for (int i = height - 1; i >= 0; i--)
            {
                for (int j = 0; j < width; j++)
                {
                    str += "x = " + (x_step * j).ToString(format) + " y = " + (y_step * i).ToString(format)
                            + " " + array[i, j].ToString(format) + " ";
                }
                str += '\n';
            }
            return str;
        }

        public static explicit operator V3DataList(V3DataArray obj)
        {
            V3DataList list = new V3DataList(obj.id + ".casted", DateTime.Now);
            for (int i = 0; i < obj.height; i++)
            {
                for (int j = 0; j < obj.width; j++)
                {
                    list.Add(new DataItem(obj.x_step * j, obj.y_step * i, obj.array[i, j]));
                }
            }
            return list;
        }
    };

    class V3MainCollection
    {

        private System.Collections.Generic.List<V3Data> list;
        public int Count { get{ return list.Count;}}

        public V3MainCollection()
        {
            list = new List<V3Data>(4);
        }

        public V3Data this [int id]
        {
            get
            {
                return list[id];
            }
        }

        public bool Contains(string id)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Add(V3Data v3Data)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == v3Data.id)
                {
                    return false;
                }
            }
            list.Add(v3Data);
            return true;
        }

        public string ToLongString(string format = "")
        {
            string str = "";
            for (int i = 0; i < list.Count; i++)
            {
                str += list[i].ToLongString(format) + '\n';
            }
            return str;
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < list.Count; i++)
            {
                str += list[i].ToString() + '\n';
            }
            return str;
        }
    };

    class Program
    {
        static void Main()
        {
            V3DataArray arr = new V3DataArray("arr1", DateTime.Now, 3, 3, 1, 1, Methods.F1);
            Console.WriteLine(arr.ToLongString());
            V3DataList list = (V3DataList)arr;
            Console.WriteLine(list.ToLongString() + '\n');
            Console.WriteLine($"arr1.Count = {arr.Count}; arr.MaxDistance = {arr.MaxDistance}; " +
                    $"list.Count = {list.Count}; list.MaxDistance = {list.MaxDistance};" + '\n');

            V3MainCollection collection = new V3MainCollection();
            collection.Add(arr);
            collection.Add(list);
            V3DataList list2 = new V3DataList("list2",DateTime.Now);
            list2.AddDefaults(4, Methods.F2);
            V3DataArray arr2 = new V3DataArray("arr2", DateTime.Now, 3, 3, 1, 1, Methods.F3);
            collection.Add(arr2);
            collection.Add(list2);
            Console.WriteLine(collection.ToLongString());


            for (int i = 0; i < collection.Count; i++)
            {
                Console.WriteLine(collection[i].id + $" {collection[i].Count} {collection[i].MaxDistance}");
            }
        }
    }
}
