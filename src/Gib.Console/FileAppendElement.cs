using Gip.Core;

namespace Gip.Console
{

    public class FileAppendElementFactory : GibElementFactory
    {

        /// <summary>
        /// Template for pads that receive input files.
        /// </summary>
        public static readonly GibSinkPadTemplate SrcTemplate = new GibSinkPadTemplate("src_%d", GibPadPresence.Dynamic, Array.Empty<GibCap>());

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FileAppendElementFactory()
        {
            AddPadTemplate(SrcTemplate);
        }

        /// <inheritdoc />
        public override GibElement Create()
        {
            return new FileAppendElement(this);
        }

    }

    /// <summary>
    /// Generates a single file one the output that consists of the appended contents of the files on the input.
    /// </summary>
    public class FileAppendElement : GibElement
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FileAppendElement(GibElementFactory factory) :
            base(factory)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <param name="caps"></param>
        /// <returns></returns>
        internal override GibSinkPad RequestPad(GibSinkPadTemplate template, string name, GibCap[] caps)
        {
            var pad = template.Create(this, name);
            pad.UserState = new FileAppendInputContext();
            AddPad(pad);
            return pad;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pad"></param>
        public override void ReleaseSinkPad(GibSinkPad pad)
        {
            pad.UserState = null;
            RemovePad(pad);
        }

        protected override bool TryChangeState(GibState state)
        {

        }

    }

    class FileAppendInputContext
    {



    }

}
