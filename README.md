# Styled Identity Framework

A framework for style-pack authors. Lets an individual Ideology-applied ThingStyleDef carry its own custom flavor description and projectile or beam configuration. A projectile weapon's projectile and its firing sound can be styled independently of each other.

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

  <!-- Framework fields: all are optional and replace only targeted behavior. -->
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <description>A shoulder-fired energy weapon approved for high-priority targets.</description>
      <projectile>Projectile_Helldiver_LaserCannon</projectile>

      <!-- Each is a SoundDef reference; all three are independently optional. -->
      <soundCast>Shot_ChargeRifle</soundCast>
      <soundCastTail>GunTail_Medium</soundCastTail>
      <soundAiming>OrbitalTargeter_Aiming</soundAiming>
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
  automatically. For a weapon mapped to this framework, `overrideLabel` also
  becomes the label shown on the drafted pawn's standard weapon (attack)
  gizmo, not just the item's inventory label.
- `<description>` replaces only the def-level flavor text. Text added by
  components (for example, generated weapon-art descriptions) still appears
  after it. For a weapon mapped to this framework, this flavor also appears
  in the weapon gizmo's tooltip description; any vanilla postfix (such as a
  weather range-cap warning) is kept.
- A style needs `graphicData` or an explicit `uiIconPath` to get a custom UI
  icon; `graphicData` alone is sufficient; RimWorld derives
  `ThingStyleDef.UIIcon` from it automatically. When present, the framework
  also uses that icon for `Verb.UIIcon`, which covers the targeting-cursor
  mouse attachment and any other UI that asks the verb for its icon (the
  weapon gizmo itself already draws the styled instance's icon natively). A
  verb's own explicit `commandIcon` is intentional and always takes priority
  over the style icon.
- `<projectile>` must reference a `ThingDef` with `ProjectileProperties` and
  a `thingClass` derived from `Verse.Projectile` (for example, `Bullet`). It
  is only used by weapons whose primary verb is `Verb_LaunchProjectile`. A
  loaded `CompChangeableProjectile` (ammo) always takes precedence over this
  override, even while a sound override on the same style stays active. For
  a purely cosmetic conversion, give the custom projectile the same combat
  properties as the base projectile. `projectile` does not apply to
  `Verb_ShootBeam` weapons; use `beamSource` for those instead.
- `<soundCast>`, `<soundCastTail>`, and `<soundAiming>` are each optional
  `SoundDef` references that replace one firing sound of a
  `Verb_LaunchProjectile` weapon, independently of `<projectile>` and of each
  other: `soundCast` is the normal, positional one-shot played on every
  successful shot; `soundCastTail` is the camera-relative distant/tail sound
  played alongside it; and `soundAiming` is the sound used during the
  weapon's aiming/warmup phase. Any of the three can be set without
  `<projectile>`, and omitting one leaves that base weapon's corresponding
  sound unchanged rather than silencing it. Like `projectile`, none of them
  apply to `Verb_ShootBeam` weapons.

The extension only affects a spawned item whose `Thing.StyleDef` is that
exact `ThingStyleDef`. Unstyled items, other styles without this extension,
and items from unrelated mods keep full vanilla behavior.

## Beam weapons

Continuous/sweeping beam weapons (`Verb_ShootBeam`, such as `Gun_BeamGraser`)
don't fire a projectile, so `<projectile>` has no effect on them. Instead,
`<beamSource>` points at a template `ThingDef` whose primary verb supplies
the styled beam configuration (damage, sweep, hit, visual, fire, mote,
effecter, and sound fields). The template is never spawned by the framework;
it exists only to hold a `Verb_ShootBeam` verb definition to copy from.

`beamSource` copies the template's entire `VerbProperties`, so it already
covers audio: `soundCast`, `soundCastTail`, and `soundAiming` behave exactly
as they do for a normal gun, and `soundCastBeam` is the sustaining `SoundDef`
for the continuous firing loop that most beam weapons actually want. Set
whichever of these the template needs directly on its verb; there is no
separate beam sound field on `StyleIdentityExtension`, and the extension's
own `soundCast`/`soundCastTail`/`soundAiming` fields only ever apply to
`Verb_LaunchProjectile` weapons, never to a beam weapon's `beamSource`.

```xml
<ThingStyleDef>
  <defName>Helldiver_Graser</defName>
  <overrideLabel>Helldiver beam cannon</overrideLabel>
  <graphicData>
    <texPath>Things/Item/Weapon/Helldiver/BeamCannon</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <beamSource>Helldiver_BeamCannon_Template</beamSource>
    </li>
  </modExtensions>
</ThingStyleDef>

<!--
  This template is never spawned by the framework, but it is still a real
  ThingDef that goes through vanilla's own def validation for whatever it
  inherits from. BaseGun requires a graphic, an authored Mass (it's
  alwaysHaulable), and either smeltable="false" or something to smelt into -
  give it the minimum needed to satisfy that, even though none of it is ever
  used in play.
-->
<ThingDef ParentName="BaseGun">
  <defName>Helldiver_BeamCannon_Template</defName>
  <label>beam graser disruptor template (unused)</label>
  <smeltable>false</smeltable>
  <graphicData>
    <texPath>Things/Item/Weapon/Helldiver/BeamCannon</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <statBases>
    <Mass>4</Mass>
  </statBases>
  <verbs>
    <li>
      <verbClass>Verb_ShootBeam</verbClass>
      <beamDamageDef>Beam</beamDamageDef>
      <beamMoteDef>Mote_GraserBeamBase</beamMoteDef>
      <beamEndEffecterDef>GraserBeam_End</beamEndEffecterDef>

      <!-- A sustaining SoundDef for the continuous beam sound while firing. -->
      <soundCastBeam>BeamGraser_Shooting</soundCastBeam>
    </li>
  </verbs>
</ThingDef>

<StyleCategoryDef>
  <defName>HelldiverBeamWeapons</defName>
  <label>Helldiver beam weapons</label>
  <thingDefStyles>
    <li>
      <thingDef>Gun_BeamGraser</thingDef>
      <styleDef>Helldiver_Graser</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

Rules and limits:

- `beamSource` is only used by a weapon whose selected primary verb is
  `Verb_ShootBeam`, or a subclass that is exactly the same runtime verb
  class as the template's primary verb. It cannot turn a bullet, spray, or
  fire weapon into a beam weapon; the change never touches the verb type,
  only the properties of an already-instantiated `Verb_ShootBeam`.
  Spray and fire verbs remain unsupported entirely.
- The override is instance-scoped: it clones the template's verb properties
  onto the runtime `Verb` of the exact styled item, and never mutates
  `ThingDef.Verbs` on the base weapon or the template. Unstyled items,
  other weapons, and the template itself are unaffected.
- `projectile` and `beamSource` are independently optional. A style pack may
  set both where it is mapped to separate compatible weapons; each runtime
  verb only ever uses the field that matches its own verb type.
- Invalid configurations (a missing or ambiguous beam verb on the template,
  or a mapped weapon whose primary verb class doesn't exactly match the
  template's) fail during def validation instead of silently doing nothing.
- The template is a real, non-abstract `ThingDef` (it has to be, so
  `beamSource` can resolve a `DefDatabase` reference to it), so it is
  checked against vanilla's own `ConfigErrors` for whatever it inherits
  from, not just this framework's. `ParentName="BaseGun"` in particular
  requires a `graphicData`, an authored `Mass` stat (guns are
  `alwaysHaulable`), and either `smeltable=false` or something to smelt
  into. None of this is ever seen in play, but it has to be present or the
  template fails to load. Setting `tradeability=None` and
  `generateCommonality=0` is also recommended, since without weapon tags an
  otherwise-normal `ThingDef` can still be picked up by generic trader
  stock or random item generation.
- Any `SoundDef` referenced by `soundCast`, `soundCastTail`, `soundAiming`,
  or `soundCastBeam` (whether on a beam template or a projectile style) must
  be a valid def supplied by RimWorld itself or by the style pack/its
  dependencies. This framework defines no audio files or `SoundDef`s of its
  own; an unresolved reference fails through RimWorld's normal XML
  def-reference resolution.

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
