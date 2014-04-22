using LibGit2Sharp.Core;
using LibGit2Sharp.Handlers;

namespace LibGit2Sharp
{
    /// <summary>
    /// Collection of parameters controlling Checkout behavior.
    /// </summary>
    public sealed class CheckoutOptions
    {
        /// <summary>
        /// Options controlling checkout behavior.
        /// </summary>
        public CheckoutModifiers CheckoutModifiers { get; set; }

        /// <summary>
        /// Delegate that checkout progress will be reported through.
        /// </summary>
        public CheckoutProgressHandler OnCheckoutProgress { get; set; }

        /// <summary>
        /// Delegate to be called during checkout for files that match
        /// desired filter specified with the NotifyFlags property.
        /// </summary>
        public CheckoutNotifyHandler OnCheckoutNotify { get; set; }

        /// <summary>
        /// The Flags specifying what notifications are reported.
        /// </summary>
        public CheckoutNotifyFlags CheckoutNotifyFlags { get; set; }

        /// <summary>
        /// How Checkout should handle Conflicts.
        /// </summary>
        public FileConflictStrategy FileConflictStrategy { get; set; }

        /// <summary>
        /// Method to translate from <see cref="FileConflictStrategy"/> to <see cref="CheckoutStrategy"/> flags.
        /// </summary>
        internal CheckoutStrategy CheckoutConflictStrategyFlags
        {
            get
            {
                CheckoutStrategy flags = default(CheckoutStrategy);

                switch (FileConflictStrategy)
                {
                    case LibGit2Sharp.FileConflictStrategy.Ours:
                        flags = CheckoutStrategy.GIT_CHECKOUT_USE_OURS;
                        break;
                    case LibGit2Sharp.FileConflictStrategy.Theirs:
                        flags = CheckoutStrategy.GIT_CHECKOUT_USE_THEIRS;
                        break;
                    case LibGit2Sharp.FileConflictStrategy.Merge:
                        flags = CheckoutStrategy.GIT_CHECKOUT_CONFLICT_STYLE_MERGE;
                        break;
                    case LibGit2Sharp.FileConflictStrategy.Diff3:
                        flags = CheckoutStrategy.GIT_CHECKOUT_CONFLICT_STYLE_DIFF3;
                        break;
                }

                return flags;
            }
        }

        /// <summary>
        /// Generate a <see cref="CheckoutCallbacks"/> object with the delegates
        /// hooked up to the native callbacks.
        /// </summary>
        /// <returns></returns>
        internal CheckoutCallbacks GenerateCallbacks()
        {
            return CheckoutCallbacks.From(OnCheckoutProgress, OnCheckoutNotify);
        }
    }

    /// <summary>
    /// Enum specifying what content checkout should write to disk
    /// for conflicts.
    /// </summary>
    public enum FileConflictStrategy
    {
        /// <summary>
        /// Use the default behavior for handling file conflicts.
        /// </summary>
        Default,

        /// <summary>
        /// For conflicting files, checkout the "ours" (stage 2)  version of
        /// the file from the index.
        /// </summary>
        Ours,

        /// <summary>
        /// For conflicting files, checkout the "theirs" (stage 3) version of
        /// the file from the index.
        /// </summary>
        Theirs,

        /// <summary>
        /// Write normal merge files for conflicts.
        /// </summary>
        Merge,

        /// <summary>
        /// Write diff3 formated files for conflicts.
        /// </summary>
        Diff3
    }
}
