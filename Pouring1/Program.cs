using System;
using Flinq;

namespace Pouring1
{
    class Program
    {
        static void Main(/* string[] args */)
        {
            var pouring = new Pouring(4, 9);
            var solutions = pouring.Solutions(6);
            solutions.ForEach(Console.WriteLine);
        }
    }
}
