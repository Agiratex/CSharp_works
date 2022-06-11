using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Lab2
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
        public System.Numerics.Vector2 vec { get; set;}

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
            return "x = " + x.ToString() + " y = " + y.ToString() + " " + vec.ToString();
        }

        public float Distance {get {return Vector2.Distance(new Vector2(Convert.ToSingle(x), Convert.ToSingle(y)),
                new Vector2(0,0));}}
    };

    abstract class V3Data : IEnumerable<DataItem>
    {
        public string id {get; protected set;}
        public DateTime time { get; protected set;}
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

        public abstract IEnumerator<DataItem> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    };

    class V3DataList : V3Data
    {
        public System.Collections.Generic.List<DataItem> list_of_data { get; private set;}

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
            return str + '\n';
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return list_of_data.GetEnumerator();
        }

        public bool SaveAsText(string fileName)
        {
            bool res = true;
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(id);
                    writer.WriteLine(time);
                    foreach (DataItem item in list_of_data)
                    {
                        writer.WriteLine(item.x);
                        writer.WriteLine(item.y);
                        writer.WriteLine(item.vec.X);
                        writer.WriteLine(item.vec.Y);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be wrote:");
                Console.WriteLine(e.Message);
            }
            finally
            {
                res = false;
            }
            return res;
        }

        public bool LoadAsText(string fileName)
        {
            bool res = true;
            try
            {
                using (StreamReader reader = new StreamReader(fileName, System.Text.Encoding.UTF8))
                {
                    id = reader.ReadLine();
                    time = Convert.ToDateTime(reader.ReadLine());
                    list_of_data = new List<DataItem>(8);
                    string str = "";
                    while((str = reader.ReadLine()) != null)
                    {
                        this.Add(new DataItem(Convert.ToDouble(str), Convert.ToDouble(reader.ReadLine()),
                                new Vector2(Convert.ToSingle(reader.ReadLine()), Convert.ToSingle(reader.ReadLine()))));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            finally
            {
                res = false;
            }
            return res;
        }
    };

    class V3DataArray : V3Data
    {
        public int height { get; private set;}
        public int width { get; private set;}
        public double x_step { get; private set;}
        public double y_step { get; private set;}
        public System.Numerics.Vector2[,] array { get; private set;}

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
            get
            {
                if (width > 0 & height > 0)
                {
                    return Math.Sqrt(Math.Pow((width - 1) * x_step, 2) + Math.Pow((height - 1) * y_step, 2));
                }
                else
                {
                    return 0;
                }
            }
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
                            + " " + array[i, j].ToString(format) + "\t";
                }
                str += '\n';
            }
            return str;
        }

        public static explicit operator V3DataList(V3DataArray obj)
        {
            V3DataList list = new V3DataList(obj.id, DateTime.Now);
            for (int i = 0; i < obj.height; i++)
            {
                for (int j = 0; j < obj.width; j++)
                {
                    list.Add(new DataItem(obj.x_step * j, obj.y_step * i, obj.array[i, j]));
                }
            }
            return list;
        }

        public override IEnumerator<DataItem> GetEnumerator()
        {
            return ((V3DataList)this).GetEnumerator();
        }

        public bool SaveBinary(string fileName)
        {
            bool res = true;
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    writer.Write(id);
                    writer.Write(time.ToBinary());
                    writer.Write(height);
                    writer.Write(width);
                    writer.Write(x_step);
                    writer.Write(y_step);
                    foreach (DataItem s in this)
                    {
                        writer.Write(s.vec.X);
                        writer.Write(s.vec.Y);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be wrote:");
                Console.WriteLine(e.Message);
            }
            finally
            {
                res = false;
            }
            return res;
        }

        public bool LoadBinary(string fileName)
        {
            bool res = true;
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    id = reader.ReadString();
                    time = DateTime.FromBinary(reader.ReadInt64());
                    height = reader.ReadInt32();
                    width = reader.ReadInt32();
                    x_step = reader.ReadDouble();
                    y_step = reader.ReadDouble();
                    array = new Vector2[height, width];
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            array[i, j] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            finally
            {
                res = false;
            }
            return res;
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

        public double AverageDistance
        {
            get
            {
                var data = (from item in list from it in item select it.Distance);
                if (data.Count() != 0)
                {
                    return data.Average();
                }
                else
                {
                    return double.NaN;
                }
            }
        }

        public IEnumerable<float> RangeDistance
        {
            get
            {
                var data = from item in list where (item.Count > 0) select (from it in item select it.Distance).Max()-
                        (from it in item select it.Distance).Min();
                if (data.Count() != 0)
                {
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }

        public IEnumerable<IGrouping<double, DataItem>> Group_x
        {
            get
            {
                var data = from item in list where (item.Count > 0) from it in item select it;
                if (data.Count() != 0)
                {
                    return from item in data
                            group item by item.x into g
                            select g;
                }
                else
                {
                    return null;
                }
            }
        }
    };

    class Program
    {
        static void Method1()
        {
            V3DataArray arr = new V3DataArray("arr", DateTime.Now, 3, 3, 1, 1, Methods.F1);
            arr.SaveBinary("for_arr.txt");
            V3DataArray arr_restored = new V3DataArray("", DateTime.Now);
            arr_restored.LoadBinary("for_arr.txt");
            Console.WriteLine(arr.ToLongString());
            Console.WriteLine(arr_restored.ToLongString());

            V3DataList list = new V3DataList("list", DateTime.Now);
            list.AddDefaults(5, Methods.F2);
            list.SaveAsText("for_list.txt");
            V3DataList list_restored = new V3DataList("", DateTime.Now);
            list_restored.LoadAsText("for_list.txt");
            Console.WriteLine(list.ToLongString());
            Console.WriteLine(list_restored.ToLongString());
        }

        static void Method2()
        {
            V3MainCollection collection = new V3MainCollection();
            collection.Add(new V3DataList("empty_list", DateTime.Now));
            V3DataList list = new V3DataList("full_list", DateTime.Now);
            list.AddDefaults(8, Methods.F2);
            collection.Add(list);
            collection.Add(new V3DataArray("empty_array", DateTime.Now, 0, 19, 1, 1, Methods.F3));
            collection.Add(new V3DataArray("full_array", DateTime.Now, 3, 3, 1, 1, Methods.F1));

            Console.WriteLine(collection.ToLongString());

            Console.WriteLine("Average distance:");
            Console.WriteLine($"{collection.AverageDistance}\n");

            Console.WriteLine("Max - Min distance:");
            var RD =collection.RangeDistance;
            if (RD != null)
            {
                foreach (float res in collection.RangeDistance)
                {
                    Console.WriteLine($"{res}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Empty collection");
            }


            Console.WriteLine("Group by X axis:");
            var GX = collection.Group_x;
            if (GX != null)
            {
                foreach (IGrouping<double, DataItem> group in GX)
                {
                    Console.WriteLine($"X: {group.Key}");
                    foreach (DataItem item in group)
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
            }
            else
            {
                Console.WriteLine("Empty collection");
            }

            V3MainCollection empty_collection = new V3MainCollection();
            empty_collection.Add(new V3DataList("empty_list", DateTime.Now));
            empty_collection.Add(new V3DataArray("full_array", DateTime.Now, 3, 3, 1, 1, Methods.F1));

            Console.WriteLine(empty_collection.ToLongString());

            Console.WriteLine("Average distance:");
            Console.WriteLine($"{collection.AverageDistance}\n");

            Console.WriteLine("Max - Min distance:");
            RD = empty_collection.RangeDistance;
            if (RD != null)
            {
                foreach (float res in RD)
                {
                    Console.WriteLine($"{res}");
                }
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Empty collection");
            }


            Console.WriteLine("Group by X axis:");
            GX = empty_collection.Group_x;
            if (GX != null)
            {
                foreach (IGrouping<double, DataItem> group in GX)
                {
                    Console.WriteLine($"X: {group.Key}");
                    foreach (DataItem item in group)
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
            }
            else
            {
                Console.WriteLine("Empty collection");
            }

        }

        static void Main()
        {
            Console.WriteLine("--------First method--------");
            Method1();
            Console.WriteLine("--------Second method-------");
            Method2();
        }
    }
}
