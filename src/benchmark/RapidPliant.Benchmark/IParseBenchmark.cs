using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.Benchmark
{
    public interface IParseBenchmark
    {
        void Setup();

        bool Run();
    }
}
