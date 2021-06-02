using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Parcs;

namespace FirstModule
{
    class MainModule : IModule
    {
        static int tasks = 2;
        static String text = "AABB";

        static void Test()
        {
            String text = "daxfvbi$";
            List<Int32> order = new List<Int32>();
            for (int i = 0; i < text.Length; i++)
            {
                order.Add(i);
            }
            String res = FMIndexModule.BuildFMIndex(text, order);
            Console.WriteLine(res);
            Console.ReadKey();
        }
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter number of tasks:");
            tasks = Convert.ToInt32(Console.ReadLine());

            text = File.ReadAllText("text.txt");

            text = text.Replace("\n", "");
            text = text.Replace("\r", "");
            text = text.Replace("\0", "");
            text = text.Replace("\t", "");
            text = text.Replace(" ", "");

            Console.WriteLine("Start!");

            var job = new Job();
            if (!job.AddFile(Assembly.GetExecutingAssembly().Location))
            {
                Console.WriteLine("File doesn't exist");
                return;
            }

            (new MainModule()).Run(new ModuleInfo(job, null));
            Console.ReadKey();
        }

        public void Run(ModuleInfo info, CancellationToken token = default(CancellationToken))
        {
            var points = new IPoint[tasks];
            var channels = new IChannel[tasks];
            for (int i = 0; i < tasks; ++i)
            {
                points[i] = info.CreatePoint();
                channels[i] = points[i].CreateChannel();
                points[i].ExecuteClass("FirstModule.FMIndexModule");
            }
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            SortedDictionary<String, List<Int32>> parts = new SortedDictionary<String, List<int>>();
            for (int i = 0; i < text.Length; i++)
            {
                string key = "" + text[i];
                if (!parts.ContainsKey(key)) parts.Add(key, new List<Int32>());
                parts[key].Add(i);
            }

            int taskSize = text.Length / tasks;


            List<List<String>> a = new List<List<String>>();
            a.Add(new List<String>());

            var keys = parts.Keys;

            int total = 0;
            foreach (var entry in parts)
            {
                total += entry.Value.Count;
                a[a.Count - 1].Add(entry.Key);
                if (total > taskSize)
                {
                    total = 0;
                    a.Add(new List<String>());
                }
            }

            Parallel.For(0, tasks, (i) =>
            {
                {
                    var order = new List<Int32>();
                    foreach (String key in a[i])
                    {
                        order.AddRange(parts[key]);
                    }
                    channels[i].WriteData(text);
                    channels[i].WriteObject(order);
                    Console.WriteLine("Task #" + i.ToString() + " started. Length: " + order.Count.ToString() + ".");
                }
            });

            String bwt = "";

            for (int i = 0; i < tasks; ++i)
            {
                bwt += channels[i].ReadString();
            }
            stopWatch.Stop();
            Console.WriteLine("Time: " + stopWatch.Elapsed);
            if (bwt.Length > 16)
            {
                Console.WriteLine("BWT Transform: (too long, written to the file res.txt)");
                Console.WriteLine(bwt.Substring(0, 16) + "...");
                File.WriteAllLines("res.txt", new string[] { bwt });
            }
            else
            {
                Console.WriteLine("BWT Transform:");
                Console.WriteLine(bwt);
            }
        }
    }
}
