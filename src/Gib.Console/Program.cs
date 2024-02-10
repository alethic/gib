using Gip.Core;

namespace Gip.Console
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var pipeline = new GipPipeline();
            var e1 = new SendElement();
            pipeline.AddElement(e1);
            var e2 = new SinkElement();
            pipeline.AddElement(e2);
            pipeline.TrySetTargetState(GibState.Ready);
            pipeline.TrySetTargetState(GibState.Running);
        }

    }

}
