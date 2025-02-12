using System.Collections.Generic;

public class FixedSizeStack<T>
{
    private readonly Stack<T> stack;
    private readonly int maxSize;

    public FixedSizeStack(int size)
    {
        maxSize = size;
        stack = new Stack<T>(size);
    }

    public void Push(T item)
    {
        if (stack.Count >= maxSize)
        {
            // Remove bottom element to maintain fixed size
            Stack<T> tempStack = new Stack<T>(stack.Count);
            while (stack.Count > 1)
            {
                tempStack.Push(stack.Pop());
            }
            stack.Pop(); // Remove oldest item
            while (tempStack.Count > 0)
            {
                stack.Push(tempStack.Pop());
            }
        }
        stack.Push(item);
    }

    public T Pop()
    {
        return stack.Pop();
    }

    public T Peek()
    {
        return stack.Peek();
    }

    public int Count => stack.Count;
    public bool IsFull => stack.Count >= maxSize;
    public void Clear() => stack.Clear();
}
