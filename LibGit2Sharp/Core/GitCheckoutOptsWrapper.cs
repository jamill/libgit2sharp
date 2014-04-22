using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibGit2Sharp.Core
{
    /// <summary>
    /// A wrapper around the native GitCheckoutOpts structure. This class is responsible
    /// for the managed objects that the native code points to.
    /// </summary>
    internal class GitCheckoutOptsWrapper : IDisposable
    {
        /// <summary>
        /// Create wrapper around <see cref="GitCheckoutOpts"/> from <see cref="CheckoutOptions"/>.
        /// </summary>
        /// <param name="options">Checkout options to create native GitCheckoutOpts structure from.</param>
        /// <param name="checkoutStrategy">Default strategy to use based on the operation.
        /// This usually depends on the operation performing the checkout (e.g. Checkout, Merge, Clone).</param>
        /// <param name="paths">Paths to checkout.</param>
        public GitCheckoutOptsWrapper(CheckoutOptions options, CheckoutStrategy checkoutStrategy, FilePath[] paths = null)
        {
            Callbacks = options.GenerateCallbacks();

            if (paths != null)
            {
                PathArray = GitStrArrayIn.BuildFrom(paths);
            }

            Options = new GitCheckoutOpts
            {
                version = 1,
                checkout_strategy = checkoutStrategy,
                progress_cb = Callbacks.CheckoutProgressCallback,
                notify_cb = Callbacks.CheckoutNotifyCallback,
                notify_flags = options.CheckoutNotifyFlags,
                paths = PathArray,
            };
        }

        /// <summary>
        /// Native struct to pass to libgit.
        /// </summary>
        public GitCheckoutOpts Options { get; set; }

        /// <summary>
        /// I thought this would be needed so the Callbacks do not get garbage collected...
        /// </summary>
        public CheckoutCallbacks Callbacks { get; private set; }

        /// <summary>
        /// Keep the paths around so we can dispose them.
        /// </summary>
        private GitStrArrayIn PathArray;

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (PathArray != null)
                {
                    PathArray.Dispose();
                    PathArray = null;
                }
            }
        }
    }
}
