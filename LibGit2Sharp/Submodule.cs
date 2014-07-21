using System;
using System.Diagnostics;
using System.Globalization;
using LibGit2Sharp.Core;

namespace LibGit2Sharp
{
    /// <summary>
    /// A Submodule.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Submodule : IEquatable<Submodule>
    {
        private static readonly LambdaEqualityHelper<Submodule> equalityHelper =
            new LambdaEqualityHelper<Submodule>(x => x.Name, x => x.HeadCommitId);

        private readonly Repository repo;
        private readonly string name;
        private readonly string path;
        private readonly string url;
        private readonly ILazy<ObjectId> headCommitId;
        private readonly ILazy<ObjectId> indexCommitId;
        private readonly ILazy<ObjectId> workdirCommitId;
        private readonly ILazy<bool> fetchRecurseSubmodulesRule;
        private readonly ILazy<SubmoduleIgnore> ignoreRule;
        private readonly ILazy<SubmoduleUpdate> updateRule;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected Submodule()
        { }

        internal Submodule(Repository repo, string name, string path, string url)
        {
            this.repo = repo;
            this.name = name;
            this.path = path;
            this.url = url;

            var commitIds = new SubmoduleLazyGroup(repo, name);
            headCommitId = commitIds.AddLazy(Proxy.git_submodule_head_id);
            indexCommitId = commitIds.AddLazy(Proxy.git_submodule_index_id);
            workdirCommitId = commitIds.AddLazy(Proxy.git_submodule_wd_id);

            var rules = new SubmoduleLazyGroup(repo, name);
            fetchRecurseSubmodulesRule = rules.AddLazy(Proxy.git_submodule_fetch_recurse_submodules);
            ignoreRule = rules.AddLazy(Proxy.git_submodule_ignore);
            updateRule = rules.AddLazy(Proxy.git_submodule_update);
        }

        /// <summary>
        /// The name of the submodule.
        /// </summary>
        public virtual string Name { get { return name; } }

        /// <summary>
        /// The path of the submodule.
        /// </summary>
        public virtual string Path { get { return path; } }

        /// <summary>
        /// The URL of the submodule.
        /// </summary>
        public virtual string Url { get { return url; } }

        /// <summary>
        /// The commit ID for this submodule in the current HEAD tree.
        /// </summary>
        public virtual ObjectId HeadCommitId { get { return headCommitId.Value; } }

        /// <summary>
        /// The commit ID for this submodule in the index.
        /// </summary>
        public virtual ObjectId IndexCommitId { get { return indexCommitId.Value; } }

        /// <summary>
        /// The commit ID for this submodule in the current working directory.
        /// </summary>
        public virtual ObjectId WorkDirCommitId { get { return workdirCommitId.Value; } }

        /// <summary>
        /// The fetchRecurseSubmodules rule for the submodule.
        ///
        /// Note that at this time, LibGit2Sharp does not honor this setting and the
        /// fetch functionality current ignores submodules.
        /// </summary>
        public virtual bool FetchRecurseSubmodulesRule { get { return fetchRecurseSubmodulesRule.Value; } }

        /// <summary>
        /// The ignore rule of the submodule.
        /// </summary>
        public virtual SubmoduleIgnore IgnoreRule { get { return ignoreRule.Value; } }

        /// <summary>
        /// The update rule of the submodule.
        /// </summary>
        public virtual SubmoduleUpdate UpdateRule { get { return updateRule.Value; } }

        /// <summary>
        /// Retrieves the state of this submodule in the working directory compared to the staging area and the latest commmit.
        /// </summary>
        /// <returns>The <see cref="SubmoduleStatus"/> of this submodule.</returns>
        public virtual SubmoduleStatus RetrieveStatus()
        {
            using (var handle = Proxy.git_submodule_lookup(repo.Handle, Name))
            {
                return Proxy.git_submodule_status(handle);
            }
        }

        /// <summary>
        /// Initialize submodule
        /// </summary>
        /// <param name="overwrite"></param>
        public virtual void Init(bool overwrite)
        {
            using (var handle = Proxy.git_submodule_lookup(repo.Handle, Name))
            {
                Proxy.git_submodule_init(handle, overwrite);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Update()
        {
            string submoduleRepoPath = System.IO.Path.Combine(this.repo.Info.WorkingDirectory, this.Path);

            // Get the status of the 
            // Clone if necessary, then checkout commit
            var submodule_status = this.RetrieveStatus();

            // If submodule is not in WorkDir, but is initialized, then clone
            if (submodule_status.HasFlag(SubmoduleStatus.WorkDirUninitialized))
            {
                // Verify submodule is initialized (i.e. information about the
                // submodule exists in .git\config)
                var configEntry = repo.Config.Get<string>(string.Format("submodule.{0}.url", this.Name));
                
                if (configEntry == null)
                {
                    var errorMessage = string.Format(CultureInfo.InvariantCulture,"Submodule {0} is not initialized.", Name);
                    throw new LibGit2SharpException(errorMessage);
                }

                Repository.Clone(this.Url, submoduleRepoPath);
            }

            // Open repository and check it out
            using (var repo = new Repository(submoduleRepoPath))
            {
                Commit commit = repo.Lookup<Commit>(this.HeadCommitId);

                // TODO: Checkout progress
                //       Proper checkout modifiers
                repo.Checkout(commit, new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force});
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="Submodule"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="Submodule"/>.</param>
        /// <returns>True if the specified <see cref="Object"/> is equal to the current <see cref="Submodule"/>; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Submodule);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Submodule"/> is equal to the current <see cref="Submodule"/>.
        /// </summary>
        /// <param name="other">The <see cref="Submodule"/> to compare with the current <see cref="Submodule"/>.</param>
        /// <returns>True if the specified <see cref="Submodule"/> is equal to the current <see cref="Submodule"/>; otherwise, false.</returns>
        public bool Equals(Submodule other)
        {
            return equalityHelper.Equals(this, other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return equalityHelper.GetHashCode(this);
        }

        /// <summary>
        /// Returns the <see cref="Name"/>, a <see cref="String"/> representation of the current <see cref="Submodule"/>.
        /// </summary>
        /// <returns>The <see cref="Name"/> that represents the current <see cref="Submodule"/>.</returns>
        public override string ToString()
        {
            return Name;
        }

        private string DebuggerDisplay
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "{0} => {1}", Name, Url);
            }
        }
    }
}
