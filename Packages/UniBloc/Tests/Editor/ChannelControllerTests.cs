using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NUnit.Framework;
using UniBloc;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class ChannelControllerTests
    {
        [UnityTest]
        public IEnumerator DebounceTest() => UniTask.ToCoroutine(async () =>
        {
            using var controller = new ChannelController<int>();
            var inValues = new List<int>();
            var outValues = new List<int>();

            using var subscription = controller.Source().TakeWhileAwait(async value =>
            {
                inValues.Add(value);
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                return true;
            }).Subscribe(message => { outValues.Add(message); });

            var cnt = 1;
            var offset = cnt + 3;
            for (; cnt < offset; cnt++) controller.Publish(cnt); // (1),2,3
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            offset = cnt + 3;
            for (; cnt < offset; cnt++) controller.Publish(cnt); // (4),5,6
            await UniTask.Delay(TimeSpan.FromSeconds(1));
            offset = cnt + 1;
            for (; cnt < offset; cnt++) controller.Publish(cnt); // 7
            
            // await UniTask.Delay(TimeSpan.FromSeconds(1));
            Assert.IsTrue(inValues.SequenceEqual(new[]
                {
                    1, 4, 7
                }),
                inValues.ToString<int>());
            Assert.IsTrue(outValues.SequenceEqual(new[]
                {
                    1, 4
                }),
                outValues.ToString<int>());
        });
    }

    public static class EnumerableExtensions
    {
        public static string ToString<T>(this IEnumerable<T> enumerable) =>
            $"[{enumerable.Aggregate(string.Empty, (prev, curt) => string.IsNullOrEmpty(prev) ? $"{curt}" : $"{prev}, {curt}")}]";
    }
}