using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.TrueType
{
	/// <summary>
	/// A linked stack implementation.
	/// This class is thread-safe.
	/// </summary>
	public class LinkedStack<T>
	{
		/// <summary>
		/// This class represents a single item
		/// on the Stack.
		/// </summary>
		private class StackItem<T2>
		{
			/// <summary>
			/// The next object on the
			/// stack.
			/// </summary>
			public StackItem<T2> NextObject;
			/// <summary>
			/// The actual value in this stack item.
			/// </summary>
			public T2 Value;
		}

		/// <summary>
		/// The top item in the stack.
		/// This is the only item we
		/// ever have a direct handle
		/// on at any given time.
		/// </summary>
		private StackItem<T> TopItem;
		/// <summary>
		/// The depth of the stack.
		/// This is mostly used for
		/// error checking, but is
		/// provided as a public
		/// property as well.
		/// </summary>
		private int depth;

		/// <summary>
		/// Creates a new instance of the 
		/// LinkedStack class.
		/// </summary>
		public LinkedStack() { }

		/// <summary>
		/// The number of items
		/// in the stack.
		/// </summary>
		public int Depth { get { return depth; } }

		/// <summary>
		/// Copies the value at the specified
		/// index in the stack, to the top of
		/// the stack.
		/// </summary>
		/// <param name="index">The index to copy from.</param>
		public void CopyToTop(int index)
		{
			index--;
			if (index >= depth)
				throw new Exception("Index is invalid! It must be less than the depth of the stack!");
			StackItem<T> curItm = TopItem;
			for (uint i = 0; i < index; i++)
			{
				curItm = curItm.NextObject;
			}
			Push(curItm.Value);
		}

		/// <summary>
		/// Duplicates the entire stack
		/// state.
		/// </summary>
		/// <returns>An exact duplicate of this stack.</returns>
		public LinkedStack<T> Duplicate()
		{
			LinkedStack<T> s1 = new LinkedStack<T>();
			StackItem<T> tItm = TopItem;
			while (tItm != null)
			{
				s1.Push(tItm.Value);
				tItm = tItm.NextObject;
			}
			LinkedStack<T> n = new LinkedStack<T>();
			while (s1.depth > 0)
			{
				n.Push(s1.Pop());
			}
			return n;
		}

		/// <summary>
		/// Pushes the specified value
		/// to the top of the stack.
		/// </summary>
		/// <param name="value">The value to add.</param>
		public void Push(T value)
		{
			lock (this)
			{
				if (TopItem == null)
				{
					// This is the first
					// item to be added to the stack.
					StackItem<T> val = new StackItem<T>();
					val.Value = value;
					TopItem = val;
				}
				else
				{
					StackItem<T> val = new StackItem<T>();
					val.Value = value;
					val.NextObject = TopItem;
					this.TopItem = val;
				}
				this.depth++;
                //Console.WriteLine("Pushing " + value.ToString() + " to the stack.");
			}
		}

		/// <summary>
		/// Pop the first item from the top
		/// of the stack.
		/// </summary>
		/// <returns>
		/// The first value in the queue.
		/// </returns>
		public T Pop()
		{
			lock (this)
			{
				if (TopItem == null)
					throw new Exception("No item on the stack!");
				this.depth--;
				StackItem<T> val = TopItem;
				if (val.NextObject == null)
				{
					TopItem = null;
				}
				else
				{
					TopItem = TopItem.NextObject;
                }
                //Console.WriteLine("Popping " + val.Value.ToString() + " from the stack.");
				return val.Value;
			}
		}

		/// <summary>
		/// Removes all items
		/// from the stack.
		/// </summary>
		public void Clear()
		{
			while (this.depth > 0)
			{
				this.Pop();
			}
		}
	}
}
