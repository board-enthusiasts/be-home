# Overview

For UI using UI Toolkit, we're employing a hybrid MVVM/MVC pattern approach we're referring to as MVVMC. The shared base contracts and helpers now live in `BE.GDK.Framework`, and the user-driven event surface is standardized on `IUserIntent`.

In this pattern, we'll have:

- `Model`: backing domain model; only contains domain data and business logic helpers for manipulating/transforming/mutating that data.
- `View`: has the reference for and manipulates the UITK `UIDocument` and `VisualElements` themselves; responsible for binding the view model to the UI controls/root and for setting the values in the view model which are data-bound to the `VisualElements` using Unity's runtime data binding system.
- `ViewModel`: UI model; only contains data binding members and helpers for manipulating/transforming/mutating that view data.
- `Controller`: the main control connecting `Model` to `View`, as in the MVC pattern.

Shared framework types now available from `BE.GDK.Framework` include:

- `IModel`
- `IViewModel`
- `IView<TViewModel>`
- `IController<TModel>`
- `IDisplayable`
- `IDisplayableController<TModel, TView, TViewModel>`
- `BaseController<TModel>`
- `BaseView<TViewModel>`
- `DisplayableController<TModel, TView, TViewModel>`
- `IUserIntent`
- `UserIntentEventArgs`
- `IUserIntentRouter`
- `IUserIntentHandler`
- `IUserIntentHandlerRoute`
- `UserIntentHandlerRoute`

The Emulator package now consumes that shared layer and only keeps Emulator-specific routing/display abstractions on top of it.

Higher-level packages must not introduce shim or alias copies of these framework contracts. If a type belongs to the shared MVVMC and user-intent framework, it should live once in `BE.GDK.Framework` and be consumed from there directly.

For production game UI, the view layout and styling should live in UXML and USS assets. The C# view should bind/query those authored assets and apply runtime behavior, but it should not construct the full screen layout tree in code unless there is a specific approved reason to do so.

# Example

For example implementation of a full MVVMC set, see the following:

- `EmulatorModel`
- `EmulatorView`
- `EmulatorViewModel`
- `Emulator`
