using System;
using System.Collections;
using System.Collections.Generic;

namespace Classes
{
    public interface IBuffer<T> : IEnumerable<T>
    {
        bool IsEmpty { get; }
        void Write(T value);
        T Read();
    }

    public class Buffer<T> : IBuffer<T>
    {
        protected Queue<T> _queue = new Queue<T>();
        public virtual bool IsEmpty 
        {
            get { return _queue.Count == 0; }
        }

        public virtual void Write(T value)
        {
            _queue.Enqueue(value);
        }

        public virtual T Read()
        {
            return _queue.Dequeue();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class CircularBuffer<T>: Buffer<T>
    {
        int _capactity;

        public event EventHandler<ItemDiscardedEventArgs<T>> ItemsDiscarded;

        public CircularBuffer(int capacity = 10)
        {
            _capactity = capacity;
        }

        public override void Write(T value)
        {
            base.Write(value);
            if (_queue.Count > _capactity)
            {
                var discarded = _queue.Dequeue();
                if (ItemsDiscarded != null)
                {
                    var args = new ItemDiscardedEventArgs<T>(discarded, value);
                    ItemsDiscarded(this, args);
                }
            }
        }
    }

    public class ItemDiscardedEventArgs<T>:EventArgs
    {
        public ItemDiscardedEventArgs(T itemDiscarded, T newItem)
        {
            ItemDiscarded = itemDiscarded;
            NewItem = newItem;
        }
        public T ItemDiscarded { get; set; }
        public T NewItem { get; set; }
    }

}
