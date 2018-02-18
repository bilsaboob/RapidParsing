using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;
using RapidPliant.Test;

namespace RapidPliant.Benchmark
{
    public static class BenchmarkRunner
    {
        public static void Run<TBenchmark>(bool debug = false)
            where TBenchmark : ITest, new()
        {
            if (!debug)
            {
                BenchmarkDotNet.Running.BenchmarkRunner.Run<TBenchmark>(new BenchmarkConfig());
            }
            else
            {
                var benchmark = new TBenchmark();
                benchmark.Setup(true);
                benchmark.Run(true);
            }

            Console.Read();
        }
    }
    
    public class TestBenchmark<TTest> : ITest
        where TTest : ITest, new()
    {
        public TTest Test { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            Setup(true);
        }
        
        [Benchmark]
        public bool Run()
        {
            return Run(true);
        }

        protected virtual TTest CreateTest()
        {
            return new TTest();
        }

        public void Setup(bool benchmark)
        {
            if (Test == null)
                Test = CreateTest();

            Test.Setup(benchmark);
        }

        public bool Run(bool benchmark)
        {
            return Test.Run(benchmark);
        }
    }

    public class BenchmarkConfig : IConfig
    {
        public BenchmarkConfig()
        {
        }

        public IEnumerable<IColumnProvider> GetColumnProviders()
        {
            return (IEnumerable<IColumnProvider>)DefaultColumnProviders.Instance;
        }

        public IEnumerable<IExporter> GetExporters()
        {
            /*yield return CsvExporter.Default;
            yield return MarkdownExporter.GitHub;
            yield return HtmlExporter.Default;*/
            yield break;
        }

        public IEnumerable<ILogger> GetLoggers()
        {
            yield return ConsoleLogger.Default;
        }

        public IEnumerable<IAnalyser> GetAnalysers()
        {
            yield return EnvironmentAnalyser.Default;
            yield return OutliersAnalyser.Default;
            yield return MinIterationTimeAnalyser.Default;
            yield return IterationSetupCleanupAnalyser.Default;
        }

        public IEnumerable<IValidator> GetValidators()
        {
            yield return (IValidator)BaselineValidator.FailOnError;
            yield return (IValidator)SetupCleanupValidator.FailOnError;
            yield return JitOptimizationsValidator.FailOnError;
            yield return UnrollFactorValidator.Default;
        }

        public IEnumerable<Job> GetJobs()
        {
            return (IEnumerable<Job>)Array.Empty<Job>();
        }

        public IOrderProvider GetOrderProvider()
        {
            return (IOrderProvider)null;
        }

        public ConfigUnionRule UnionRule
        {
            get
            {
                return ConfigUnionRule.Union;
            }
        }

        public bool KeepBenchmarkFiles
        {
            get
            {
                return false;
            }
        }

        public IEnumerable<BenchmarkLogicalGroupRule> GetLogicalGroupRules()
        {
            return (IEnumerable<BenchmarkLogicalGroupRule>)Array.Empty<BenchmarkLogicalGroupRule>();
        }

        public ISummaryStyle GetSummaryStyle()
        {
            return (ISummaryStyle)SummaryStyle.Default;
        }

        public IEnumerable<IDiagnoser> GetDiagnosers()
        {
            return (IEnumerable<IDiagnoser>)Array.Empty<IDiagnoser>();
        }

        public IEnumerable<HardwareCounter> GetHardwareCounters()
        {
            return (IEnumerable<HardwareCounter>)Array.Empty<HardwareCounter>();
        }

        public IEnumerable<IFilter> GetFilters()
        {
            return (IEnumerable<IFilter>)Array.Empty<IFilter>();
        }
    }
}
