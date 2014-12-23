using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LibGit2Sharp.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitSubmoduleOptions
    {
        public uint Version;

        public GitCheckoutOpts CheckoutOptions;

        public GitRemoteCallbacks RemoteCallbacks;

        public CheckoutStrategy CloneCheckoutStrategy;

        public IntPtr Signature;
    }
}
