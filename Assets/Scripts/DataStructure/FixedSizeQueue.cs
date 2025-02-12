using System.Collections.Generic;

public class FixedSizeQueue<T>
{
    private readonly Queue<T> queue;
    private readonly int maxSize;
    private T lastEnqueued;

    public FixedSizeQueue(int size)
    {
        maxSize = size;
        queue = new Queue<T>(size);
    }

    public void Enqueue(T item)
    {
        if (queue.Count >= maxSize)
        {
            queue.Dequeue(); // Remove the oldest item
        }
        queue.Enqueue(item);
        lastEnqueued = item; // Track the last added element
    }

    public T Dequeue()
    {
        return queue.Dequeue();
    }

    public T GetLastEnqueued => lastEnqueued;
    public int Count => queue.Count;
    public T Peek() => queue.Peek();
    public bool IsFull => queue.Count >= maxSize;
    public void Clear() { queue.Clear(); }
}