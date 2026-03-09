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

    public bool TryDequeueFront(out T value)
    {
        if (Count == 0)
        {
            value = default!;
            return false;
        }

        var item = firstItem!;

        firstItem = item.NextItem;

        if (firstItem != null)
            firstItem.PreviousItem = null;
        else
            lastItem = null;

        Count--;
        value = item.Value;
        return true;
    }

    public bool TryDequeueBack(out T value)
    {
        if (Count == 0)
        {
            value = default!;
            return false;
        }

        var item = lastItem!;

        lastItem = item.PreviousItem;

        if (lastItem != null)
            lastItem.NextItem = null;
        else
            firstItem = null;

        Count--;

        value = item.Value;
        return true;
    }

    public bool TryPeekFront(out T value)
    {
        if (Count == 0)
        {
            value = default!;
            return false;
        }

        value = firstItem!.Value;
        return true;
    }

    public bool TryPeekBack(out T value)
    {
        if (Count == 0)
        {
            value = default!;
            return false;
        }

        value = lastItem!.Value;
        return true;
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