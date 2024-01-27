using Gib.Core;

namespace Gib.Console
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var pipeline = new GibPipeline();
            var e1 = new SendElement();
            pipeline.Add(e1);
            var e2 = new SinkElement();
            pipeline.Add(e2);
            pipeline.TrySetTargetState(GibElementState.Ready);
            pipeline.TrySetTargetState(GibElementState.Running);
        }

    }

}
