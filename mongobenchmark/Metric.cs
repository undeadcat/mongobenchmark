using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace mongobenchmark
{
	public class Metric
	{
		private readonly ConcurrentStack<long> _ticks = new ConcurrentStack<long>();

		public Metric(string name)
		{
			Name = name;
		}

		public string Name { get; private set; }

		public void Register(Action action)
		{
			var sw = Stopwatch.StartNew();
			try
			{
				action();
			}
			finally
			{
				_ticks.Push(sw.Elapsed.Ticks);
			}
		}
		public Stats GetStats(TimeSpan wallTime)
		{
			var values = _ticks.OrderBy(x => x).ToArray();
			if (values.Length == 0)
				return new Stats();
			return new Stats
			{
				Name = Name,
				Count = values.Length,
				Min = new TimeSpan(values.Min()).TotalMilliseconds,
				Average = new TimeSpan((long) (values.Sum()/(double) values.Length)).TotalMilliseconds,
				P90 = new TimeSpan(values.Take((int) (values.Length*0.9)).Max()).TotalMilliseconds,
				P95 = new TimeSpan(values.Take((int) (values.Length*0.95)).Max()).TotalMilliseconds,
				P99 = new TimeSpan(values.Take((int) (values.Length*0.99)).Max()).TotalMilliseconds,
				Max = new TimeSpan(values.Max()).TotalMilliseconds,
				Total = wallTime,
				ThroughputPerSecond = values.Length / wallTime.TotalSeconds
			};
		}

		public class Stats
		{
			public double Average;
			public int Count;
			public double Max;
			public double Min;
			public double P90;
			public double P95;
			public double P99;
			public double ThroughputPerSecond;
			public TimeSpan Total;
			public string Name;

			public override string ToString()
			{
				return
					string.Format(
						"{9}\r\nCount: {3}, Total: {0}, ThroughputPerSecond: {1}, Average: {2}ms, Max: {4}ms, Min: {5}ms, P90: {6}ms, P95: {7}ms, P99: {8}ms",
						Total, ThroughputPerSecond, Average, Count, Max, Min, P90, P95, P99, Name);
			}
		}
	}
}