﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSFree4All.Enums
{
    /// <summary>
    /// The different expanders available for the
    /// Expander UserControl
    /// </summary>
    public enum ExpanderStyles
    {
        /// <summary>
        /// The default expander style
        /// </summary>
        Default,

        /// <summary>
        /// Static style, doesn't expand
        /// </summary>
        Static,

        /// <summary>
        /// Button style, supports click event
        /// handlers
        /// </summary>
        Button,

        /// <summary>
        /// Transparent, same as static but without
        /// a background or border
        /// </summary>
        Transparent,

        /// <summary>
        /// Disabled style, uses secondary colors,
        /// does not show content
        /// </summary>
        Disabled
    }
}
