using HollowSpirit.Core.Input;
using NUnit.Framework;

namespace HollowSpirit.Tests.EditMode
{
    public class InputBufferTests
    {
        private const float Window = 0.2f;

        [Test]
        public void Consumes_InsideWindow()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Attack, 1f);
            Assert.IsTrue(buffer.TryConsume(BufferedAction.Attack, 1.1f));
        }

        [Test]
        public void Consumes_AtWindowEdge()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Attack, 0f);
            Assert.IsTrue(buffer.TryConsume(BufferedAction.Attack, Window));
        }

        [Test]
        public void Rejects_AfterWindow()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Attack, 1f);
            Assert.IsFalse(buffer.TryConsume(BufferedAction.Attack, 1.5f));
        }

        [Test]
        public void Rejects_DifferentAction()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Attack, 1f);
            Assert.IsFalse(buffer.TryConsume(BufferedAction.Roll, 1.1f));
        }

        [Test]
        public void Consumes_OnlyOnce()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Roll, 1f);
            Assert.IsTrue(buffer.TryConsume(BufferedAction.Roll, 1.05f));
            Assert.IsFalse(buffer.TryConsume(BufferedAction.Roll, 1.06f));
        }

        [Test]
        public void NewerPress_ReplacesOlder()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Attack, 1f);
            buffer.Press(BufferedAction.Roll, 1.1f);
            Assert.IsFalse(buffer.TryConsume(BufferedAction.Attack, 1.15f));
            Assert.IsTrue(buffer.TryConsume(BufferedAction.Roll, 1.15f));
        }

        [Test]
        public void Clear_DropsBufferedAction()
        {
            var buffer = new InputBuffer(Window);
            buffer.Press(BufferedAction.Heal, 1f);
            buffer.Clear();
            Assert.IsFalse(buffer.TryConsume(BufferedAction.Heal, 1.05f));
        }
    }
}
