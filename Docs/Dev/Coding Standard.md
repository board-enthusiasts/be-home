# Documentation

## Always use the `<see>` or `<seealso>` (as applicable), and other similar tags when referencing C# types or members (e.g. `<paramref>`, `<typeparamref>`)

### Examples

Bad:
```csharp
/// <summary>
/// Calculate the NextPlayerIndex based on the provided ICalculationStrategy of type T.
/// </summary>
```

Good
```csharp
/// <summary>
/// Calculate the <see cref="NextPlayerIndex"/> based on the provided <see cref="ICalculationStrategy"/> of type <typeparamref name="T"/>.
/// </summary>
/// <seealso cref="PreviousPlayerIndex"/>
```
## For non-XML comments, prefer self-documenting code by using descriptive variable and type names

- Only add inline comments for context of things like
	- complexity of algorithmic details
	- justification of a an algorithm or method of doing something that seems atypical or goes against a standard
	- TODO items

### Examples

Bad:
```csharp
var x = 2;
```

Good:
```csharp
int nextPlayerIndex = 2;
```
## All non-private members should have thorough inline XML docs

- Don't be redundant to what is already clearly gleaned from the type/name. For example, if it is a property, we already know it is a property, and can see if it gets or sets it, just from the member signature.
- Describe "why"/"what", not "how", as the "how" can get out-of-date quickly from implementation.
### Examples

Bad:
```csharp
/// <summary>
/// Gets the value of NextPlayerIndex.
/// </summary>
public int NextPlayerIndex { get; }
```

Good:
```csharp
/// <summary>
/// The index that will be applied to the next added player.
/// </summary>
/// <seealso cref="CurrentPlayerIndex"/>
public int NextPlayerIndex { get; }
```

Bad:
```csharp
/// <summary>
/// Gets the next player index by incrementing the current player index by 1.
/// </summary>
public int GetNextPlayerIndex()
{
	// ...
}
```

Good:
```csharp
/// <summary>
/// Get the index that will be applied to the next added player.
/// </summary>
public int GetNextPlayerIndex()
{
	// ...
}
```
## TODO comments must always be associated with an issue number

- If you're adding a TODO comment, you must create an associated issue against the applicable repo in GitHub (or whatever provider we may be using in the future) and tag it with the TODO
- When completing issues, make sure you always search the TODOs for the related issue number and clean them up before completing the issue.
### Examples

Bad:
```csharp
// TODO: this needs to be refactored because X
```

Good:
```csharp
// TODO [issue_number]: this needs to be refactored because X
```

# Naming Conventions

Generally, follow the [Microsoft standard naming convention for C#](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names#naming-conventions). For any conflicts, the rules in this document take precedence.
# Unity Serialized Fields/Properties

## Both
### Always add `[Tooltip]` with an editor-user-friendly description of the use of the field
### Always organize at the top of a serializable class, above all other members, regardless of access level

- We always want to be very clear about what is exposed to the Unity inspector.
### Use decorator attributes like `[Range]`, `[Min]`, and `[TextArea]` to help with usability in the inspector

- If it is known that a serialized field or property has some constraints, use Unity's decoration attributes to format the field nicely in the inspector for the user, and to also help control input.
- Caveat: try not to hardcode these values if they are tied to business/domain logic, but reference constants if possible.
## Unity Serialized Fields

### Never public

- Never expose Unity serialized **fields** publicly. Prefer private, unless there is a very strong argument/case for why they should be protected or internal. If so, justify the case in comments.
- Make sure the containing classes are marked with `[Serializable]` when necessary (e.g. not necessary for classes derived from `ScriptableObject` and `MonoBehaviour`, but required for vanilla C# classes).
- Always use the `[SerializeField]` attribute to ensure the non-public member is exposed to the Unity inspector.
### Always prefix with `m_`

- Unity serialized fields should always be prefixed with `m_` regardless of their access level
### Always use lowerCamelCase after prefix

## Examples

Bad:
```csharp
public class MyClass
{
	public float force;
}
```

Good:
```csharp
using System;
using UnityEngine;

[Serializable]
public class MyClass
{
	[SerializeField]
	[Tooltip("The force applied when the ships thrusters are active.")]
	private float m_thrusterForce;
}
```
# Unity Serialized Properties

- Prefer Unity serialized auto-properties in cases when there would be a backing Unity serialized field that would just be used as a backing field for passthrough.
- Not applicable if there must be logic wrapped around the backing field, as Unity only supports these for auto-properties.
## Never private

There is no reason to use this type for private members. Use Unity serialized fields for private members.
## Follow standard naming convention for C# properties

- No special naming rules for these members
## Examples

Bad:
```csharp
public class MyClass
{
	[SerialzieField]
	private int numPlayers;
	
	public int NumPlayers { get => numPlayers; set => numPlayers = value; }
}
```

Good:
```csharp
using System;
using UnityEngine;

[Serializable]
public class MyClass
{
	/// <summary>
	/// The force applied when the ships thrusters are active.
	/// </summary>
	[field: SerializeField]
	[field: Tooltip("The force applied when the ships thrusters are active.")]
	[field: Range(1,4)]
	public int NumPlayers { get; set; }
}
```
# Private Static Fields

## Prefix with `s_`

## Examples

Bad:
```csharp
private static int _myInt;
```

Good:
```csharp
private static int s_myInt;
```

# UI Toolkit (UITK)

## UXML

### Never set inline styles in UXML; prefer to set styles in USS files

### Do not use inline `style="..."`-style layout or appearance authoring in UXML; attach USS classes and let USS own presentation

### Prefer setting runtime data bindings in UXML for setting values of UXML attributes, rather than setting them directly in C# code

### Production screen layouts should be authored in UXML, not constructed wholesale in C# code

### Prefer `data-source-type` plus `DataBinding` in UXML for bindable view-model content instead of manually pushing every text/style value from C\#

## Unity Style Sheets (USS)

### Always check the latest [supported USS properties](https://docs.unity3d.com/6000.4/Documentation/Manual/UIE-USS-Properties-Reference.html) for the current Unity version the project is using, and **do not use properties that are not listed/supported**.

Examples:
- `row-gap` and `column-gap` are **not** currently supported, even though AI agents often hallucinate that it is due to certain Unity forum posts.

### Use hyphen-case for class names and IDs

### Only set inline styles in C# code as an absolute last resort; prefer to set styles in USS files

### Prefer class toggles on screen/template root elements for runtime visual states instead of setting inline style values from C\#

### Prefer BEM naming convention for class names, but this is not a hard requirement

## Localization

### All player-facing production text must go through Unity Localization

- Do not hardcode production copy in C# classes, UXML, or USS.
- Define localized strings in the appropriate table collection for the owning layer.
- Bind localized values into the UI through view models or localized bindings rather than embedding raw fallback copy in the authored screen markup.

### Localized assets should also use Unity Localization when they vary by locale

- Examples include icons, locale-specific art treatments, or any imagery that contains text.
- Place localization tables/assets in `Assets/i18n`.
