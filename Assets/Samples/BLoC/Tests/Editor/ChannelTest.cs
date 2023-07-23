using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UniBloc;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Samples.BLoC.Tests.Editor
{
    public class ChannelTest
    {
        public enum ChannelEvent
        {
            Done,
            OnData,
            Close
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityTest]
        public IEnumerator CompletePasses() => UniTask.ToCoroutine(async () =>
        {
            var events = new List<(ChannelEvent e, object data)>();

            var controller = new ChannelController<int>();
            controller.Done.ContinueWith(() => { events.Add((ChannelEvent.Done, null)); });
            var subscription = controller.Source().Subscribe(_ => { events.Add((ChannelEvent.OnData, _)); });
            controller.Publish(10);
            await UniTask.DelayFrame(1);
            subscription.Dispose();
            controller.Publish(11);
            controller.Dispose();
            events.Add((ChannelEvent.Close, null));

            Assert.IsTrue(events.SequenceEqual(new List<(ChannelEvent e, object data)>()
            {
                (ChannelEvent.OnData, 10),
                (ChannelEvent.Done, null),
                (ChannelEvent.Close, null)
            }));
        });
    }
}