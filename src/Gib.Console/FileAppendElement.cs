using Gip.Core;

namespace Gip.Console
{

    public class FileAppendElementFactory : GipElementFactory
    {

        /// <summary>
        /// Template for pads that receive input files.
        /// </summary>
        public static readonly GipSinkPadTemplate SrcTemplate = new GipSinkPadTemplate("src_%d", GipPadPresence.Dynamic, Array.Empty<GipCap>());

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FileAppendElementFactory()
        {
            AddPadTemplate(SrcTemplate);
        }

        /// <inheritdoc />
        public override GipElement Create()
        {
            return new FileAppendElement(this);
        }

    }

    /// <summary>
    /// Generates a single file one the output that consists of the appended contents of the files on the input.
    /// </summary>
    public class FileAppendElement : GipElement
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public FileAppendElement(GipElementFactory factory) :
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
        internal override GipSinkPad RequestPadCore(GipSinkPadTemplate template, string name, GipCapList caps)
        {
            var pad = template.Create();
            pad.Name = name;
            pad.UserState = new FileAppendInputContext();
            AddPad(pad);
            return pad;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pad"></param>
        public override void ReleaseSinkPad(GipSinkPad pad)
        {
            pad.UserState = null;
            RemovePad(pad);
        }

        protected override bool TryChangeState(GipState state)
        {

        }

    }

    class FileAppendInputContext
    {



    }

}
