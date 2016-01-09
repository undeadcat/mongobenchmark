using System;
using Kontur.Elba.Core.Tests;

namespace mongobenchmark
{
	public interface IBenchmark
	{
		string Name { get; }
		Metric.Stats[] Run();
	}

	public class RwBenchmark : IBenchmark
	{
		private readonly int _opsCount;

		private readonly Action _readAction;
		private readonly double _readRatio;
		private readonly int _threads;
		private readonly Action _writeAction;

		public RwBenchmark(IImpl impl, double readRatio, int threads, int opsCount, int maxKey)
		{
			_readRatio = readRatio;
			_opsCount = opsCount;
			_threads = threads;
			_readAction = () => impl.Find(Helpers.Random.Value.Next(maxKey));
			_writeAction = () =>
			{
				var buffer = new byte[1024];
				Helpers.Random.Value.NextBytes(buffer);
				impl.Update(Helpers.Random.Value.Next(maxKey), buffer);
			};
		}

		public string Name
		{
			get { return string.Format("{0}r/{1}w, {2} threads", _readRatio*100, 100 - _readRatio*100, _threads); }
		}

		public Metric.Stats[] Run()
		{
			var readMetric = new Metric("Read");
			var writeMetric = new Metric("Write");
			var elapsed = Helpers.RunParallel(_threads, _opsCount, () =>
			{
				if (Helpers.Random.Value.NextDouble() < _readRatio)
					readMetric.Register(_readAction);
				else writeMetric.Register(_writeAction);
			});

			return new[] {readMetric.GetStats(elapsed), writeMetric.GetStats(elapsed)};
		}

		public static void Prepare(IImpl impl, Logger logger, int maxKey)
		{
			logger("starting cleanup");
			impl.Clear();
			logger("Clean");
			var bulksize = 1024;
			var inserted = 0;
			while (inserted < maxKey)
			{
				impl.BulkInsert(inserted, bulksize);
				inserted += bulksize;
			}
			logger("Prepare done, inserted: {0}", maxKey);
		}
	}

	public class InsertBenchmark : IBenchmark
	{
		private readonly int _count;
		private readonly IImpl _impl;
		private readonly int _maxKey;
		private readonly int _threads;

		public InsertBenchmark(int count, int threads, IImpl impl, int maxKey)
		{
			_count = count;
			_threads = threads;
			_impl = impl;
			_maxKey = maxKey;
		}

		public string Name
		{
			get { return string.Format("Insert, {0} items, {1} threads", _count, _threads); }
		}

		public Metric.Stats[] Run()
		{
			var val = _maxKey;
			var metric = new Metric("Insert");
			return new[]
			{metric.GetStats(Helpers.RunParallel(_threads, _count, () => metric.Register(() => _impl.Insert(val++))))};
		}
	}
}