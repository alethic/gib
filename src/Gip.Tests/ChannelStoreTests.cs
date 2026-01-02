using System;
using System.Linq;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Hosting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nito.AsyncEx;

namespace Gip.Tests
{

    [TestClass]
    public class ChannelStoreTests
    {

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task CanEnumerateSingleBlockComplete()
        {
            var h = HashCode.Combine(1, 2, 3, 4);
            var c = new InMemoryChannelStore();
            c.Store(1);
            c.Store(2);
            c.Store(3);
            c.Store(4);
            c.Complete();

            var s = new HashCode();
            await foreach (var i in c.OpenAsync<int>(TestContext.CancellationToken))
                s.Add(i);

            Assert.AreEqual(h, s.ToHashCode());
        }

        [TestMethod]
        public async Task CanEnumerateManyItems()
        {
            var l = Enumerable.Range(0, 100).ToArray();

            var h = new HashCode();
            foreach (var i in l)
                h.Add(i);

            var c = new InMemoryChannelStore();

            foreach (var i in l)
                c.Store(i);

            c.Complete();

            var s = new HashCode();
            await foreach (var i in c.OpenAsync<int>(TestContext.CancellationToken))
                s.Add(i);

            Assert.AreEqual(h.ToHashCode(), s.ToHashCode());
        }

        [TestMethod]
        public async Task CanEnumerateSingleBlockCompleteAsync()
        {
            var c = new InMemoryChannelStore();
            var h1 = new AsyncManualResetEvent();
            var h2 = new AsyncManualResetEvent();
            var i = 0;
            var t = Task.Run(async () =>
            {
                var l = c.OpenAsync<int>(TestContext.CancellationToken);
                var e = l.GetAsyncEnumerator(TestContext.CancellationToken);

                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();

                Assert.IsTrue(await e.MoveNextAsync());
                i = e.Current;
                h1.Set();
                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();

                Assert.IsTrue(await e.MoveNextAsync());
                i = e.Current;
                h1.Set();
                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();

                Assert.IsTrue(await e.MoveNextAsync());
                i = e.Current;
                h1.Set();
                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();

                Assert.IsTrue(await e.MoveNextAsync());
                i = e.Current;
                h1.Set();
                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();

                Assert.IsFalse(await e.MoveNextAsync());
                i = 0;
                h1.Set();
                Console.WriteLine("Waiting h2");
                await h2.WaitAsync();
                h2.Reset();
            });

            c.Store(1);
            h2.Set();
            Console.WriteLine("Waiting h1");
            await h1.WaitAsync();
            h1.Reset();
            Assert.AreEqual(1, i);

            c.Store(2);
            h2.Set();
            Console.WriteLine("Waiting h1");
            await h1.WaitAsync();
            h1.Reset();
            Assert.AreEqual(2, i);

            c.Store(3);
            h2.Set();
            Console.WriteLine("Waiting h1");
            await h1.WaitAsync();
            h1.Reset();
            Assert.AreEqual(3, i);

            c.Store(4);
            h2.Set();
            Console.WriteLine("Waiting h1");
            await h1.WaitAsync();
            h1.Reset();
            Assert.AreEqual(4, i);

            c.Complete();
            h2.Set();
            Console.WriteLine("Waiting h1");
            await h1.WaitAsync();
            h1.Reset();
            Assert.AreEqual(0, i);

            await t;
        }

    }

}
