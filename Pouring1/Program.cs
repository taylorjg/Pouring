using System;
using System.Linq;

namespace Pouring1
{
    class Program
    {
        static void Main(string[] args)
        {
            var capacities = new[] {4,9};
            var target = 6;

            if (args.Length != 0 && args.Length != 2)
            {
                Usage();
            }

            if (args.Length == 2)
            {
                capacities = args[0]
                    .Split(',')
                    .Select(arg => arg.Trim())
                    .Select(arg => Convert.ToInt32(arg))
                    .ToArray();
                target = Convert.ToInt32(args[1]);
            }

            var pouring = new Pouring(capacities);
            var firstSolution = pouring.Solutions(target).First();
            Console.WriteLine(firstSolution);
        }

        static void Usage()
        {
            Console.Error.WriteLine("Pouring1 [ <capacities> <target> ]");
            Console.Error.WriteLine("\te.g. Pouring1 4,9 6");
            Environment.Exit(1);
        }
    }
}
