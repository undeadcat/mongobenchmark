namespace Kontur.Elba.Core.Tests
{
	public interface IImpl
	{
		void Clear();
		void Insert(int key);
		void Update(int key, byte[] value);
		byte[] Find(int key);
		void BulkInsert(int start, int count);
	}
}