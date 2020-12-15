namespace ChaoticBadge
{
    /// <summary>
    /// Represents a badge type used to populate default background color and text.
    /// </summary>
    public enum BadgeType
    {
        /// <summary>
        /// The process is passing.
        /// </summary>
        Passing,

        /// <summary>
        /// The process is failing.
        /// </summary>
        Failing,

        /// <summary>
        /// The status cannot be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The target is in a release state.
        /// </summary>
        Release,

        /// <summary>
        /// The target is in a prerelease state.
        /// </summary>
        PreRelease,

        /// <summary>
        /// The resource was not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// An operation failed.
        /// </summary>
        Error
    }
}
