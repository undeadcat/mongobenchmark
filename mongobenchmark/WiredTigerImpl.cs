using System;
using System.IO;
using System.Linq;
using System.Threading;
using Kontur.Elba.Core.Tests;
using WiredTigerNet;

namespace mongobenchmark
{
	public class WiredTigerImpl : IImpl
	{
		private const string Table = "table:documents";
		private readonly string path = "C:\\tmp\\WiredTiger";
		private readonly Connection connection;
		private readonly byte[] initialValue = new byte[1024];
		private readonly ThreadLocal<Session> session;

		public WiredTigerImpl(string config)
		{
			Helpers.Random.Value.NextBytes(initialValue);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			connection = Connection.Open(path,
				string.Format("cache_size=16GB,create=true,checkpoint=(wait=5){0}", string.IsNullOrEmpty(config) ? "" : "," + config));
			session = new ThreadLocal<Session>(() => connection.OpenSession(""));
			session.Value.Create(Table,
				"key_format=u,value_format=u,block_compressor=snappy,columns=(key,value)"
				);
		}

		public void Clear()
		{
			session.Value.Drop(Table);
			session.Value.Create(Table,
			"key_format=u,value_format=u,block_compressor=snappy,columns=(key,value)");
		}

		public void Insert(int key)
		{
			using (var cursor = session.Value.OpenCursor(Table))
			{
				try
				{
					session.Value.BeginTran();
					cursor.Insert(BitConverter.GetBytes(key), initialValue);
				}
				finally
				{
					session.Value.CommitTran();
				}
			}
		}

		public void Update(int key, byte[] value)
		{
			using (var cursor = session.Value.OpenCursor(Table))
			try
			{
				session.Value.BeginTran();
				cursor.Insert(BitConverter.GetBytes(key), initialValue);
			}
			finally
			{
				session.Value.CommitTran();
			}
		}

		public byte[] Find(int key)
		{
			using (var cursor = session.Value.OpenCursor(Table))
			{
				if (!cursor.Search(BitConverter.GetBytes(key)))
					throw new InvalidOperationException(string.Format("Value {0} not found", key));
				return cursor.GetValue();
			}
		}

		public void BulkInsert(int start, int count)
		{
			using (var cursor = session.Value.OpenCursor(Table))
				try
				{
					session.Value.BeginTran();
					foreach (var val in Enumerable.Range(start, count))
						cursor.Insert(BitConverter.GetBytes(val), initialValue);
				}
				finally
				{
					session.Value.CommitTran();
				}
		}

		public void Dispose()
		{
			if (session.IsValueCreated)
				session.Value.Dispose();
			if (connection != null)
				connection.Dispose();
		}
	}
}