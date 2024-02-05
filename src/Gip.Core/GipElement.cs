using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Gip.Core
{

    /// <summary>
    /// A <see cref="GipElement"/> represents the basic building block of a <see cref="GipPipeline"/>.
    /// </summary>
    public abstract class GipElement : GipObject<GipBin>
    {

        readonly GipElementFactory factory;

        GipState targetState;
        GipState state;
        GipState stateNext;

        readonly GipPadList pads = new GipPadList();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="factory"></param>
        protected GipElement(GipElementFactory factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Throws an exception if the current state of the element does not match the specified state.
        /// </summary>
        /// <param name="state"></param>
        /// <exception cref="GipException"></exception>
        internal protected void AssertState(GipState state)
        {
            lock (this)
                if (this.state != state)
                    throw new GipException($"{nameof(GipElement)} not in required state '{state}'.");
        }

        /// <summary>
        /// Gets or sets the current target state of the element.
        /// </summary>
        public GipState TargetState => targetState;

        /// <summary>
        /// Gets the current actual state of this element.
        /// </summary>
        public GipState State => state;

        /// <summary>
        /// Gets the current actual state of this element.
        /// </summary>
        public GipState StateNext => stateNext;

        /// <summary>
        /// Raises the <see cref="StateChangedEvent"/> event.
        /// </summary>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        internal void OnStateChangedEvent(GipState oldState, GipState newState)
        {
            RaiseEvent(new GipElementStateChangedEventArgs(this, oldState, newState));
        }

        /// <summary>
        /// Gets the pads of the element.
        /// </summary>
        public IReadOnlyCollection<GipPad> Pads => pads;

        /// <summary>
        /// Invoked to raise the the <see cref="PadEvent"/> event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="pad"></param>
        protected internal void OnPadEvent(GipElementPadEventType eventType, GipPad pad)
        {
            RaiseEvent(new GipElementPadEventArgs(this, eventType, pad));
        }

        /// <summary>
        /// Invoked by the user to set the desired state.
        /// </summary>
        /// <param name="targetState"></param>
        public bool TrySetTargetState(GipState targetState)
        {
            lock (this)
            {
                // pipeline is the only element that can change state without a bin
                if (this is not GipPipeline && Parent == null)
                    throw new GipException($"Element requires a configured {nameof(GipBin)} parent to change state.");

                // update target state
                this.targetState = targetState;

                // we haven't yet proceeded to the target state, so try to change
                if (state != targetState)
                    return TryChangeStateInternal();
            }

            // we did no change, indicate success
            return true;
        }

        /// <summary>
        /// Invoked when an element should transition to the <see cref="GipState.Running"/> state.
        /// </summary>
        bool TryChangeStateInternal()
        {
            try
            {
                if (TryChangeState(targetState))
                {
                    var oldState = state;
                    state = targetState;

                    try
                    {
                        OnStateChangedEvent(oldState, state);
                    }
                    catch
                    {
                        // TODO log this, but proceed with state change
                    }
                }
            }
            catch
            {
                // TODO log this, but indicate we failed
                return false;
            }

            return true;
        }

        /// <summary>
        ///  Adds a pad (link point) to this element. <paramref name="pad"/>'s parent will be set to this element.
        /// </summary>
        /// <remarks>
        /// Pads are automatically activated when added in the PAUSED or PLAYING state.
        /// </remarks>
        /// <param name="pad"></param>
        protected virtual void AddPad(GipPad pad)
        {
            using var _ = Lock();

            var name = pad.Name;
            var active = pad.Mode != GipPadMode.None;

            // then check to see if there is already a pad by that name on this element
            if (pads.Any(i => i.Name == name))
                throw new GipException($"Padname {name} is not unique in element {Name}.");

            // set parent of pad to ourselves
            pad.Parent = this;

            // check for active pads
            var shouldActivate = !active && (State > GipState.Ready || StateNext == GipState.Paused);

            // add and activate if requird
            pads.Add(pad);
            if (shouldActivate)
                pad.SetActive(true);

            // raise pad event
            OnPadEvent(GipElementPadEventType.Added, pad);
        }

        /// <summary>
        /// Removes <paramref name="pad"/> from this element.
        /// </summary>
        /// <remarks>
        /// Pads are not automatically deactivated so elements should perform the needed
        /// steps to deactivate the pad in case this pad is removed in the Ready or
        /// Running state.
        /// </remarks>
        /// <param name="pad"></param>
        protected virtual void RemovePad(GipPad pad)
        {
            using var _ = Lock();

            if (pad.Parent != this)
                throw new GipException($"Pad {pad.Name} does not belong to element {Name} when removing.");
        }

        /// <summary>
        /// Retrieves a pad from @element by name. This version only retrieves already-existing (i.e. 'static') pads.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual GipPad? GetStaticPad(string name)
        {
            using var _ = Lock();

            foreach (var pad in pads)
                if (pad.Name == name)
                    return pad;

            return null;
        }

        /// <summary>
        /// Requests a pad be allocated given the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected GipSinkPad? RequestPad(string name)
        {
            // find sink template with the same name
            var requestName = default(string);
            var template = factory.GetPadTemplate(name) as GipSinkPadTemplate;
            if (template != null)
            {
                // if name is a template name, we can't use it
                requestName = name.Contains('%') ? null : name;
            }
            else
            {
                // find from template factory
                template = factory.PadTemplates.OfType<GipSinkPadTemplate>().FirstOrDefault(i => i.Presence == GipPadPresence.Dynamic && IsValidRequestPadName(i.Name, name));
                if (template != null)
                    requestName = name;
            }

            // if we found a template, requests a pad using it
            return template != null ? RequestPad(template, requestName, GipCapList.Any) : null;
        }

        /// <summary>
        /// Requests a pad be allocated from the specified template.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <param name="caps"></param>
        /// <returns></returns>
        protected GipSinkPad? RequestPad(GipSinkPadTemplate template, string? name, GipCapList caps)
        {
            // factory doesn't contain template
            if (factory.PadTemplates.Contains(template) == false)
                return null;

            // validate name
            if (name != null)
            {
                // name matches template requirement
                if (IsValidRequestPadName(template.Name, name) == false)
                    return null;

                // pad with same name is already allocated
                var pad = GetStaticPad(name);
                if (pad != null)
                    throw new GipException($"Element already has a pad named {name}.");
            }

            // call element implementation
            return RequestPadCore(template, name, caps);
        }

        /// <summary>
        /// Requests a pad be allocated from the specified template. Implement this method to handle custom dynamic pad template requests.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <param name="caps"></param>
        /// <returns></returns>
        protected virtual GipSinkPad? RequestPadCore(GipSinkPadTemplate template, string? name, GipCapList caps)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns <c>true</c> if the value given by <paramref name="name"/> is a valid name for a pad created by a request template with name given by <paramref name="templateName"/>.
        /// </summary>
        /// <remarks>
        /// This is a copy of C code. It needs to be redone.
        /// </remarks>
        /// <param name="templateName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static unsafe bool IsValidRequestPadName(ReadOnlySpan<char> templateName, ReadOnlySpan<char> name)
        {

            static char* strchr(char* str, int c)
            {
                while (true)
                {
                    if (*str == '\0')
                        return null;
                    else if (*str == (char)c)
                        return str;
                    else
                        str++;
                }
            }

            static int strncmp(char* s, char* t, uint num)
            {
                for (; num > 0; s++, t++, num--)
                    if (*s == 0)
                        return 0;

                if (*s == *t)
                {
                    ++s;
                    ++t;
                }
                else if (*s != *t)
                    return *s - *t;

                return 0;
            }

            static uint strlen(char* str)
            {
                char* s;
                for (s = str; *s != 0; ++s) ;
                return (uint)(s - str);
            }

            static char* strncpy(char* s1, char* s2, uint n)
            {
                char* s = s1;
                while (n > 0 && *s2 != '\0')
                {
                    *s++ = *s2++;
                    --n;
                }
                while (n > 0)
                {
                    *s++ = '\0';
                    --n;
                }
                return s1;
            }

            static char* g_strndup(char* str, uint n)
            {
                char* new_str;

                if (str != null)
                {
                    new_str = (char*)Marshal.AllocHGlobal((int)n + 1);
                    strncpy(new_str, str, n);
                    new_str[(int)n] = '\0';
                }
                else
                    new_str = null;

                return new_str;
            }

            static long strtol(char* nptr, char** endptr, int base_)
            {
                char* p = nptr, endp;
                int is_neg = 0, overflow = 0;
                /* Need unsigned so (-LONG_MIN) can fit in these: */
                ulong n = 0UL, cutoff;
                int cutlim;
                if (base_ < 0 || base_ == 1 || base_ > 36)
                {
                    return 0L;
                }
                endp = nptr;
                while (char.IsWhiteSpace(*p))
                    p++;
                if (*p == '+')
                {
                    p++;
                }
                else if (*p == '-')
                {
                    is_neg = 1;
                    p++;
                }
                if (*p == '0')
                {
                    p++;
                    /* For strtol(" 0xZ", &endptr, 16), endptr should point to 'x';
                     * pointing to ' ' or '0' is non-compliant.
                     * (Many implementations do this wrong.) */
                    endp = p;
                    if (base_ == 16 && (*p == 'X' || *p == 'x'))
                    {
                        p++;
                    }
                    else if (base_ == 2 && (*p == 'B' || *p == 'b'))
                    {
                        /* C23 standard supports "0B" and "0b" prefixes. */
                        p++;
                    }
                    else if (base_ == 0)
                    {
                        if (*p == 'X' || *p == 'x')
                        {
                            base_ = 16;
                            p++;
                        }
                        else if (*p == 'B' || *p == 'b')
                        {
                            base_ = 2;
                            p++;
                        }
                        else
                        {
                            base_ = 8;
                        }
                    }
                }
                else if (base_ == 0)
                {
                    base_ = 10;
                }
                cutoff = (ulong)((is_neg != 0) ? -(long.MinValue / (long)base_) : long.MaxValue / (long)base_);
                cutlim = (int)((is_neg != 0) ? -(long.MinValue % (long)base_) : long.MaxValue % (long)base_);
                while (true)
                {
                    int c;
                    if (*p >= 'A')
                        c = ((*p - 'A') & (~('a' ^ 'A'))) + 10;
                    else if (*p <= '9')
                        c = *p - '0';
                    else
                        break;
                    if (c < 0 || c >= base_) break;
                    endp = ++p;
                    if (overflow != 0)
                    {
                        /* endptr should go forward and point to the non-digit character
                         * (of the given base); required by ANSI standard. */
                        if (endptr != null) continue;
                        break;
                    }
                    if (n > cutoff || (n == cutoff && c > cutlim))
                    {
                        overflow = 1; continue;
                    }
                    n = n * (ulong)base_ + (ulong)c;
                }
                if (endptr != null) *endptr = (char*)endp;
                if (overflow != 0)
                {
                    return ((is_neg != 0) ? long.MinValue : long.MaxValue);
                }

                return (is_neg != 0) ? -(long)n : (long)n;
            }

            if (templateName == null)
                return false;
            if (name == null)
                return false;

            // is this the template name?
            if (templateName.Equals(name, StringComparison.Ordinal))
                return true;

            // preserve existing logic by allocating null terminated copies on the stack
            var templ_name = stackalloc char[templateName.Length + 1];
            templateName.CopyTo(new Span<char>(templ_name, templateName.Length + 1));
            var name_ = stackalloc char[name.Length + 1];
            name.CopyTo(new Span<char>(name_, name.Length + 1));

            char* endptr;
            char* templ_name_ptr, name_ptr;
            bool next_specifier;
            int templ_postfix_len = 0, name_postfix_len = 0;

            // otherwise check all the specifiers
            do
            {
                /* Because of sanity checks in gst_pad_template_new(), we know that %s
                 * and %d and %u, occurring at the template_name */
                templ_name_ptr = strchr(templ_name, '%');

                /* check characters ahead of the specifier */
                if (templ_name_ptr == null || name.Length <= templ_name_ptr - templ_name || strncmp(templ_name, name_, (uint)((IntPtr)templ_name_ptr - (IntPtr)templ_name)) != 0)
                {
                    return false;
                }

                /* %s is not allowed for multiple specifiers, just a single specifier can be
                 * accepted in gst_pad_template_new() and can not be mixed with other
                 * specifier '%u' and '%d' */
                if (*(templ_name_ptr + 1) == 's')
                {
                    return true;
                }

                name_ptr = name_ + (templ_name_ptr - templ_name);

                /* search next specifier, each of specifier should be separated by '_' */
                templ_name = strchr(templ_name_ptr, '_');
                name_ = strchr(name_ptr, '_');

                /* don't match the number of specifiers */
                if ((templ_name != null && name_ == null) || (templ_name == null && name_ != null))
                    return false;

                if (templ_name != null && name_ != null)
                    next_specifier = true;
                else
                    next_specifier = false;

                /* check characters followed by the specifier */
                if (*(templ_name_ptr + 2) != '\0' && *(templ_name_ptr + 2) != '_')
                {
                    if (next_specifier)
                    {
                        templ_postfix_len = (int)(templ_name - (templ_name_ptr + 2));
                        name_postfix_len = (int)(name_ - name_ptr);
                    }
                    else
                    {
                        templ_postfix_len = (int)strlen(templ_name_ptr + 2);
                        name_postfix_len = (int)strlen(name_ptr);
                    }

                    if (strncmp(templ_name_ptr + 2, name_ptr + name_postfix_len - templ_postfix_len, (uint)templ_postfix_len) != 0)
                    {
                        return false;
                    }
                }

                /* verify the specifier */
                if (*(name_ptr) == '%')
                {
                    uint len;

                    len = (next_specifier) ? (uint)(name_ - name_ptr) : strlen(name_ptr);

                    if (strncmp(name_ptr, templ_name_ptr, len) != 0)
                        return false;

                }
                else
                {
                    char* specifier;
                    char* target = null;

                    /* extract specifier when it has postfix characters */
                    if (name_postfix_len > templ_postfix_len)
                    {
                        target = g_strndup(name_ptr, (uint)(name_postfix_len - templ_postfix_len));
                    }

                    specifier = target != null ? target : name_ptr;

                    if (*(templ_name_ptr + 1) == 'd')
                    {
                        long tmp;

                        /* it's an int */
                        tmp = strtol(specifier, &endptr, 10);
                        if (tmp < int.MinValue || tmp > int.MaxValue || (*endptr != '\0' && *endptr != '_'))
                            return false;
                    }
                    else if (*(templ_name_ptr + 1) == 'u')
                    {
                        long tmp;

                        /* it's an int */
                        tmp = strtol(specifier, &endptr, 10);
                        if (tmp > uint.MaxValue || (*endptr != '\0' && *endptr != '_'))
                            return false;
                    }

                    Marshal.FreeHGlobal((nint)target);
                }

                /* otherwise we increment these from NULL to 1 */
                if (next_specifier)
                {
                    templ_name++;
                    name_++;
                }
            } while (next_specifier);

            return true;
        }

        /// <summary>
        /// Releases the specified pad.
        /// </summary>
        /// <param name="pad"></param>
        protected void ReleasePad(GipSinkPad pad)
        {
            if (pad.Template == null)
                return;
            if (pad.Template.Presence != GipPadPresence.Dynamic)
                return;
            if (pad.Parent != this)
                return;

            ReleasePadCore(pad);
        }

        /// <summary>
        /// Releases the specified pad. Implement this method to implement custom releasing of pad requests.
        /// </summary>
        /// <param name="pad"></param>
        protected virtual void ReleasePadCore(GipSinkPad pad)
        {
            RemovePad(pad);
        }

        /// <summary>
        /// Invoked when an element should transition to the specified state.
        /// </summary>
        protected abstract bool TryChangeState(GipState state);

    }

}
