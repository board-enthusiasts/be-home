using System;
using System.Threading;
using System.Threading.Tasks;

using BoardEnthusiasts.BeHome.Api.Models;
using BoardEnthusiasts.BeHome.Api.Services;

using UnityEngine.UIElements;

namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Coordinates the first native BE Home browse spike against the public catalog list endpoint.
/// </summary>
internal sealed class BeHomeNativeBrowseController : IDisposable
{
    private readonly BeHomeNativeBrowseModel _model;
    private readonly BeHomeNativeBrowseView _view;
    private readonly IBeHomeCatalogService _catalogService;
    private readonly Action<string> _log;
    private BeHomeNativeBrowseViewModel _viewModel;
    private BeHomeCatalogPage _lastCatalogPage;
    private CancellationTokenSource _loadCancellationSource;
    private bool _isInitialized;
    private string _selectedTitleId;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeHomeNativeBrowseController"/> class.
    /// </summary>
    /// <param name="model">The native browse model.</param>
    /// <param name="view">The native browse view.</param>
    /// <param name="catalogService">The public catalog service.</param>
    /// <param name="log">The optional logger used for spike diagnostics.</param>
    public BeHomeNativeBrowseController(
        BeHomeNativeBrowseModel model,
        BeHomeNativeBrowseView view,
        IBeHomeCatalogService catalogService,
        Action<string> log = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        _log = log;
        _viewModel = BeHomeNativeBrowseViewModelBuilder.CreateLoading(_model);
    }

    /// <summary>
    /// Initializes the native browse spike inside the supplied host element.
    /// </summary>
    /// <param name="host">The authored native browse host.</param>
    public void Initialize(VisualElement host)
    {
        if (!_isInitialized)
        {
            _view.Bind(host, _viewModel, HandleUserIntent);
            _isInitialized = true;
        }

        _ = RefreshCatalogAsync();
    }

    /// <summary>
    /// Requests a manual catalog refresh for the native browse spike.
    /// </summary>
    public void RequestRefresh()
    {
        _ = RefreshCatalogAsync();
    }

    /// <summary>
    /// Disposes the controller and cancels any in-flight catalog work.
    /// </summary>
    public void Dispose()
    {
        _loadCancellationSource?.Cancel();
        _loadCancellationSource?.Dispose();
        _loadCancellationSource = null;
        _view.Unbind();
        _isInitialized = false;
    }

    private async Task RefreshCatalogAsync()
    {
        ReplaceCancellationSource();
        var cancellationToken = _loadCancellationSource.Token;
        SetViewModel(BeHomeNativeBrowseViewModelBuilder.CreateLoading(_model));
        _log?.Invoke($"Native browse loading public catalog from {_model.ApiBaseUrl}.");

        try
        {
            var page = await _catalogService
                .ListTitlesAsync(pageSize: _model.PageSize, cancellationToken: cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            _lastCatalogPage = page;
            SetViewModel(BeHomeNativeBrowseViewModelBuilder.CreateLoaded(_model, page, _selectedTitleId));
            _selectedTitleId = ResolveSelectedTitleId(page);
            _log?.Invoke($"Native browse loaded {page.Titles.Count} titles ({page.TotalCount} total).");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            SetViewModel(BeHomeNativeBrowseViewModelBuilder.CreateError(
                _model,
                "We couldn't reach the public BE catalog right now. Check connectivity and try again."));
            _log?.Invoke($"Native browse catalog load failed: {ex}");
        }
    }

    private void HandleUserIntent(IUserIntent intent)
    {
        switch (intent)
        {
        case RefreshCatalogUserIntent:
            RequestRefresh();
            return;

        case SelectCatalogTitleUserIntent selectCatalogTitleUserIntent:
            _selectedTitleId = selectCatalogTitleUserIntent.TitleId;
            if (_lastCatalogPage != null)
            {
                SetViewModel(BeHomeNativeBrowseViewModelBuilder.CreateLoaded(
                    _model,
                    _lastCatalogPage,
                    _selectedTitleId));
            }

            return;
        }
    }

    private void SetViewModel(BeHomeNativeBrowseViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        if (_isInitialized)
        {
            _view.SetViewModel(_viewModel);
        }
    }

    private void ReplaceCancellationSource()
    {
        _loadCancellationSource?.Cancel();
        _loadCancellationSource?.Dispose();
        _loadCancellationSource = new CancellationTokenSource();
    }

    private string ResolveSelectedTitleId(BeHomeCatalogPage page)
    {
        if (page == null || page.Titles.Count == 0)
        {
            return null;
        }

        foreach (var title in page.Titles)
        {
            if (string.Equals(title.Id, _selectedTitleId, StringComparison.Ordinal))
            {
                return _selectedTitleId;
            }
        }

        return page.Titles[0].Id;
    }
}
}
