# Styled Identity Framework

A framework for style-pack authors. Lets an individual Ideology-applied ThingStyleDef carry its own custom flavor description and fired projectile.

## Requirements

Add `sk.styledidframework` as a mod dependency in your pack's `About.xml`.

## Usage

Add `StyleIdentityExtension` to the _concrete_ `ThingStyleDef` that gets
assigned to a weapon or item. Every field is optional; a missing field
leaves the matching vanilla behavior completely unchanged.

```xml
<ThingStyleDef>
  <defName>Helldiver_LaserCannon</defName>

  <!-- Native RimWorld field: optional, but recommended for a renamed item. -->
  <overrideLabel>Laser Cannon</overrideLabel>

  <!-- Existing graphics fields supplied by the style pack. -->
  <graphicData>
    <texPath>Things/Item/Weapon/Helldiver/LaserCannon</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>

  <!-- Framework fields: both are optional and replace only targeted behavior. -->
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <description>A shoulder-fired energy weapon approved for high-priority targets.</description>
      <projectile>Projectile_Helldiver_LaserCannon</projectile>
    </li>
  </modExtensions>
</ThingStyleDef>

<StyleCategoryDef>
  <defName>HelldiverWeapons</defName>
  <label>Helldiver weapons</label>
  <thingDefStyles>
    <li>
      <thingDef>Gun_Laser</thingDef>
      <styleDef>Helldiver_LaserCannon</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

- `<overrideLabel>` is a native RimWorld `ThingStyleDef` field, not part of
  this framework. It flows through RimWorld's normal label pipeline, so
  quality, hit-point, corpse, and stack-count suffixes are still appended
  automatically.
- `<description>` replaces only the def-level flavor text. Text added by
  components (for example, generated weapon-art descriptions) still appears
  after it.
- `<projectile>` must reference a `ThingDef` with `ProjectileProperties` and
  a `thingClass` derived from `Verse.Projectile` (for example, `Bullet`). It
  is only used by weapons whose primary verb is `Verb_LaunchProjectile`.
  Beam, spray, and fire verbs are not supported. A loaded
  `CompChangeableProjectile` (ammo) always takes precedence over this
  override. For a purely cosmetic conversion, give the custom projectile the
  same combat properties as the base projectile.

The extension only affects a spawned item whose `Thing.StyleDef` is that
exact `ThingStyleDef`. Unstyled items, other styles without this extension,
and items from unrelated mods keep full vanilla behavior.

## Translation

Both `overrideLabel` and the extension's `description` are translated as
fields of the `ThingStyleDef`:

```xml
<LanguageData>
  <Helldiver_LaserCannon.overrideLabel>Laser Cannon</Helldiver_LaserCannon.overrideLabel>
  <Helldiver_LaserCannon.modExtensions.0.description>A shoulder-fired energy weapon approved for high-priority targets.</Helldiver_LaserCannon.modExtensions.0.description>
</LanguageData>
```

Use one `StyleIdentityExtension` per style def, and keep it first in
`modExtensions` so the `0` index in the translation key stays stable.
