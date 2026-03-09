/*
This is the .NET6 implementation of priority queue which has been adapted from the official github repository. This is part of the System.Collections.Generic namespace.

dotnet. “Runtime/Src/Libraries/System.Collections/Src/System/Collections/Generic/PriorityQueue.cs at Main · Dotnet/Runtime.” 
GitHub, 2019, github.com/dotnet/runtime/blob/main/src/libraries/System.Collections/src/System/Collections/Generic/PriorityQueue.cs. 
Accessed 9 Mar. 2026.

Unity has not yet migrated to this version of .NET (as of 09/03/2026), therefore PriorityQueue is not available, so an implementation is
explicitly included for this project.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class PriorityQueue<TElement, TPriority>
{
    /// <summary>
    /// Represents an implicit heap-ordered complete d-ary tree, stored as an array.
    /// </summary>
    private (TElement Element, TPriority Priority)[] _nodes;

    /// <summary>
    /// Custom comparer used to order the heap.
    /// Null means "use Comparer&lt;TPriority&gt;.Default via optimized path".
    /// </summary>
    private readonly IComparer<TPriority> _comparer;

    /// <summary>
    /// Cached unordered-items collection.
    /// </summary>
    private UnorderedItemsCollection _unorderedItems;

    /// <summary>
    /// The number of nodes in the heap.
    /// </summary>
    private int _size;

    /// <summary>
    /// Version updated on mutation to help validate enumerators operate on a consistent state.
    /// </summary>
    private int _version;

    /// <summary>
    /// Specifies the arity of the d-ary heap, which here is quaternary.
    /// It is assumed that this value is a power of 2.
    /// </summary>
    private const int Arity = 4;

    /// <summary>
    /// The binary logarithm of <see cref="Arity" />.
    /// </summary>
    private const int Log2Arity = 2;

    /// <summary>
    /// Conservative maximum array length for compatibility with older runtimes.
    /// </summary>
    private const int MaxArrayLength = 0X7FEFFFFF;

#if DEBUG
    static PriorityQueue()
    {
        Debug.Assert(Log2Arity > 0 && Math.Pow(2, Log2Arity) == Arity);
    }
#endif

    public PriorityQueue()
    {
        _nodes = new (TElement, TPriority)[0];
        _comparer = InitializeComparer(null);
    }

    public PriorityQueue(int initialCapacity)
        : this(initialCapacity, null)
    {
    }

    public PriorityQueue(IComparer<TPriority> comparer)
    {
        _nodes = new (TElement, TPriority)[0];
        _comparer = InitializeComparer(comparer);
    }

    public PriorityQueue(int initialCapacity, IComparer<TPriority> comparer)
    {
        if (initialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException("initialCapacity");
        }

        _nodes = new (TElement, TPriority)[initialCapacity];
        _comparer = InitializeComparer(comparer);
    }

    public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items)
        : this(items, null)
    {
    }

    public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> items, IComparer<TPriority> comparer)
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }

        _nodes = EnumerableHelpers.ToArray(items, out _size);
        _comparer = InitializeComparer(comparer);

        if (_size > 1)
        {
            Heapify();
        }
    }

    public int Count
    {
        get { return _size; }
    }

    public int Capacity
    {
        get { return _nodes.Length; }
    }

    public IComparer<TPriority> Comparer
    {
        get { return _comparer ?? Comparer<TPriority>.Default; }
    }

    public UnorderedItemsCollection UnorderedItems
    {
        get
        {
            if (_unorderedItems == null)
            {
                _unorderedItems = new UnorderedItemsCollection(this);
            }

            return _unorderedItems;
        }
    }

    public void Enqueue(TElement element, TPriority priority)
    {
        int currentSize = _size;
        _version++;

        if (_nodes.Length == currentSize)
        {
            Grow(currentSize + 1);
        }

        _size = currentSize + 1;

        if (_comparer == null)
        {
            MoveUpDefaultComparer((element, priority), currentSize);
        }
        else
        {
            MoveUpCustomComparer((element, priority), currentSize);
        }
    }

    public TElement Peek()
    {
        if (_size == 0)
        {
            throw new InvalidOperationException(errorStrings.InvalidOperation_EmptyQueue);
        }

        return _nodes[0].Element;
    }

    public TElement Dequeue()
    {
        if (_size == 0)
        {
            throw new InvalidOperationException(errorStrings.InvalidOperation_EmptyQueue);
        }

        TElement element = _nodes[0].Element;
        RemoveRootNode();
        return element;
    }

    public TElement DequeueEnqueue(TElement element, TPriority priority)
    {
        if (_size == 0)
        {
            throw new InvalidOperationException(errorStrings.InvalidOperation_EmptyQueue);
        }

        (TElement Element, TPriority Priority) root = _nodes[0];

        if (_comparer == null)
        {
            if (Comparer<TPriority>.Default.Compare(priority, root.Priority) > 0)
            {
                MoveDownDefaultComparer((element, priority), 0);
            }
            else
            {
                _nodes[0] = (element, priority);
            }
        }
        else
        {
            if (_comparer.Compare(priority, root.Priority) > 0)
            {
                MoveDownCustomComparer((element, priority), 0);
            }
            else
            {
                _nodes[0] = (element, priority);
            }
        }

        _version++;
        return root.Element;
    }

    public bool TryDequeue(out TElement element, out TPriority priority)
    {
        if (_size != 0)
        {
            element = _nodes[0].Element;
            priority = _nodes[0].Priority;
            RemoveRootNode();
            return true;
        }

        element = default(TElement);
        priority = default(TPriority);
        return false;
    }

    public bool TryPeek(out TElement element, out TPriority priority)
    {
        if (_size != 0)
        {
            element = _nodes[0].Element;
            priority = _nodes[0].Priority;
            return true;
        }

        element = default(TElement);
        priority = default(TPriority);
        return false;
    }

    public TElement EnqueueDequeue(TElement element, TPriority priority)
    {
        if (_size != 0)
        {
            (TElement Element, TPriority Priority) root = _nodes[0];

            if (_comparer == null)
            {
                if (Comparer<TPriority>.Default.Compare(priority, root.Priority) > 0)
                {
                    MoveDownDefaultComparer((element, priority), 0);
                    _version++;
                    return root.Element;
                }
            }
            else
            {
                if (_comparer.Compare(priority, root.Priority) > 0)
                {
                    MoveDownCustomComparer((element, priority), 0);
                    _version++;
                    return root.Element;
                }
            }
        }

        return element;
    }

    public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException("items");
        }

        int count = 0;
        ICollection<(TElement Element, TPriority Priority)> collection = items as ICollection<(TElement Element, TPriority Priority)>;
        if (collection != null && (count = collection.Count) > _nodes.Length - _size)
        {
            Grow(checked(_size + count));
        }

        if (_size == 0)
        {
            if (collection != null)
            {
                collection.CopyTo(_nodes, 0);
                _size = count;
            }
            else
            {
                int i = 0;
                (TElement, TPriority)[] nodes = _nodes;
                foreach ((TElement element, TPriority priority) in items)
                {
                    if (nodes.Length == i)
                    {
                        Grow(i + 1);
                        nodes = _nodes;
                    }

                    nodes[i++] = (element, priority);
                }

                _size = i;
            }

            _version++;

            if (_size > 1)
            {
                Heapify();
            }
        }
        else
        {
            foreach ((TElement element, TPriority priority) in items)
            {
                Enqueue(element, priority);
            }
        }
    }

    public void EnqueueRange(IEnumerable<TElement> elements, TPriority priority)
    {
        if (elements == null)
        {
            throw new ArgumentNullException("elements");
        }

        int count;
        ICollection<TElement> collection = elements as ICollection<TElement>;
        if (collection != null && (count = collection.Count) > _nodes.Length - _size)
        {
            Grow(checked(_size + count));
        }

        if (_size == 0)
        {
            int i = 0;
            (TElement, TPriority)[] nodes = _nodes;
            foreach (TElement element in elements)
            {
                if (nodes.Length == i)
                {
                    Grow(i + 1);
                    nodes = _nodes;
                }

                nodes[i++] = (element, priority);
            }

            _size = i;
            _version++;
        }
        else
        {
            foreach (TElement element in elements)
            {
                Enqueue(element, priority);
            }
        }
    }

    public bool Remove(
        TElement element,
        out TElement removedElement,
        out TPriority priority,
        IEqualityComparer<TElement> equalityComparer = null)
    {
        int index = FindIndex(element, equalityComparer);
        if (index < 0)
        {
            removedElement = default(TElement);
            priority = default(TPriority);
            return false;
        }

        (TElement Element, TPriority Priority)[] nodes = _nodes;
        removedElement = nodes[index].Element;
        priority = nodes[index].Priority;
        int newSize = --_size;

        if (index < newSize)
        {
            (TElement Element, TPriority Priority) lastNode = nodes[newSize];

            if (_comparer == null)
            {
                if (Comparer<TPriority>.Default.Compare(lastNode.Priority, priority) < 0)
                {
                    MoveUpDefaultComparer(lastNode, index);
                }
                else
                {
                    MoveDownDefaultComparer(lastNode, index);
                }
            }
            else
            {
                if (_comparer.Compare(lastNode.Priority, priority) < 0)
                {
                    MoveUpCustomComparer(lastNode, index);
                }
                else
                {
                    MoveDownCustomComparer(lastNode, index);
                }
            }
        }

        nodes[newSize] = default((TElement, TPriority));
        _version++;
        return true;
    }

    public void Clear()
    {
        Array.Clear(_nodes, 0, _size);
        _size = 0;
        _version++;
    }

    public int EnsureCapacity(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentOutOfRangeException("capacity");
        }

        if (_nodes.Length < capacity)
        {
            Grow(capacity);
            _version++;
        }

        return _nodes.Length;
    }

    public void TrimExcess()
    {
        int threshold = (int)(_nodes.Length * 0.9);
        if (_size < threshold)
        {
            Array.Resize(ref _nodes, _size);
            _version++;
        }
    }

    private void Grow(int minCapacity)
    {
        Debug.Assert(_nodes.Length < minCapacity);

        const int GrowFactor = 2;
        const int MinimumGrow = 4;

        int newCapacity = GrowFactor * _nodes.Length;

        if ((uint)newCapacity > MaxArrayLength)
        {
            newCapacity = MaxArrayLength;
        }

        newCapacity = Math.Max(newCapacity, _nodes.Length + MinimumGrow);

        if (newCapacity < minCapacity)
        {
            newCapacity = minCapacity;
        }

        Array.Resize(ref _nodes, newCapacity);
    }

    private void RemoveRootNode()
    {
        int lastNodeIndex = --_size;
        _version++;

        if (lastNodeIndex > 0)
        {
            (TElement Element, TPriority Priority) lastNode = _nodes[lastNodeIndex];
            if (_comparer == null)
            {
                MoveDownDefaultComparer(lastNode, 0);
            }
            else
            {
                MoveDownCustomComparer(lastNode, 0);
            }
        }

        _nodes[lastNodeIndex] = default((TElement, TPriority));
    }

    private static int GetParentIndex(int index)
    {
        return (index - 1) >> Log2Arity;
    }

    private static int GetFirstChildIndex(int index)
    {
        return (index << Log2Arity) + 1;
    }

    private void Heapify()
    {
        int lastParentWithChildren = GetParentIndex(_size - 1);

        if (_comparer == null)
        {
            for (int index = lastParentWithChildren; index >= 0; --index)
            {
                MoveDownDefaultComparer(_nodes[index], index);
            }
        }
        else
        {
            for (int index = lastParentWithChildren; index >= 0; --index)
            {
                MoveDownCustomComparer(_nodes[index], index);
            }
        }
    }

    private void MoveUpDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        Debug.Assert(_comparer == null);
        Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

        (TElement Element, TPriority Priority)[] nodes = _nodes;

        while (nodeIndex > 0)
        {
            int parentIndex = GetParentIndex(nodeIndex);
            (TElement Element, TPriority Priority) parent = nodes[parentIndex];

            if (Comparer<TPriority>.Default.Compare(node.Priority, parent.Priority) < 0)
            {
                nodes[nodeIndex] = parent;
                nodeIndex = parentIndex;
            }
            else
            {
                break;
            }
        }

        nodes[nodeIndex] = node;
    }

    private void MoveUpCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        Debug.Assert(_comparer != null);
        Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

        IComparer<TPriority> comparer = _comparer;
        (TElement Element, TPriority Priority)[] nodes = _nodes;

        while (nodeIndex > 0)
        {
            int parentIndex = GetParentIndex(nodeIndex);
            (TElement Element, TPriority Priority) parent = nodes[parentIndex];

            if (comparer.Compare(node.Priority, parent.Priority) < 0)
            {
                nodes[nodeIndex] = parent;
                nodeIndex = parentIndex;
            }
            else
            {
                break;
            }
        }

        nodes[nodeIndex] = node;
    }

    private void MoveDownDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        Debug.Assert(_comparer == null);
        Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

        (TElement Element, TPriority Priority)[] nodes = _nodes;
        int size = _size;

        int i;
        while ((i = GetFirstChildIndex(nodeIndex)) < size)
        {
            (TElement Element, TPriority Priority) minChild = nodes[i];
            int minChildIndex = i;

            int childIndexUpperBound = Math.Min(i + Arity, size);
            while (++i < childIndexUpperBound)
            {
                (TElement Element, TPriority Priority) nextChild = nodes[i];
                if (Comparer<TPriority>.Default.Compare(nextChild.Priority, minChild.Priority) < 0)
                {
                    minChild = nextChild;
                    minChildIndex = i;
                }
            }

            if (Comparer<TPriority>.Default.Compare(node.Priority, minChild.Priority) <= 0)
            {
                break;
            }

            nodes[nodeIndex] = minChild;
            nodeIndex = minChildIndex;
        }

        nodes[nodeIndex] = node;
    }

    private void MoveDownCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
    {
        Debug.Assert(_comparer != null);
        Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

        IComparer<TPriority> comparer = _comparer;
        (TElement Element, TPriority Priority)[] nodes = _nodes;
        int size = _size;

        int i;
        while ((i = GetFirstChildIndex(nodeIndex)) < size)
        {
            (TElement Element, TPriority Priority) minChild = nodes[i];
            int minChildIndex = i;

            int childIndexUpperBound = Math.Min(i + Arity, size);
            while (++i < childIndexUpperBound)
            {
                (TElement Element, TPriority Priority) nextChild = nodes[i];
                if (comparer.Compare(nextChild.Priority, minChild.Priority) < 0)
                {
                    minChild = nextChild;
                    minChildIndex = i;
                }
            }

            if (comparer.Compare(node.Priority, minChild.Priority) <= 0)
            {
                break;
            }

            nodes[nodeIndex] = minChild;
            nodeIndex = minChildIndex;
        }

        nodes[nodeIndex] = node;
    }

    private int FindIndex(TElement element, IEqualityComparer<TElement> equalityComparer)
    {
        if (equalityComparer == null)
        {
            equalityComparer = EqualityComparer<TElement>.Default;
        }

        for (int i = 0; i < _size; i++)
        {
            if (equalityComparer.Equals(element, _nodes[i].Element))
            {
                return i;
            }
        }

        return -1;
    }

    private static IComparer<TPriority> InitializeComparer(IComparer<TPriority> comparer)
    {
        if (typeof(TPriority).IsValueType)
        {
            if (comparer == Comparer<TPriority>.Default)
            {
                return null;
            }

            return comparer;
        }
        else
        {
            return comparer ?? Comparer<TPriority>.Default;
        }
    }

    public sealed class UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection
    {
        internal readonly PriorityQueue<TElement, TPriority> _queue;

        internal UnorderedItemsCollection(PriorityQueue<TElement, TPriority> queue)
        {
            _queue = queue;
        }

        public int Count
        {
            get { return _queue._size; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException(errorStrings.Arg_RankMultiDimNotSupported, "array");
            }

            if (array.GetLowerBound(0) != 0)
            {
                throw new ArgumentException(errorStrings.Arg_NonZeroLowerBound, "array");
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException("index", errorStrings.ArgumentOutOfRange_IndexMustBeLessOrEqual);
            }

            if (array.Length - index < _queue._size)
            {
                throw new ArgumentException(errorStrings.Argument_InvalidOffLen);
            }

            try
            {
                Array.Copy(_queue._nodes, 0, array, index, _queue._size);
            }
            catch (ArrayTypeMismatchException)
            {
                throw new ArgumentException(errorStrings.Argument_IncompatibleArrayType, "array");
            }
        }

        public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>
        {
            private readonly PriorityQueue<TElement, TPriority> _queue;
            private readonly int _version;
            private int _index;
            private (TElement, TPriority) _current;

            internal Enumerator(PriorityQueue<TElement, TPriority> queue)
            {
                _queue = queue;
                _index = 0;
                _version = queue._version;
                _current = default((TElement, TPriority));
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                PriorityQueue<TElement, TPriority> localQueue = _queue;

                if (_version != localQueue._version)
                {
                    ThrowHelper.ThrowVersionCheckFailed();
                }

                if ((uint)_index < (uint)localQueue._size)
                {
                    _current = localQueue._nodes[_index];
                    _index++;
                    return true;
                }

                _current = default((TElement, TPriority));
                _index = -1;
                return false;
            }

            public (TElement Element, TPriority Priority) Current
            {
                get { return _current; }
            }

            object IEnumerator.Current
            {
                get { return _current; }
            }

            void IEnumerator.Reset()
            {
                if (_version != _queue._version)
                {
                    ThrowHelper.ThrowVersionCheckFailed();
                }

                _index = 0;
                _current = default((TElement, TPriority));
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_queue);
        }

        IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator()
        {
            if (_queue.Count == 0)
            {
                return EnumerableHelpers.GetEmptyEnumerator<(TElement Element, TPriority Priority)>();
            }

            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<(TElement Element, TPriority Priority)>)this).GetEnumerator();
        }
    }
}

internal static class ThrowHelper
{
    internal static void ThrowVersionCheckFailed()
    {
        throw new InvalidOperationException(errorStrings.InvalidOperation_EnumFailedVersion);
    }
}

internal static class EnumerableHelpers
{
    internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        ICollection<T> collection = source as ICollection<T>;
        if (collection != null)
        {
            int count = collection.Count;
            if (count == 0)
            {
                length = 0;
                return new T[0];
            }

            T[] result = new T[count];
            collection.CopyTo(result, 0);
            length = count;
            return result;
        }

        List<T> list = new List<T>(source);
        length = list.Count;
        return list.ToArray();
    }

    internal static IEnumerator<T> GetEmptyEnumerator<T>()
    {
        return EmptyEnumerator<T>.Instance;
    }

    private sealed class EmptyEnumerator<T> : IEnumerator<T>
    {
        internal static readonly EmptyEnumerator<T> Instance = new EmptyEnumerator<T>();

        public T Current
        {
            get { return default(T); }
        }

        object IEnumerator.Current
        {
            get { return default(T); }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }
    }
}

internal static class errorStrings
{
    internal const string InvalidOperation_EmptyQueue = "The priority queue is empty.";
    internal const string InvalidOperation_EnumFailedVersion = "Collection was modified; enumeration operation may not execute.";
    internal const string Arg_RankMultiDimNotSupported = "Only single dimensional arrays are supported for the requested action.";
    internal const string Arg_NonZeroLowerBound = "The lower bound of target array must be zero.";
    internal const string ArgumentOutOfRange_IndexMustBeLessOrEqual = "Index was out of range. Must be non-negative and less than or equal to the size of the collection.";
    internal const string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
    internal const string Argument_IncompatibleArrayType = "Target array type is not compatible with the type of items in the collection.";
}