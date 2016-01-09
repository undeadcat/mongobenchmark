using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mongobenchmark
{
	public static class Helpers
	{
		public static readonly ThreadLocal<Random> Random =
			new ThreadLocal<Random>(() => new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0)));

		public static TimeSpan RunParallel(int threads, int count, Action iteration)
		{
			var doneOps = 0;
			var sw = Stopwatch.StartNew();
			Task.WaitAll(Enumerable.Range(1, threads).Select(_ => Task.Run(() =>
			{
				while (doneOps < count)
				{
					Interlocked.Increment(ref doneOps);
					iteration();
				}
			})).ToArray());
			return sw.Elapsed;
		}
	}
}