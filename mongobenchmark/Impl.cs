using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Kontur.Elba.Core.Tests
{
	public class Impl : IImpl
	{
		private readonly MongoCollection<SomeDocument> _collection;
		private readonly byte[] _initialValue = new byte[1024];
		private readonly WriteConcern _writeConcern;

		public Impl(MongoServer connection, WriteConcern writeConcern)
		{
			_writeConcern = writeConcern;
			new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray().Take(4).ToArray(), 0)).NextBytes(_initialValue);
			_collection =
				connection.GetDatabase(typeof (SomeDocument).Name).GetCollection<SomeDocument>(typeof (SomeDocument).Name);
		}

		public void Insert(int key)
		{
			_collection.Insert(new SomeDocument {Key = key, Value = _initialValue}, _writeConcern);
		}

		public void Update(int key, byte[] value)
		{
			_collection.Update(Query<SomeDocument>.EQ(x => x.Key, key), new UpdateBuilder().Set("Value", BsonValue.Create(value)),
				_writeConcern);
		}

		public byte[] Find(int key)
		{
			var document = _collection.Find(Query<SomeDocument>.EQ(x => x.Key, key)).FirstOrDefault();
			if (document == null)
				throw new InvalidOperationException(string.Format("Document by key {0} not found", key));
			return document.Value;
		}

		public void BulkInsert(int start, int count)
		{
			_collection.InsertBatch(Enumerable.Range(start, count).Select(x => new SomeDocument {Key = x, Value = _initialValue}));
		}

		public void Clear()
		{
			_collection.Drop();
		}

		public class SomeDocument
		{
			[BsonId] public int Key;

			public byte[] Value;
		}
	}
}