using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MKL_Test
{
    class Program
    {
        public struct VMTime
        {
            public uint length;
            public double []ratio; // 0: single EP  1: single HA  2: double EP  3: double HA
        }

        public struct VMAccuracy
        {
            public (double, double) segment;
            public uint length;
            public (double, double) max_ratio;  // 0: single  1: double
            public ((double, double), (double, double)) arguments; // ((singel_x, single_y), (double_x, double_y))
        }

        public class VMBenchmark
        {
            List<VMTime> time_res;
            List<VMAccuracy> acc_res;

            public VMBenchmark(int n = 4)
            {
                time_res = new List<VMTime>(n);
                acc_res = new List<VMAccuracy>(n);
            }

            public bool Calculate((double, double) segment, uint num)
            {
                bool res = true;
                VMTime time = new VMTime();
                VMAccuracy acc = new VMAccuracy();
                double []coords = new double[num];
                float[] coords_f = new float[num];
                try
                {
                    double dx = (segment.Item2 - segment.Item2) / (num - 1);
                    for (int i = 0; i < num; i++)
                    {
                        coords[i] = segment.Item1 + dx * i;
                        coords_f[i] = (float)coords[i];
                    }

                    time.length = num;
                    acc.length = num;
                    acc.segment = segment;
                    time.ratio = new double[4];

                    float[] y_s_ha = new float[num], y_s_ep = new float[num];
                    double[] y_d_ha = new double[num], y_d_ep = new double[num];

                    int ret = 0;

                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    call_vmdLn(num, coords, y_d_ha, true, ref ret);
                    timer.Stop();
                    
                    long[] t = new long[4];
                    t[3] = timer.ElapsedTicks;
                    //Console.WriteLine(timer.ElapsedTicks);

                    timer = new Stopwatch();
                    timer.Start();
                    call_vmdLn(num, coords, y_d_ep, false, ref ret);
                    timer.Stop();
                    
                    t[2] = timer.ElapsedTicks;
                    //Console.WriteLine(timer.ElapsedTicks);


                    timer = new Stopwatch();
                    timer.Start();
                    call_vmsLn(num, coords_f, y_s_ep, false, ref ret);
                    timer.Stop();
                    
                    t[0] = timer.ElapsedTicks;
                    //Console.WriteLine(timer.ElapsedTicks);


                    timer = new Stopwatch();
                    timer.Start();
                    call_vmsLn(num, coords_f, y_s_ha, true, ref ret);
                    timer.Stop();
                    
                    t[1] = timer.ElapsedTicks;
                    //Console.WriteLine(timer.ElapsedTicks);

                    for (uint i = 0; i < 4; i++)
                    {
                        time.ratio[i] = (double)t[i] / t[3];
                    }

                    uint k_s = 0, k_d = 0;
                    float max_s = 0;
                    double max_d = 0;
                    for (uint i = 0; i < num; i++)
                    {
                        if (Math.Abs((y_d_ha[i] - y_d_ep[i]) / y_d_ha[i]) > max_d)
                        {
                            k_d = i;
                            max_d = Math.Abs((y_d_ha[i] - y_d_ep[i]) / y_d_ha[i]);
                        }
                        if (Math.Abs((y_s_ha[i] - y_s_ep[i]) / y_s_ha[i]) > max_s)
                        {
                            k_s = i;
                            max_s = Math.Abs((y_s_ha[i] - y_s_ep[i]) / y_s_ha[i]);
                        }
                    }

                    acc.max_ratio = (max_s, max_d);
                    acc.arguments = ((coords_f[k_s], y_s_ha[k_s]), (coords[k_d], y_d_ha[k_d]));

                    time_res.Add(time);
                    acc_res.Add(acc);
                }
                catch(Exception e)
                {
                    Console.WriteLine("The calculation cannot be performed");
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    res = false;
                }
                return res;
            }

            public override string ToString()
            {
                string str = "";
                for (int i = 0; i < time_res.Count; i++)
                {
                    str += i.ToString() + ": " + "number of elements " + time_res[i].length.ToString() + " segment " +
                            acc_res[i].segment.ToString() + " time ratio SEP: " + time_res[i].ratio[0].ToString() + 
                            " SHA: " + time_res[i].ratio[1] + " DEP: " + time_res[i].ratio[2] + " DHA: " + time_res[i].ratio[3] +
                            " accure ratio " + acc_res[i].max_ratio.ToString() + " arguments " + 
                            acc_res[i].arguments.ToString() + "\n";
                }
                return str;
            }

            public bool Save(string fileName)
            {
                bool res = true;
                try
                {
                    using (StreamWriter writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        writer.Write(ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file could not be writen:");
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    res = false;
                }
                return res;
            }
        }


        static void Main(string[] args)
        {
            VMBenchmark bench = new VMBenchmark();
            bench.Calculate((2, 3), 11);
            bench.Calculate((2, 3), 101);
            bench.Calculate((2, 3), 1001);
            bench.Calculate((100, 200), 11);
            bench.Calculate((100, 200), 101);
            bench.Calculate((100, 200), 1001);

            bench.Save("benchmark.txt");
        }



        //[DllImport("..\\..\\..\\..\\x64\\DEBUG\\CPP_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern
        //void VM_Sqrt_Double(int n, double[] x, double[] y, ref int ret);
        [DllImport("..\\..\\..\\..\\x64\\Debug\\CPP_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void call_vmdLn(uint n, double[] a, double[] y, bool mode, ref int ret);
        [DllImport("..\\..\\..\\..\\x64\\Debug\\CPP_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void call_vmsLn(uint n, float[] a, float[] y, bool mode, ref int ret);
    }

}
