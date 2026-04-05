We currently use [Extenject](https://github.com/Mathijs-Bakker/Extenject) for DI. Make sure you read the documentation to familiarize yourself with the framework, particularly the injection lifecycle, the gotchas and quirks.
# Prefer Constructor Injection

For maximum portability/compatibility between dependency injection systems in the future, in case we decide to switch our DI provider or want to support multiple ones, always prefer vanilla constructor injection when possible above using any custom `Inject` attributes that the framework may provide.

For Zenject, in some cases this is not possible (e.g. `MonoBehaviours` cannot have constructors). In these cases, prefer **method injection** above property/field injection. This keeps defined injections confined to a single location and makes it easier to understand what is injected.
## Examples

```csharp
public class MyVanillaClass
{
	private ISomeInterface _someInterface;

	public MyVanillaClass([NotNull] ISomeInterface someInterface)
	{
		_someInterface = someInterface ?? throw new InjectionException(nameof(someInterface));
	}
}
```

```csharp
public class MyBehavior : UnityEngine.MonoBehaviour
{
	private ISomeInterface _someInterface;

	[Inject]
	private void Injection([NotNull] ISomeInterface someInterface)
	{
		_someInterface = someInterface ?? throw new InjectionException(nameof(someInterface));
	}
}
```

# `ScriptableObject` Injection

`ScriptableOjbect` instances do not get injected automatically like `MonoBehaviours` do, so you must make sure that you either queue them for injection from an installer during the DI graph build/bindings, or inject them directly via the `DIContainer` instance later.

# Dynamic Object Creation

Prefer the use of [factories](https://github.com/Mathijs-Bakker/Extenject/blob/master/Documentation/Factories.md), but any time that you want/need to create objects dynamically at runtime, you **must** create them using an injected `IInstantiator` via an applicable [Instantiate]([https://github.com/Mathijs-Bakker/Extenject/blob/master/Documentation/Factories.md](https://github.com/Mathijs-Bakker/Extenject?tab=readme-ov-file#dicontainerinstantiate)) method. This ensures that the object is properly injected at runtime.


> [!WARNING] `Object.Instantiate`
> Do NOT use any of the `Object.Instantiate` methods, as this will result in your object not being injected.

# Non-Public Collaborators

If a type needs helper collaborators during construction, but those collaborator interfaces are not meant to be part of the public DI surface, do not bind them just to satisfy one constructor. Prefer a `FromMethod(...)` or dedicated factory binding that constructs the object with those internal collaborators directly.

`LoggerFactory` is the current project example: keep `ILogProvider` and `IShouldLogStrategy` internal to the logging package setup rather than exposing them as globally injectable contracts.

If the package is providing a default implementation for a contract that consumers may reasonably replace, bind that default as a fallback with `IfNotBound()`. This keeps the package usable out of the box while still allowing consuming projects to provide their own binding for the public contract.
