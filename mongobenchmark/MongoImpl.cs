using System;
using System.Linq;
using Kontur.Elba.Core.Tests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace mongobenchmark
{
	public class MongoImpl : IImpl
	{
		private readonly MongoCollection<SomeDocument> collection;
		private readonly byte[] initialValue = new byte[1024];
		private readonly WriteConcern writeConcern;

		public MongoImpl(MongoServer connection, WriteConcern writeConcern)
		{
			this.writeConcern = writeConcern;
			Helpers.Random.Value.NextBytes(initialValue);
			collection =
				connection.GetDatabase(typeof (SomeDocument).Name).GetCollection<SomeDocument>(typeof (SomeDocument).Name);
		}

		public void Insert(int key)
		{
			collection.Insert(new SomeDocument {Key = key, Value = initialValue}, writeConcern);
		}

		public void Update(int key, byte[] value)
		{
			collection.Update(Query<SomeDocument>.EQ(x => x.Key, key), new UpdateBuilder().Set("Value", BsonValue.Create(value)),
				writeConcern);
		}

		public byte[] Find(int key)
		{
			var document = collection.Find(Query<SomeDocument>.EQ(x => x.Key, key)).FirstOrDefault();
			if (document == null)
				throw new InvalidOperationException(string.Format("Document by key {0} not found", key));
			return document.Value;
		}

		public void BulkInsert(int start, int count)
		{
			collection.InsertBatch(Enumerable.Range(start, count).Select(x => new SomeDocument {Key = x, Value = initialValue}));
		}

		public void Clear()
		{
			collection.Drop();
		}

		public class SomeDocument
		{
			[BsonId] public int Key;

			public byte[] Value;
		}

		public void Dispose()
		{
			
		}
	}
}