# Rendering Engine

We will use Unity 6.4 LTS and keep it updated to latest patch version. The main reason is Board SDK support is only available for Unity right now, but also developer familiarity with the engine, assets ecosystem, and licensing.
# General Architecture Guidance
- Prefer code-first implementations that have Unity inspector wrappers exposing configuration to the editor for designers to tweak things without having to edit code, but developers can tweak things how they like directly in code.
- Always avoid raw/magic strings and numbers, preferring to expose them as configuration. You can generally use the Rahmen libraries as good examples for how this can be approached using interfaces as tags, particularly the events package.
- Ideally all systems are backed with interfaces where possible to be able to take full advantage of DI for configuration.
# UI

We will use Unity's [UI Toolkit](https://docs.unity3d.com/6000.3/Documentation/Manual/UIElements.html) (UITK), in screen space; (no [uGUI](https://docs.unity3d.com/Packages/com.unity.ugui@2.5/manual/index.html) unless absolutely necessary as a fallback for something UITK does not yet support). The reason for this is that UITK can be entirely written in code very easily while uGUI is highly editor-based for setup, using game objects, components, and typically a large amount of prefabs.

UITK is also Unity's [documented direction](https://docs.unity3d.com/6000.3/Documentation/Manual/UIToolkits.html#_leavefeedback) for the future of their UI, so we intend to align with the guidance.

For UI Implementation with UITK, we expect to follow an MVC/MVVM hybrid approach which we'd refer to as MVVMC.

Production game screens implemented with UI Toolkit should be authored primarily in UXML and USS assets. Use C# in views/controllers only for binding, dynamic state changes, templated element instantiation, and event wiring; do **not** build screen layouts directly in C# except for narrow utility/debug cases or explicitly approved exceptions where there is no way to query the element from UXML.

All player-facing text, and any assets that vary by locale, should be implemented through Unity Localization tables and locale assets. Do not hardcode production copy in views, view models, controllers, or screen models.

There is a base level framework for the MVVMC pattern we are using, with contracts and helpers. This framework is currently located in the `BE GDK for Board` package, in the `BE.GDK.Framework` namespace. Higher-level packages and assemblies must consume those directly rather than creating package-local shim copies of types like `IModel`, `IViewModel`, `IView<T>`, `BaseView<T>`, `IUserIntent`, or `UserIntentEventArgs`.
## Runtime UI Theme Architecture

- `Assets/Settings/UI/PanelSettings` is already set up for the app and shouldn't have to be modified often, if at all.

Runtime UI theming is centralized in the shared theme stack under `Assets/UI/`. 

- Non-icon textures should be placed under `Assets/UI/Textures/`
- Icon textures should be placed under `Assets/UI/Textures/Icons/`. We are standardizing on sourcing icons from 
- `BE Theme.tss` is the runtime theme entry point that the panel settings references.
- `BE Styles.uss` is the canonical location for shared UI theme definitions such as:
  - custom USS variables for brand colors and base style colors, spacing/tokenized sizing, fonts, and other reusable presentation values
  - shared icon asset variables using relative `url(...)` references
  - reusable theme-level utility/layout/component classes that apply those shared variables and other useful common styling
- Screen-local USS files should consume shared values from `BE Styles.uss` via `var(...)` and shared classes rather than redefining icon paths, common button palettes, or other cross-screen theme concerns.
- Screen-local USS should focus on screen layout, element composition, and truly screen-specific state styling.
- When a new color, icon, or other style token is expected to be reused across screens, promote it into `BE Styles.uss` instead of duplicating literals across multiple USS files.
- Runtime UI panel settings should continue to point at `BE Theme.tss` so all Board/client screens receive the same shared theme layer automatically.

See the dev docs for more detailed information about UI implementation expectations.
# Input
  
As this is an app that will be built to Board, we must use Unity's ["new" input system](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html) exclusively for handling input. Do NOT use the legacy input manager. The Board SDK's `BoardUIInputModule` is already instanced in the `bootstrap` scene on the `EventSystem` game object, and should generally be left alone; it should process most everything for us automatically, so we should only have to worry about using UITK callback registration for input as usual. The main input we'll be dealing with is touches and presses, and Android on-screen keyboard input for fields.
# Dependency Injection (DI)  
We will use [Zenject (aka Extenject)](https://github.com/Mathijs-Bakker/Extenject) to start with, as that is what we are most familiar with; preferring `ScriptableObjectInstallers` for cross-scene installations and prefab-based `MonoInstallers` for scene-only installation.

For the [Rahmen](#Rahmen) and [Board GDK](#Board GDK) packages, we are experimenting with supporting `VContainer` as well as `Zenject`, to provide flexibility for the users of our packages, but this is not yet fully proven out or implemented.