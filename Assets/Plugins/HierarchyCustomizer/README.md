# Hierarchy Customizer

A free, lightweight alternative to vHierarchy2: assign a color and/or icon to
any GameObject in the Hierarchy, or any folder in the Project window, to keep
big scenes and projects organized.

## Installation

1. Copy the `HierarchyCustomizer` folder into your project's `Assets` folder
   (e.g. `Assets/HierarchyCustomizer`). It contains its own `Editor` folder
   with an editor-only assembly definition, so it will never end up in your
   builds.
2. Let Unity recompile.
3. Run **Tools > Hierarchy Customizer > Create Database Asset** once. This
   creates the data asset that stores every color/icon you assign. It's
   created next to the plugin's scripts by default.
4. Commit the resulting `CustomizerDatabase.asset` (and its `.meta` file) to
   source control, the same as any other project asset.

The database is **never created automatically** — only step 3 creates it.
Until you run it, the tool stays fully inactive (no hover buttons, no
drawing) and just logs a one-time reminder in the Console. This is
deliberate: auto-creating it on first use meant every teammate's machine (or
a fresh clone) could generate its own copy with a different GUID before
anyone had pulled the committed one, which produces merge conflicts and
broken references. With an explicit, one-time creation step, everyone ends
up sharing the exact same asset.

If a database asset already exists anywhere in the project, the tool finds
it automatically by type — it doesn't matter where you put it or where it
ends up after a clone, so feel free to move it if you'd rather keep it
somewhere else.

## Usage

- **Hierarchy**: hover over any GameObject's row. A small dot button (●)
  appears at the far right — click it to open the color/icon picker.
  - Click a color swatch to tint the whole row.
  - Click a built-in icon to replace the object's icon in the Hierarchy.
  - Drag any `Texture2D` into the "Custom icon" field to use your own icon.
  - Click the ✕ at the start of either row to clear that color or icon.
  - Once an object has a color, its dot button stays visible even without
    hovering, so you can always see and re-open it.
- **Project window**: same idea, but only shows up on **folders** (works in
  both list view and grid/icon view). The color re-draws the actual folder
  icon texture tinted with your chosen color (so it still looks like a
  folder, just recolored — pick a color with alpha < 1 for a lighter tint),
  and a custom icon is drawn as a small badge in the bottom-right corner
  rather than replacing the whole icon.
- **Tools > Hierarchy Customizer**:
  - `Create Database Asset` — the one-time setup step described above. If a
    database already exists anywhere in the project, this just selects it
    instead of making a duplicate.
  - `Select Database Asset` — jumps to the current data asset in the
    Project window (offers to create one if none exists yet). Commit this
    file to source control if you want your team to share the same
    colors/icons.
  - `Clear All Customizations` — wipes every assignment (with a confirmation).

## How it works

- Colors/icons for GameObjects are keyed by a `GlobalObjectId`, which stays
  stable across editor sessions and scene reloads (as long as the object
  itself isn't deleted and recreated).
- Colors/icons for folders are keyed by the folder's asset GUID, so they
  survive renames and moves.
- Everything is drawn via `EditorApplication.hierarchyWindowItemOnGUI` and
  `EditorApplication.projectWindowItemOnGUI` — no changes to your actual
  scene data or assets, it's purely an editor-side overlay.

## Customizing further

- **More icons**: add any built-in icon name to the `BuiltinIconNames` array
  in `IconLibrary.cs`. Any name recognized by
  `EditorGUIUtility.IconContent(name)` works.
- **More preset colors**: edit the `PresetColors` array in
  `IconColorPickerPopup.cs`.
- **Layout tweaks**: the row-tint start position, icon size, and button size
  are all simple constants at the top of `HierarchyCustomizerHook.cs` and
  `ProjectCustomizerHook.cs` if you want to nudge alignment for your Unity
  version or editor theme.

## Performance notes

- Computing a GameObject's stable ID (`GlobalObjectId.GetGlobalObjectIdSlow`)
  is genuinely expensive, so it's cached per instance ID and only
  recomputed when the hierarchy structurally changes or an instance ID gets
  reused by a different object.
- Rows/folders with nothing assigned and no mouse hover skip all drawing
  work entirely, and label styles are cached instead of being rebuilt every
  repaint, so the overhead scales with how many items you've actually
  customized, not with the total size of your scene or project.
- The Project window fires its GUI callback for every visible item, most of
  which are files, not folders. Whether a given GUID is a folder is cached
  per-GUID (invalidated only when the project's asset structure changes),
  so files are skipped instantly instead of re-querying AssetDatabase every
  repaint.
- Custom icon textures (the ones you drag into the "Custom icon" field) are
  now cached by GUID too, so using a custom icon doesn't cost an
  AssetDatabase lookup every repaint the way it used to.
- The small customize button, the clear-icon "✕", and the picker's icon
  grid buttons all reuse cached `GUIContent`/`GUIStyle` objects instead of
  allocating new ones each repaint - implicit string-to-GUIContent
  conversions in IMGUI allocate every time, which adds up when a row/folder
  is visibly hovered or colored continuously.
- `EditorUtility.InstanceIDToObject` is version-guarded to use
  `EditorUtility.EntityIdToObject` on Unity 6+ where the former is obsolete,
  so you shouldn't see that compiler warning anymore.
- Unity is mid-migration from `int` instance IDs to a new `EntityId` type
  across much of the Editor API, and the exact replacement methods have
  been shifting between Unity 6.x point releases. Where we already have the
  actual object reference on hand (e.g. checking whether a GameObject is
  selected), we compare against `Selection.gameObjects` directly instead of
  going through any ID at all - that sidesteps the churn entirely rather
  than chasing each point release's exact API shape. If you hit a similar
  "X is obsolete, use Y instead" warning from a different Unity API this
  tool touches, the same approach (use the object reference you already
  have instead of an ID) is usually the most durable fix.

## Known limitations

- If you'd used an earlier version of this tool, your old data asset may
  still be sitting at `Assets/Editor/HierarchyCustomizer/CustomizerDatabase.asset`.
  You don't need to move it — the tool finds any `CustomizerDatabase` asset
  in the project by type, regardless of location — so it'll just be picked
  up as-is.

- Very deeply nested/indented objects, and unusual Project window layouts,
  may need the offset constants above tweaked slightly — exact pixel
  positions can shift a little between Unity versions.
- Colors are drawn as flat overlays (not blended with selection highlight),
  so a selected + colored row will show the color on top of the selection
  tint, which is normal and matches most similar tools.
