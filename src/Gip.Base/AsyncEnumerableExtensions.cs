using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Gip.Base
{

    public static class AsyncEnumerableExtensions
    {

        static async Task ForEach<T>(IAsyncEnumerable<T> l, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellationToken)
        {
            await foreach (var item in l.WithCancellation(cancellationToken))
                await action(item, cancellationToken);
        }

        /// <summary>
        /// Merges the streams, keeping the state of each value at each step.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(T1, T2)> Latest<T1, T2>(
            IAsyncEnumerable<T1> c1,
            IAsyncEnumerable<T2> c2,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var chan = Channel.CreateBounded<(int i, T1? v1, T2? v2)>(1);
            var t1 = ForEach(c1, (i, ct) => chan.Writer.WriteAsync((1, i, default), ct), cancellationToken);
            var t2 = ForEach(c2, (i, ct) => chan.Writer.WriteAsync((2, default, i), ct), cancellationToken);

            Optional<T1> p1 = default;
            Optional<T2> p2 = default;

            await foreach (var (i, v1, v2) in chan.Reader.ReadAllAsync(cancellationToken))
            {
                switch (i)
                {
                    case 1:
                        p1 = new(v1!);
                        break;
                    case 2:
                        p2 = new(v2!);
                        break;
                }

                if (p1.HasValue && p2.HasValue)
                    yield return (p1.Value, p2.Value);
            }

            await Task.WhenAll([t1, t2]);
        }

        /// <summary>
        /// Merges the streams, keeping the state of each value at each step.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(T1, T2, T3)> Latest<T1, T2, T3>(
            IAsyncEnumerable<T1> c1,
            IAsyncEnumerable<T2> c2,
            IAsyncEnumerable<T3> c3,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var chan = Channel.CreateBounded<(int i, T1? v1, T2? v2, T3? v3)>(1);
            var t1 = ForEach(c1, (i, ct) => chan.Writer.WriteAsync((1, i, default, default), ct), cancellationToken);
            var t2 = ForEach(c2, (i, ct) => chan.Writer.WriteAsync((2, default, i, default), ct), cancellationToken);
            var t3 = ForEach(c3, (i, ct) => chan.Writer.WriteAsync((3, default, default, i), ct), cancellationToken);

            Optional<T1> p1 = default;
            Optional<T2> p2 = default;
            Optional<T3> p3 = default;

            await foreach (var (i, v1, v2, v3) in chan.Reader.ReadAllAsync(cancellationToken))
            {
                switch (i)
                {
                    case 1:
                        p1 = new(v1!);
                        break;
                    case 2:
                        p2 = new(v2!);
                        break;
                    case 3:
                        p3 = new(v3!);
                        break;
                }

                if (p1.HasValue &&
                    p2.HasValue &&
                    p3.HasValue)
                    yield return (p1.Value, p2.Value, p3.Value);
            }

            await Task.WhenAll([t1, t2, t3]);
        }

        /// <summary>
        /// Merges the streams, keeping the state of each value at each step.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<(T1, T2, T3, T4)> Latest<T1, T2, T3, T4>(
            IAsyncEnumerable<T1> c1,
            IAsyncEnumerable<T2> c2,
            IAsyncEnumerable<T3> c3,
            IAsyncEnumerable<T4> c4,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var chan = Channel.CreateBounded<(int i, T1? v1, T2? v2, T3? v3, T4? v4)>(1);
            var t1 = ForEach(c1, (i, ct) => chan.Writer.WriteAsync((1, i, default, default, default), ct), cancellationToken);
            var t2 = ForEach(c2, (i, ct) => chan.Writer.WriteAsync((2, default, i, default, default), ct), cancellationToken);
            var t3 = ForEach(c3, (i, ct) => chan.Writer.WriteAsync((3, default, default, i, default), ct), cancellationToken);
            var t4 = ForEach(c4, (i, ct) => chan.Writer.WriteAsync((4, default, default, default, i), ct), cancellationToken);

            Optional<T1> p1 = default;
            Optional<T2> p2 = default;
            Optional<T3> p3 = default;
            Optional<T4> p4 = default;

            await foreach (var (i, v1, v2, v3, v4) in chan.Reader.ReadAllAsync(cancellationToken))
            {
                switch (i)
                {
                    case 1:
                        p1 = new(v1!);
                        break;
                    case 2:
                        p2 = new(v2!);
                        break;
                    case 3:
                        p3 = new(v3!);
                        break;
                    case 4:
                        p4 = new(v4!);
                        break;
                }

                if (p1.HasValue &&
                    p2.HasValue &&
                    p3.HasValue &&
                    p4.HasValue)
                    yield return (p1.Value, p2.Value, p3.Value, p4.Value);
            }

            await Task.WhenAll([t1, t2, t3, t4]);
        }

    }

}
