﻿using System.Threading;

// ReSharper disable once CheckNamespace
namespace Ascentis.Infrastructure
{
    public class QueuedBagNodeSlim<T> : BaseNodeSlim<T, QueuedBagNodeSlim<T>>
    {
        internal volatile bool Ground;

        internal QueuedBagNodeSlim(T value) : base(value)
        {
            Ground = false;
        }

        internal QueuedBagNodeSlim() : base(default)
        {
            Ground = true; // Default to grounded nodes
        }

        internal void EnsureUngrounded()
        {
            if (!Ground)
                return;
            SpinWait? spinner = null;
            while (Ground)
            {
                spinner ??= new SpinWait();
                // ReSharper disable once ConstantConditionalAccessQualifier
                spinner?.SpinOnce();
            }
        }

        internal T GetUngroundedValue()
        {
            EnsureUngrounded();
            return Value;
        }

        internal override BaseNodeSlim<T> GetNext()
        {
            if (Next?.Next != null)
                Next.EnsureUngrounded();
            return (!Next?.Ground ?? false) ? Next : null;
        }
    }
}