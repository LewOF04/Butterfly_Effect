public class DEQueue<T>
{
    private DEQueueItem<T>? firstItem;
    private DEQueueItem<T>? lastItem;
    public int Count { get; private set; }

    public void EnqueueFront(T element)
    {
        var item = new DEQueueItem<T>(element);

        if (firstItem == null) firstItem = lastItem = item;
        else
        {
            item.NextItem = firstItem;
            firstItem.PreviousItem = item;
            firstItem = item;
        }

        Count++;
    }

    public void EnqueueBack(T element)
    {
        var item = new DEQueueItem<T>(element);

        if (lastItem == null) firstItem = lastItem = item;
        else
        {
            item.PreviousItem = lastItem;
            lastItem.NextItem = item;
            lastItem = item;
        }

        Count++;
    }

    public T DequeueFront()
    {
        if (Count == 0) throw new InvalidOperationException("Deque is empty.");

        var item = firstItem!;

        firstItem = item.NextItem;

        if (firstItem != null) firstItem.PreviousItem = null;
        else lastItem = null;

        Count--;
        return item.Value;
    }

    public T DequeueBack()
    {
        if (Count == 0) throw new InvalidOperationException("Deque is empty.");

        var item = lastItem!;

        lastItem = item.PreviousItem;

        if (lastItem != null) lastItem.NextItem = null;
        else firstItem = null;

        Count--;
        return item.Value;
    }

    public T ReadFront()
    {
        if (Count == 0) return default;

        return firstItem!.Value;
    }

    public T ReadBack()
    {
        if (Count == 0) return default;

        return lastItem!.Value;
    }
}

internal class DEQueueItem<T>
{
    public DEQueueItem(T value)
    {
        Value = value;
    }

    public DEQueueItem<T>? NextItem { get; set; }
    public DEQueueItem<T>? PreviousItem { get; set; }
    public T Value { get; }
}