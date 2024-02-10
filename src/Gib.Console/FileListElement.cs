using Gip.Core;

namespace Gip.Console
{

    public class FileListElementFactory : GipElementFactory
    {

        public static readonly GipCapList FilePathCaps = new GipCapList(new[] { new GipCap(typeof(string), GipConstraintList.Empty, GipCapFeatureList.Empty) });

        public static readonly GipSinkPadTemplate PathTemplate = new GipSinkPadTemplate("path", GipPadPresence.Always, FilePathCaps);

        public static readonly GipSendPadTemplate FileTemplate = new GipSendPadTemplate("file_%d", GipPadPresence.Dynamic, FilePathCaps);

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FileListElementFactory()
        {
            AddPadTemplate(PathTemplate);
            AddPadTemplate(FileTemplate);
        }

        public override GipElement Create()
        {
            return new FileListElement(this);
        }

    }

    public class FileListElement : GipElement
    {

        readonly GipSinkPad pathPad;
        readonly Dictionary<string, GipSendPad> filePads = new Dictionary<string, GipSendPad>();

        string? path;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="factory"></param>
        public FileListElement(GipElementFactory factory) :
            base(factory)
        {
            pathPad = FileListElementFactory.PathTemplate.Create();
            pathPad.RecvFunc = PathRecvFunc;
            AddPad(pathPad);
        }

        /// <summary>
        /// Invoked when new data is received over the path pad.
        /// </summary>
        /// <param name="sinkPad"></param>
        /// <param name="sendPad"></param>
        /// <param name="element"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        GipPadRecvResult PathRecvFunc(GipSinkPad sinkPad, GipSendPad sendPad, GipElement element, object? data)
        {
            var oldPath = path;
            path = data as string;
            if (path != oldPath)
                Update();

            return GipPadRecvResult.Ok;
        }

        /// <summary>
        /// Update 
        /// </summary>
        void Update()
        {
            var l = string.IsNullOrWhiteSpace(path) ? Array.Empty<string>() : Directory.GetFiles(path);

        }

        protected override bool TryChangeState(GipState state)
        {
            return true;
        }

    }

}
