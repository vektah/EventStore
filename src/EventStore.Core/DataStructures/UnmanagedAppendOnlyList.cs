using System;
using System.Runtime.InteropServices;

namespace EventStore.Core.DataStructures {

	public interface IAppendOnlyList<in T> {
		void Add(T item);
		void Clear();
		int Count { get; }
	}

	public class UnmanagedAppendOnlyList<T> : IAppendOnlyList<T>, IDisposable {
		private readonly int _maxCapacity;
		private readonly IntPtr _dataPtr;

		private int _count;
		private bool _disposed;

		public UnmanagedAppendOnlyList(int maxCapacity) {
			_maxCapacity = maxCapacity;
			_count = 0;
			_dataPtr = IntPtr.Zero;
			_dataPtr = Marshal.AllocHGlobal(new IntPtr((long)_maxCapacity * Marshal.SizeOf(typeof(T))));
		}

		~UnmanagedAppendOnlyList() => Dispose(false);

		public void Add(T item) {
			if (_count >= _maxCapacity)
				throw new MaxCapacityReachedException();

			unsafe
			{
				new Span<T>(_dataPtr.ToPointer(), _maxCapacity) {
					[_count] = item
				};
			}
			_count++;
		}

		public void Clear() {
			_count = 0;
		}

		public int Count => _count;

		public T this[int index] {
			get {
				if (index >= _count) {
					throw new OutOfBoundsException();
				}

				unsafe
				{
					return new ReadOnlySpan<T>(_dataPtr.ToPointer(), _maxCapacity)[index];
				}
			}
		}

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) {
				return;
			}

			if (disposing) {
				//dispose any managed objects here
			}

			if (_dataPtr != IntPtr.Zero) {
				Marshal.FreeHGlobal(_dataPtr);
			}

			_disposed = true;
		}
	}

	public class OutOfBoundsException : Exception {
		public OutOfBoundsException() : base("Out of bounds exception.") {
		}
	}

	public class MaxCapacityReachedException : Exception {
		public MaxCapacityReachedException() : base("Max capacity reached") {
		}
	}
}
