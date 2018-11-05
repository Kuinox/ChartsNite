using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace ChartsNite.Data.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            int result = new AutoRun(typeof(Program).GetTypeInfo().Assembly)
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
            Console.ReadKey();
            return result;
        }
    }
}
