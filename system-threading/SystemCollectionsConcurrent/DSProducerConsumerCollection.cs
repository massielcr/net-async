using System.Collections;
using System.Collections.Concurrent;

namespace SystemCollectionsConcurrent
{
    internal static class DSProducerConsumerCollection
    {
        public static async Task Run()
        {
            Console.WriteLine($"1. Create empty DSProducerConsumerStack<int> instance");
            DSProducerConsumerStack<int> consummerProducerStack = new();

            Console.WriteLine($"2. Get IProducerConsumerCollection<int> from the DSProducerConsumerStack<int> instance");
            IProducerConsumerCollection<int> iconsummerProducerStack = consummerProducerStack;

            Console.WriteLine($"3. Push and TryAdd items into the DSProducerConsumerStack<int> and IProducerConsumerCollection<int>");
            consummerProducerStack.Push(10); Console.WriteLine("- Pushed 10 into DSProducerConsumerStack<int>");
            iconsummerProducerStack.TryAdd(20); Console.WriteLine("- TryAdded 20 into IProducerConsumerCollection<int>");
            consummerProducerStack.Push(15); Console.WriteLine("- Pushed 15 into DSProducerConsumerStack<int>");

            int[] testArray = new int[3];

            Console.WriteLine($"4. CopyTo() within boundaries from IProducerConsumerCollection<int>");
            try
            {
                iconsummerProducerStack.CopyTo(testArray, 0);
                Console.WriteLine("CopyTo() within boundaries worked, as expected");
            }
            catch (Exception e)
            {
                Console.WriteLine("CopyTo() within boundaries unexpectedly threw an exception: {0}", e.Message);
            }

            Console.WriteLine($"5. CopyTo() that overflows from IProducerConsumerCollection<int>");
            try
            {
                iconsummerProducerStack.CopyTo(testArray, 1);
                Console.WriteLine("CopyTo() with index overflow worked, and it SHOULD NOT HAVE");
            }
            catch (Exception e)
            {
                Console.WriteLine("CopyTo() with index overflow threw an exception, as expected: {0}", e.Message);
            }

            Console.WriteLine($"6. Enumeration on IProducerConsumerCollection<int>");
            Console.Write("Enumeration (should be three items): ");
            foreach (int item in iconsummerProducerStack) Console.Write("{0} ", item);
            Console.WriteLine("");

            // Test TryPop()
            Console.WriteLine($"7. TryPop() from IProducerConsumerCollection<int>");
            int popped = 0;
            if (consummerProducerStack.TryPop(out popped))
            {
                Console.WriteLine("Successfully popped {0}", popped);
            }
            else
            {
                Console.WriteLine("FAILED to pop!!");
            }

            Console.WriteLine($"8. Count from IProducerConsumerCollection<int>");
            Console.WriteLine("stack count is {0}, should be 2", consummerProducerStack.Count);

            Console.WriteLine($"8. TryTake() from IProducerConsumerCollection<int>");
            if (iconsummerProducerStack.TryTake(out popped))
            {
                Console.WriteLine("Successfully IPCC-TryTaked {0}", popped);
            }
            else
            {
                Console.WriteLine("FAILED to IPCC.TryTake!!");
            }
        }        
    }

    internal class DSProducerConsumerStack<T> : IProducerConsumerCollection<T>
    {
        private object _lockObject = new();
        private Stack<T> _sequentialStack = new();

        public int Count { get { return _sequentialStack!.Count; } }
        public bool IsSynchronized { get { return true; } }
        public object SyncRoot { get { return _lockObject; } }
        


        public DSProducerConsumerStack()
        {
            _sequentialStack = new();
        }

        public DSProducerConsumerStack(IEnumerable<T> collection)
        {
            _sequentialStack = new(collection);
        }

        //
        // Safe Push/Pop support
        //
        public void Push(T item)
        {
            lock (_lockObject)
            {
                _sequentialStack.Push(item);
            }
        }

        public bool TryPop(out T item)
        {
            item = default!;

            lock (_lockObject)
            {
                if (_sequentialStack.Count != 0)
                {
                    item = _sequentialStack.Pop();
                    return true;
                }                
            }

            return true;
        }


        //
        // IProducerConsumerCollection(T) support
        //
        public bool TryAdd(T item)
        {
            Push(item);
            return true;
        }

        public bool TryTake(out T item)
        {
            return TryPop(out item);
        }

        public T[] ToArray()
        {
            T[] result = [];

            lock (_lockObject)
            {
                result = [.. _sequentialStack];
            }

            return result;
        }

        public void CopyTo(T[] array, int index)
        {
            lock (_lockObject)
            {
                _sequentialStack.CopyTo(array, index);
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (_lockObject)
            {
                ((ICollection)_sequentialStack).CopyTo(array, index);
            }
        }

        public  IEnumerator<T> GetEnumerator()
        {
            Stack<T> stackCopy = new();

            lock (_lockObject)
            {
                stackCopy = new Stack<T>(_sequentialStack);
            }

            return stackCopy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }
    }
}
