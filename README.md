# Runtime Scriptable Edit

Tune your ScriptableObject config assets **while the game is running**, and keep the changes
when you stop.

Unity resets ScriptableObject edits made during Play Mode as soon as you leave it. Runtime
Scriptable Edit hands your scene a disposable copy of each asset you nominate, lets you edit
that copy live in a dedicated window, and gives you an explicit choice at the end: throw the
changes away, or write them back to the real asset.

**Editor-only.** Nothing in this package is compiled into a build.

---

## Requirements

| | |
|---|---|
| Unity | 6000.0.68f1 or newer |
| Render pipeline | Any — Built-in, URP and HDRP all work |
| Dependencies | None |

---

## Installation

**From the Asset Store:** import the package. It installs to
`Assets/RuntimeScriptableEdit/` and needs no further setup.

**Manually:** copy the `RuntimeScriptableEdit` folder anywhere inside your `Assets` folder.
It does *not* need to go in an `Editor` folder — the package carries its own assembly
definitions, and the editor code is already restricted to the Editor platform.

---

## Quick start

1. **Create a profile.**
   `Assets > Create > Runtime Scriptable Edit > Profile`

2. **List the assets you want to tune.**
   Select the profile and add your ScriptableObjects to `Tunable Assets`.

3. **Open the window.**
   `Tools > RuntimeScriptableEdit > Runtime Scriptable Edit Window`

4. **Set the profile as active.**
   Drag it into the `Active Profile` field. This only appears in Edit Mode.

5. **Press Play and tune.**
   Every tunable asset gets an inspector in the window. Edits apply on the next frame.

6. **Decide what to keep.**

   | Button | Effect |
   |---|---|
   | **Temporary Apply** | Writes to the real asset now, but it is rolled back when Play Mode ends |
   | **Perma Save** | Writes to the real asset and keeps it after Play Mode ends |
   | **Revert** | Restores the values from the last Perma Save, or from when Play Mode began |

   Anything you have not **Perma Saved** is reverted automatically on exiting Play Mode.

---

## Demo scene

`Demo/Scenes/RuntimeScriptableEditDemo.unity`

A capsule patrols and bounces using nothing but the values in `DemoMovementConfig`. The
on-screen readout shows the values being read on the current frame, so dragging a slider in
the window while the scene runs shows the number and the motion changing together.

The demo profile ships pre-populated — open the scene, press Play, and start tuning.

---

## The one rule your own code has to follow

Read config values **when you use them**. Do not copy them into fields at startup:

```csharp
// Live tuning works — the value is read on the frame it is needed.
void Update()
{
    transform.position += transform.forward * config.moveSpeed * Time.deltaTime;
}

// Live tuning silently does nothing — the value was captured before you could edit it.
void Awake()
{
    cachedSpeed = config.moveSpeed;
}
```

The package swaps your component's *reference* to point at a runtime copy. A value already
copied out of the asset is beyond its reach.

---

## Documentation

Full setup guide, API reference and troubleshooting:

* **[Documentation/QuickStart.md](Documentation/QuickStart.md)**
* **[Documentation/RuntimeScriptableEdit_Documentation.pdf](Documentation/RuntimeScriptableEdit_Documentation.pdf)**

---

## Package layout

```
RuntimeScriptableEdit/
├── Runtime/          RuntimeScriptableEditProfile — the only type your build-time code sees
├── Editor/           Bootstrap, registry, reference replacer, editor window
├── Demo/             Sample config, mover, HUD and the demo scene
└── Documentation/    Quick start guide and PDF manual
```

---

## Unity Asset Store

**Status:** currently in the publishing process — not yet available.

*(Link will be added once live.)*
