namespace LupercaliaMGCore;

public class FixedSizeQueue<T> : Queue<T>
{
    private int maxSize { get; }

    public FixedSizeQueue(int maxSize)
    {
        this.maxSize = maxSize;
    }

    public new void Enqueue(T item)
    {
        if (Count >= maxSize)
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