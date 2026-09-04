<h1 align="center">Runtime Scriptable Edit</h1>

<p align="center">
  <em>Tune your ScriptableObject config assets while the game is running &mdash; and keep the changes when you stop.</em>
</p>

<p align="center">
  <img alt="Unity 6000.0.68f1 or newer" src="https://img.shields.io/badge/Unity-6000.0.68f1%2B-000000?logo=unity&logoColor=white">
  <img alt="Editor only" src="https://img.shields.io/badge/scope-Editor%20only-blue">
  <img alt="No dependencies" src="https://img.shields.io/badge/dependencies-none-brightgreen">
  <img alt="Asset Store pending publication" src="https://img.shields.io/badge/Asset%20Store-pending%20publication-yellow">
</p>

---

Unity throws away ScriptableObject edits made during Play Mode the moment you leave it. So
the usual tuning loop is: play, guess, stop, edit, play again.

Runtime Scriptable Edit replaces that loop. It hands your scene a disposable copy of each
asset you nominate, lets you edit that copy live in a dedicated window, and gives you an
explicit choice when you stop: throw the changes away, or write them back to the real asset.

<p align="center">
  <strong>Editor-only.</strong> Nothing in this package is compiled into a build.
</p>

---

<h2>📋 Requirements</h2>

<table>
  <tr><td><strong>Unity</strong></td><td>6000.0.68f1 or newer</td></tr>
  <tr><td><strong>Render pipeline</strong></td><td>Any &mdash; Built-in, URP and HDRP all work</td></tr>
  <tr><td><strong>Dependencies</strong></td><td>None</td></tr>
</table>

---

<h2>⚙️ Installation</h2>

<details open>
<summary><strong>From the Asset Store</strong></summary>

<br>

Import the package. It installs to `Assets/RuntimeScriptableEdit/` and needs no further
setup.

</details>

<details>
<summary><strong>Manually</strong></summary>

<br>

Copy the `RuntimeScriptableEdit` folder anywhere inside your `Assets` folder.

> **Note**
> It does **not** need to go in an `Editor` folder. The package carries its own assembly
> definitions, and the editor code is already restricted to the Editor platform. Placing the
> whole package inside `Assets/Editor/` would force the runtime and demo code into the
> Editor assembly too, which breaks the demo scene.

</details>

---

<h2>🚀 Quick start</h2>

<table>
  <tr>
    <td align="center"><strong>1</strong></td>
    <td><strong>Create a profile.</strong><br><code>Assets &gt; Create &gt; Runtime Scriptable Edit &gt; Profile</code></td>
  </tr>
  <tr>
    <td align="center"><strong>2</strong></td>
    <td><strong>List the assets you want to tune.</strong><br>Select the profile and add your ScriptableObjects to <code>Tunable Assets</code>.</td>
  </tr>
  <tr>
    <td align="center"><strong>3</strong></td>
    <td><strong>Open the window.</strong><br><code>Tools &gt; RuntimeScriptableEdit &gt; Runtime Scriptable Edit Window</code></td>
  </tr>
  <tr>
    <td align="center"><strong>4</strong></td>
    <td><strong>Set the profile as active.</strong><br>Drag it onto the <code>Active Profile</code> field, or pick it with the field's picker button. This field only appears in Edit Mode.</td>
  </tr>
  <tr>
    <td align="center"><strong>5</strong></td>
    <td><strong>Press Play and tune.</strong><br>Every tunable asset gets an inspector in the window. Edits apply on the next frame.</td>
  </tr>
  <tr>
    <td align="center"><strong>6</strong></td>
    <td><strong>Decide what to keep.</strong><br>See the table below.</td>
  </tr>
</table>

<h3>What the three buttons do</h3>

<table>
  <tr>
    <th align="left">Button</th>
    <th align="left">Effect</th>
  </tr>
  <tr>
    <td><strong>Temporary Apply</strong></td>
    <td>Writes to the real asset now, but it is rolled back when Play Mode ends</td>
  </tr>
  <tr>
    <td><strong>Perma Save</strong></td>
    <td>Writes to the real asset and keeps it after Play Mode ends</td>
  </tr>
  <tr>
    <td><strong>Revert</strong></td>
    <td>Restores the values from the last Perma Save, or from when Play Mode began</td>
  </tr>
</table>

> **Warning**
> Anything you have not **Perma Saved** is reverted automatically on exiting Play Mode.

---

<h2>🎮 Demo scene</h2>

`Demo/Scenes/RuntimeScriptableEditDemo.unity`

A capsule patrols and bounces using nothing but the values in `DemoMovementConfig`. The
on-screen readout shows the values being read on the current frame, so dragging a slider in
the window while the scene runs shows the number and the motion changing together.

The demo profile ships pre-populated. Open the scene, set it as the active profile, press
Play, and start tuning.

---

<h2>⚠️ The one rule your own code has to follow</h2>

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

<h2>📖 Documentation</h2>

Full setup guide, API reference and troubleshooting:

* **[Documentation/QuickStart.md](Documentation/QuickStart.md)**
* **[Documentation/RuntimeScriptableEdit_Documentation.pdf](Documentation/RuntimeScriptableEdit_Documentation.pdf)**

---

<h2>📁 Package layout</h2>

```
RuntimeScriptableEdit/
├── Runtime/          RuntimeScriptableEditProfile — the only type your build-time code sees
├── Editor/           Bootstrap, registry, reference replacer, editor window
├── Demo/             Sample config, mover, HUD and the demo scene
└── Documentation/    Quick start guide and PDF manual
```

---

<h2>🛒 Unity Asset Store</h2>

<table>
  <tr>
    <td><strong>Status</strong></td>
    <td>⚠️ <em>Currently in the publishing process — not yet available.</em></td>
  </tr>
  <tr>
    <td><strong>Link</strong></td>
    <td><em>Will be added once live.</em></td>
  </tr>
</table>
