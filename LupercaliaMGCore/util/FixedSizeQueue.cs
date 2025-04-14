namespace LupercaliaMGCore.util;

public class FixedSizeQueue<T>(int maxSize) : Queue<T>
{
    private int MaxSize { get; } = maxSize;

    public new void Enqueue(T item)
    {
        if (Count >= MaxSize)
        {
            Dequeue();
        }

        base.Enqueue(item);
    }

    public override string ToString()
    {
        return string.Join(", ", this);
    }
}