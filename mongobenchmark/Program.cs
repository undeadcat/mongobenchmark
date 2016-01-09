using System;
using System.IO;
using System.Linq;
using mongobenchmark;
using MongoDB.Driver;

namespace Kontur.Elba.Core.Tests
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var connection = new MongoServer(new MongoServerSettings
			{
				WriteConcern = WriteConcern.Acknowledged,
				Servers = new[] {new MongoServerAddress("elba", 27017)}
			});
			var impl = new Impl(connection, WriteConcern.Acknowledged);
			var itemCount = 20*1000*1000;
			var opsCount = 50000;
			Logger("mmapv1, 20mln documents");
			RwBenchmark.Prepare(impl, Logger, itemCount);
			var readRatios = new[] {0, 0.5, 0.8, 1};
			var threadCounts = Enumerable.Range(0, 8).Select(i => (int) Math.Pow(2, i)).ToArray();
			foreach (var readRatio in readRatios)
			{
				foreach (var threadCount in threadCounts)
				{
					var benchmark = new RwBenchmark(impl, readRatio, threadCount, opsCount, itemCount);
					var metrics = benchmark.Run();
					Logger(benchmark.Name);
					foreach (var metric in metrics)
						Logger(metric.ToString());
				}
			}
		}

		private static void Logger(string format, params object[] args)
		{
			Console.WriteLine(format, args);
			File.AppendAllLines("log", new[] {string.Format(format, args)});
		}
	}
}