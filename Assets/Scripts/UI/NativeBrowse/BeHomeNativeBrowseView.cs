using System;

using UnityEngine.UIElements;

namespace BoardEnthusiasts.BeHome.UI.NativeBrowse
{
/// <summary>
/// Binds the native BE Home browse spike view model to the authored UI Toolkit surface.
/// </summary>
internal sealed class BeHomeNativeBrowseView
{
    private const string HeadingName = "native-browse-heading";
    private const string SubtitleName = "native-browse-subtitle";
    private const string StatusName = "native-browse-status";
    private const string MessageName = "native-browse-message";
    private const string RefreshButtonName = "native-browse-refresh";
    private const string EmptyStateName = "native-browse-empty";
    private const string EmptyTitleName = "native-browse-empty-title";
    private const string EmptyBodyName = "native-browse-empty-body";
    private const string ListName = "native-browse-list";
    private const string SelectionTitleName = "native-browse-selection-title";
    private const string SelectionStudioName = "native-browse-selection-studio";
    private const string SelectionMetaName = "native-browse-selection-meta";
    private const string SelectionDescriptionName = "native-browse-selection-description";
    private const string CardClassName = "be-home-native-browse__card";
    private const string CardSelectedModifierClassName = "be-home-native-browse__card--selected";
    private const string CardTitleClassName = "be-home-native-browse__card-title";
    private const string CardStudioClassName = "be-home-native-browse__card-studio";
    private const string CardMetaClassName = "be-home-native-browse__card-meta";
    private const string CardDescriptionClassName = "be-home-native-browse__card-description";

    private VisualElement _host;
    private Label _heading;
    private Label _subtitle;
    private Label _status;
    private Label _message;
    private Button _refreshButton;
    private VisualElement _emptyState;
    private Label _emptyTitle;
    private Label _emptyBody;
    private ScrollView _list;
    private Label _selectionTitle;
    private Label _selectionStudio;
    private Label _selectionMeta;
    private Label _selectionDescription;
    private Action<IUserIntent> _intentHandler;
    private BeHomeNativeBrowseViewModel _viewModel;

    /// <summary>
    /// Binds the authored native browse surface.
    /// </summary>
    /// <param name="host">The host element that contains the authored native browse subtree.</param>
    /// <param name="viewModel">The current native browse view model.</param>
    /// <param name="intentHandler">The handler to invoke for user intents.</param>
    public void Bind(VisualElement host, BeHomeNativeBrowseViewModel viewModel, Action<IUserIntent> intentHandler)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _intentHandler = intentHandler ?? throw new ArgumentNullException(nameof(intentHandler));
        _heading = _host.Q<Label>(HeadingName);
        _subtitle = _host.Q<Label>(SubtitleName);
        _status = _host.Q<Label>(StatusName);
        _message = _host.Q<Label>(MessageName);
        _refreshButton = _host.Q<Button>(RefreshButtonName);
        _emptyState = _host.Q<VisualElement>(EmptyStateName);
        _emptyTitle = _host.Q<Label>(EmptyTitleName);
        _emptyBody = _host.Q<Label>(EmptyBodyName);
        _list = _host.Q<ScrollView>(ListName);
        _selectionTitle = _host.Q<Label>(SelectionTitleName);
        _selectionStudio = _host.Q<Label>(SelectionStudioName);
        _selectionMeta = _host.Q<Label>(SelectionMetaName);
        _selectionDescription = _host.Q<Label>(SelectionDescriptionName);

        if (_refreshButton != null)
        {
            _refreshButton.clicked += OnRefreshClicked;
        }

        Refresh();
    }

    /// <summary>
    /// Updates the current view model and re-renders the view.
    /// </summary>
    /// <param name="viewModel">The updated native browse view model.</param>
    public void SetViewModel(BeHomeNativeBrowseViewModel viewModel)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Refresh();
    }

    /// <summary>
    /// Re-renders the current native browse view model.
    /// </summary>
    public void Refresh()
    {
        if (_viewModel == null)
        {
            return;
        }

        if (_heading != null)
        {
            _heading.text = _viewModel.HeadingText;
        }

        if (_subtitle != null)
        {
            _subtitle.text = _viewModel.SubtitleText;
        }

        if (_status != null)
        {
            _status.text = _viewModel.StatusText;
        }

        if (_message != null)
        {
            _message.text = _viewModel.MessageText;
        }

        if (_emptyState != null)
        {
            _emptyState.style.display = _viewModel.ShowEmptyState ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_emptyTitle != null)
        {
            _emptyTitle.text = _viewModel.EmptyTitleText;
        }

        if (_emptyBody != null)
        {
            _emptyBody.text = _viewModel.EmptyBodyText;
        }

        if (_selectionTitle != null)
        {
            _selectionTitle.text = _viewModel.SelectedTitleHeading;
        }

        if (_selectionStudio != null)
        {
            _selectionStudio.text = _viewModel.SelectedTitleStudio;
        }

        if (_selectionMeta != null)
        {
            _selectionMeta.text = _viewModel.SelectedTitleMeta;
        }

        if (_selectionDescription != null)
        {
            _selectionDescription.text = _viewModel.SelectedTitleDescription;
        }

        if (_refreshButton != null)
        {
            _refreshButton.SetEnabled(!_viewModel.IsLoading);
        }

        RenderCards();
    }

    /// <summary>
    /// Unbinds the native browse surface and removes view-owned callbacks.
    /// </summary>
    public void Unbind()
    {
        if (_refreshButton != null)
        {
            _refreshButton.clicked -= OnRefreshClicked;
        }

        if (_list != null)
        {
            _list.Clear();
        }

        _host = null;
        _heading = null;
        _subtitle = null;
        _status = null;
        _message = null;
        _refreshButton = null;
        _emptyState = null;
        _emptyTitle = null;
        _emptyBody = null;
        _list = null;
        _selectionTitle = null;
        _selectionStudio = null;
        _selectionMeta = null;
        _selectionDescription = null;
        _intentHandler = null;
        _viewModel = null;
    }

    private void OnRefreshClicked()
    {
        _intentHandler?.Invoke(new RefreshCatalogUserIntent());
    }

    private void RenderCards()
    {
        if (_list == null)
        {
            return;
        }

        _list.Clear();
        foreach (var card in _viewModel.Cards)
        {
            _list.Add(CreateCard(card));
        }
    }

    private VisualElement CreateCard(BeHomeNativeBrowseCardViewModel card)
    {
        var root = new VisualElement
        {
            focusable = true,
        };
        root.AddToClassList(CardClassName);
        root.EnableInClassList(CardSelectedModifierClassName, card.IsSelected);
        root.AddManipulator(new Clickable(() => _intentHandler?.Invoke(new SelectCatalogTitleUserIntent(card.TitleId))));

        var titleLabel = new Label(card.DisplayName);
        titleLabel.AddToClassList(CardTitleClassName);
        root.Add(titleLabel);

        var studioLabel = new Label(card.StudioSlug);
        studioLabel.AddToClassList(CardStudioClassName);
        root.Add(studioLabel);

        var metaLabel = new Label(BuildCardMeta(card));
        metaLabel.AddToClassList(CardMetaClassName);
        root.Add(metaLabel);

        var descriptionLabel = new Label(card.Description);
        descriptionLabel.AddToClassList(CardDescriptionClassName);
        root.Add(descriptionLabel);

        return root;
    }

    private static string BuildCardMeta(BeHomeNativeBrowseCardViewModel card)
    {
        string contentKind = !string.IsNullOrWhiteSpace(card.ContentKind) ? card.ContentKind : "unknown";
        string genre = !string.IsNullOrWhiteSpace(card.GenreDisplay) ? card.GenreDisplay : "genre pending";
        string players = !string.IsNullOrWhiteSpace(card.PlayerCountDisplay) ? card.PlayerCountDisplay : "players pending";
        string age = !string.IsNullOrWhiteSpace(card.AgeDisplay) ? card.AgeDisplay : "age guidance pending";
        return $"{contentKind} | {genre} | {players} | {age}";
    }
}
}
