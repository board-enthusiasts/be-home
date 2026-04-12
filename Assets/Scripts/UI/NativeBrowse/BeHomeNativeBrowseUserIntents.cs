using System;

namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Represents a user-driven UI intent emitted from the native BE Home browse spike.
/// </summary>
internal interface IUserIntent
{
}

/// <summary>
/// Requests that the native BE Home browse spike reload the current public catalog page.
/// </summary>
internal sealed class RefreshCatalogUserIntent : IUserIntent
{
}

/// <summary>
/// Requests that the native BE Home browse spike select a specific catalog title in the summary panel.
/// </summary>
internal sealed class SelectCatalogTitleUserIntent : IUserIntent
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectCatalogTitleUserIntent"/> class.
    /// </summary>
    /// <param name="titleId">The selected title identifier.</param>
    public SelectCatalogTitleUserIntent(string titleId)
    {
        TitleId = !string.IsNullOrWhiteSpace(titleId)
            ? titleId
            : throw new ArgumentException("A title id is required.", nameof(titleId));
    }

    /// <summary>
    /// Gets the selected title identifier.
    /// </summary>
    public string TitleId { get; }
}
}
