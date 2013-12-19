using LibGit2Sharp.Core.Compat;

namespace LibGit2Sharp.Core
{
    internal static class GitDiffExtensions
    {
        public static bool IsBinary(this GitDiffDelta delta)
        {
            //TODO Fix the interop issue on amd64 and use GitDiffDelta.Binary
            return ((GitDiffFileFlags)delta.Flags).HasFlag(GitDiffFileFlags.GIT_DIFF_FLAG_BINARY);
        }
    }
}
