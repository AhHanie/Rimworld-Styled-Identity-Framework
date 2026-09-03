# Styled Identity Framework

A framework for style-pack authors. Lets an individual Ideology-applied ThingStyleDef carry its own custom flavor description and projectile-weapon or beam-weapon configuration. A projectile weapon's projectile and its firing sound can be styled independently of each other via the legacy fields, or a projectile weapon's entire verb configuration can be replaced via a full template, just like beam weapons.

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
- `<projectile>`, `<soundCast>`, `<soundCastTail>`, and `<soundAiming>` are
  the **legacy, selective override** fields for `Verb_LaunchProjectile`
  weapons: each replaces exactly one piece of the base weapon's verb (the
  launched projectile, or one firing sound) and leaves everything else on
  that verb untouched. They are mutually exclusive with `<projectileSource>`
  (see "Projectile weapon templates" below); a style must use one approach
  or the other, not both, on the same `StyleIdentityExtension`.
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

## Projectile weapon templates

`<projectile>` and the three sound fields only ever replace one piece of a
`Verb_LaunchProjectile` weapon's verb at a time. To replace the **entire**
verb configuration (range, warm-up, burst count/interval, targeting,
forced-miss settings, the launched projectile, firing/aiming sounds,
`commandIcon`, and every other `VerbProperties` field), use
`<projectileSource>` instead. It works exactly like `beamSource` (below),
but for `Verb_LaunchProjectile` weapons instead of `Verb_ShootBeam` ones.

This includes single-use launchers. `Verb_ShootOneUse` (vanilla
`Gun_TripleRocket` and `Gun_DoomsdayRocket`) is itself a
`Verb_LaunchProjectile` descendant, so a template whose primary verb is
`Verb_ShootOneUse` supplies a full configuration to a `Verb_ShootOneUse`
weapon the same way a `Verb_Shoot` template does for an ordinary gun. The
weapon's exact-class rule (below) applies here too: the mapped weapon and
the template must both declare `Verb_ShootOneUse`; a `Verb_Shoot` template
cannot convert a normal gun into a consumable launcher, and a `Verb_Shoot`
or plain `Verb_LaunchProjectile` template cannot turn a consumable launcher
into a reusable one. Consumption itself is not something the template
configures: `Verb_ShootOneUse` destroys the live equipped weapon after a
successful final burst shot (and in some partial-burst failure/loss cases)
entirely through RimWorld's own runtime logic, independent of any
`VerbProperties` field. The template only supplies the copied properties
(projectile, range, warm-up, burst, targeting, sounds, forced-miss
settings, effects, and command icon); it must not attempt to model or
disable consumption.

```xml
<ThingStyleDef>
  <defName>Style_TripleRocket_SingleUseConversion</defName>
  <overrideLabel>converted triple rocket launcher</overrideLabel>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <!-- Never spawned by the framework; only its primary Verb_ShootOneUse verb is read. -->
      <projectileSource>Template_TripleRocket_SingleUseConversion</projectileSource>
    </li>
  </modExtensions>
</ThingStyleDef>

<!-- Never spawned; same BaseGun safety metadata as any other projectileSource template. -->
<ThingDef ParentName="BaseGun">
  <defName>Template_TripleRocket_SingleUseConversion</defName>
  <label>triple rocket single-use conversion template (unused)</label>
  <tradeability>None</tradeability>
  <generateCommonality>0</generateCommonality>
  <smeltable>false</smeltable>
  <graphicData>
    <texPath>Things/Item/Equipment/WeaponRanged/RocketLauncher</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <statBases>
    <Mass>7</Mass>
  </statBases>
  <verbs>
    <li>
      <!-- Must be exactly Verb_ShootOneUse, matching Gun_TripleRocket's own primary verb. -->
      <verbClass>Verb_ShootOneUse</verbClass>
      <hasStandardCommand>true</hasStandardCommand>
      <defaultProjectile>Bullet_DoomsdayRocket</defaultProjectile>
      <warmupTime>3.2</warmupTime>
      <range>28.9</range>
      <burstShotCount>2</burstShotCount>
      <ticksBetweenBurstShots>30</ticksBetweenBurstShots>
      <soundCast>Shot_IncendiaryLauncher</soundCast>
      <soundCastTail>GunTail_Medium</soundCastTail>
      <onlyManualCast>true</onlyManualCast>
      <targetParams>
        <canTargetLocations>true</canTargetLocations>
      </targetParams>
    </li>
  </verbs>
</ThingDef>

<StyleCategoryDef>
  <defName>ExampleStyles_TripleRocketSingleUseConversion</defName>
  <label>converted single-use launchers</label>
  <thingDefStyles>
    <li>
      <thingDef>Gun_TripleRocket</thingDef>
      <styleDef>Style_TripleRocket_SingleUseConversion</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

`Gun_TripleRocket` still self-consumes exactly as it does unstyled: the
style only changed its projectile, range, warm-up, burst, sounds, and
forced-miss behavior. This snippet is a trimmed copy of the full worked
example (with explanatory comments) shipped in
`Example Mod/1.6/Defs/ThingStyleDefs/ExampleStyledSingleUseLauncher.xml`.

`projectileSource` points at a template `ThingDef` whose primary verb is the
`Verb_LaunchProjectile`-derived verb to copy from. The template is never
spawned by the framework; it exists only to hold a verb definition.
`projectileSource` is a weapon `ThingDef`, not a projectile `ThingDef`: the
copied behavior is `Verse.VerbProperties`, and a projectile def only holds
flight/impact/explosion/damage properties, which stay under the template's
own `defaultProjectile`.

```xml
<ThingStyleDef>
  <defName>Helldiver_ChargeRifle</defName>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <projectileSource>Helldiver_ChargeRifle_Template</projectileSource>
    </li>
  </modExtensions>
</ThingStyleDef>

<!--
  Same rules as a beamSource template below: it is a real ThingDef that goes
  through vanilla's own def validation for whatever it inherits from, even
  though it is never spawned. Give it the minimum BaseGun needs.
-->
<ThingDef ParentName="BaseGun">
  <defName>Helldiver_ChargeRifle_Template</defName>
  <label>charge rifle template (unused)</label>
  <tradeability>None</tradeability>
  <generateCommonality>0</generateCommonality>
  <smeltable>false</smeltable>
  <graphicData>
    <texPath>Things/Item/Weapon/Helldiver/ChargeRifle</texPath>
    <graphicClass>Graphic_Single</graphicClass>
  </graphicData>
  <statBases>
    <Mass>4</Mass>
  </statBases>
  <verbs>
    <li>
      <verbClass>Verb_Shoot</verbClass>
      <hasStandardCommand>true</hasStandardCommand>
      <defaultProjectile>Projectile_Helldiver_ChargeRifle</defaultProjectile>
      <range>30.9</range>
      <warmupTime>0.8</warmupTime>
      <burstShotCount>3</burstShotCount>
      <ticksBetweenBurstShots>10</ticksBetweenBurstShots>
      <soundCast>Shot_ChargeRifle</soundCast>
      <soundCastTail>GunTail_Medium</soundCastTail>
    </li>
  </verbs>
</ThingDef>

<StyleCategoryDef>
  <defName>HelldiverChargeWeapons</defName>
  <label>Helldiver charge weapons</label>
  <thingDefStyles>
    <li>
      <thingDef>Gun_ChargeRifle</thingDef>
      <styleDef>Helldiver_ChargeRifle</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

Rules and limits:

- "Everything" means every field of the compatible primary `VerbProperties`,
  exactly as for `beamSource`; this is not a hard-coded list, so any future
  RimWorld `VerbProperties` field that `MemberwiseClone()` copies is included
  automatically. Representative categories: targeting/range (`range`,
  `minRange`, `forceNormalTest`), warm-up/burst (`warmupTime`,
  `burstShotCount`, `ticksBetweenBurstShots`), projectile and forced-miss
  (`defaultProjectile`, `ForcedMissRadius`, `forcedMissEvenDispersal`), and
  effects/sounds/UI (`soundCast`, `soundCastTail`, `soundAiming`,
  `commandIcon`, muzzle flash and other effecters).
- It does **not** override values sourced from the runtime item itself.
  Vanilla equipped-weapon accuracy and cooldown come from the mapped
  weapon's own `Accuracy*` and `RangedWeapon_Cooldown` stats, not from the
  template; graphics, mass, market value, tags, components, tools, and other
  `ThingDef` data also remain on the base styled item. Per-style stat
  substitution is intentionally outside this feature's scope.
- A loaded `CompChangeableProjectile` (ammo) always takes precedence for the
  launched projectile def only. It does not restore the base weapon's range,
  burst, sounds, targeting, or other template-owned settings, and a
  successful shot still consumes ammo through vanilla behavior.
- `projectileSource` is only used by a weapon whose selected primary verb is
  `Verb_LaunchProjectile`, or a subclass that is exactly the same runtime
  verb class as the template's primary verb (for example, a normal gun with
  `Verb_Shoot` requires a template whose primary verb also uses
  `Verb_Shoot`; a plain `Verb_LaunchProjectile` template is not
  interchangeable with it). `Verb_ShootOneUse` (single-use launchers such as
  `Gun_TripleRocket`) follows the same exact-class rule as any other
  `Verb_LaunchProjectile` subclass. It cannot convert a beam, spray, fire,
  ability, or melee verb, and it never changes the instantiated runtime verb
  class.
- `<projectileSource>` and the legacy `<projectile>`/sound fields are
  mutually exclusive on the same `StyleIdentityExtension`; setting both is a
  def-validation error rather than an undocumented partial-merge rule.
- Like `beamSource`, the override is instance-scoped: it clones the
  template's verb properties onto the runtime `Verb` of the exact styled
  item, and never mutates `ThingDef.Verbs` on the base weapon or the
  template. It is a shallow clone, so referenced mutable data inside
  `VerbProperties` is shared with the template; the framework never mutates
  it. Unstyled items, other weapons, and the template itself are unaffected.
- The template's primary verb's `defaultProjectile` must exist, have
  `ProjectileProperties`, and have a `thingClass` derived from
  `Verse.Projectile`, and every mapped weapon's primary verb class must
  exactly match the template's. Invalid or ambiguous configurations fail
  during def validation instead of silently doing nothing.
- The template is a real, non-abstract `ThingDef` for the same reason as a
  `beamSource` template (see below): it is checked against vanilla's own
  `ConfigErrors`, so give it the minimum `BaseGun` needs and consider
  `tradeability=None`/`generateCommonality=0`.

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
- `beamSource` and `projectileSource` are independently optional and are
  each described symmetrically above/below for their respective verb
  family. A style pack may set both where it is mapped to separate
  compatible beam and projectile weapons; each runtime verb only ever uses
  the source that matches its own verb type, so no cross-application
  occurs. The legacy `projectile`/sound fields are likewise independent of
  `beamSource`, but mutually exclusive with `projectileSource` (see
  "Projectile weapon templates" above).
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

## Melee weapons

Melee support lets a style replace a weapon's complete vanilla `<tools>`
list and its weapon-level melee hit sound. The list is a replacement, not an
append: its capacities select vanilla manoeuvres, and its power, cooldown,
armor penetration, extra damage, hit/miss sounds, and battle-log labels use
RimWorld's normal `Tool` behavior.

```xml
<ThingStyleDef>
  <defName>Style_Mace_MonobladeConversion</defName>
  <overrideLabel>monoblade-converted mace</overrideLabel>
  <graphicData>
    <texPath>Things/Item/Equipment/WeaponMelee/Mace</texPath>
    <graphicClass>Graphic_Single</graphicClass>
    <color>(100, 210, 235)</color>
  </graphicData>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <description>A mace rebuilt around a compact vibrating blade. It trades the original crushing head for rapid cutting strikes.</description>

      <!-- Replaces Mace's complete handle/head list; this does not append a tool. -->
      <tools>
        <li>
          <label>vibrating edge</label>
          <capacities>
            <!-- Cut resolves to the vanilla Slash manoeuvre. -->
            <li>Cut</li>
          </capacities>
          <power>18</power>
          <cooldownTime>1.7</cooldownTime>
          <soundMeleeHit>MeleeHit_Metal_Sharp</soundMeleeHit>
          <soundMeleeMiss>Pawn_Melee_Punch_Miss</soundMeleeMiss>
        </li>
      </tools>

      <!-- Weapon-level sound: wins over the tool/material hit sound. -->
      <meleeHitSound>MeleeHit_Metal_Sharp</meleeHitSound>
    </li>
  </modExtensions>
</ThingStyleDef>

<StyleCategoryDef>
  <defName>ExampleStyles_MaceMonobladeConversion</defName>
  <label>monoblade weapons</label>
  <thingDefStyles>
    <li>
      <thingDef>MeleeWeapon_Mace</thingDef>
      <styleDef>Style_Mace_MonobladeConversion</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

This snippet is a trimmed copy of the full worked example (with explanatory
comments) shipped in
`Example Mod/1.6/Defs/ThingStyleDefs/ExampleStyledMeleeWeapon.xml`.

Rules and limits:

- `<tools>` is only used by a `ThingWithComps` mapped (via a
  `StyleCategoryDef`) to a melee `ThingDef` (`ThingDef.IsMeleeWeapon`). An
  omitted `<tools>` (the default) leaves the base weapon's own tools
  completely unchanged; a present `<tools>` list replaces all of them,
  including their capacities/manoeuvres, power, cooldown, armor penetration,
  extra damage, battle-log labels, and hit/miss sounds. Each tool follows
  normal vanilla `Tool` rules: it needs at least one capacity that matches a
  `ManeuverDef`, or it can never produce a usable attack.
- The replacement changes actual melee combat, not just displayed stats: it
  changes which runtime `Verb_MeleeAttackDamage` objects RimWorld builds for
  the equipped instance, so damage, cooldown, AI/player melee-verb selection,
  and the melee debug table all use the replacement tools. It never mutates
  `ThingDef.tools` on the base weapon; unstyled copies of the weapon, other
  styles, and any other instance keep full vanilla tools.
- The item's info card reflects the replacement too: "Melee average DPS" and
  "Melee average armor penetration" (and their tooltip's per-tool breakdown)
  are computed from the styled instance's own tools, not the base weapon's,
  because both stats are re-pointed at the replacement list specifically for
  that equipped `Thing`. Only that specific instance's stat display changes;
  the same stats shown for an unstyled copy, another instance, or the def in
  the abstract (for example, in the crafting bill list) are unaffected.
- `<meleeHitSound>` mirrors vanilla `ThingDef.meleeHitSound`: it overrides
  the selected tool's `<soundMeleeHit>` and the weapon material's sound for
  both pawn and building hits. A target building's own
  `<soundMeleeHitOverride>` still takes priority, exactly as it does in
  vanilla. Omit `<meleeHitSound>` to let the replacement tool or material
  choose the hit sound. `<meleeHitSound>` is independent of `<tools>` and can
  be set with or without it. The framework defines no SoundDefs or audio
  clips of its own; referenced sounds must come from RimWorld, the style
  pack, or one of its dependencies.
- The framework does not add a separate miss-sound field: a replacement
  tool's own `<soundMeleeMiss>` already reaches vanilla's normal miss/dodge
  sound selection, so set it directly on the tool.
- `<tools>` and `<meleeHitSound>` are independent of every projectile and
  beam field above; a style may combine a melee override with any of them if
  it is mapped to multiple compatible weapon defs, though in practice a
  given weapon only ever exercises the field family that matches its own
  verb/tool type.
- Applying, removing, or replacing a style on an equipped weapon rebuilds its
  runtime verbs immediately, and a save/load cycle reconstructs the same
  styled tools deterministically; the equipped pawn never keeps using a
  stale verb from a tool the style no longer supplies.
- Invalid configurations (an empty or malformed `<tools>` list, a tool with
  no capacity matching a `ManeuverDef`, or a mapping to a non-melee
  `ThingDef`) fail during def validation instead of silently doing nothing.

## Stat base overrides

A style can replace selected existing `ThingDef.statBases` values for the
individual styled item, via `statBases`. This is an **absolute replacement**
of the def's base value, not a `+` offset or `x` factor, and an omitted stat
keeps that base completely unchanged. The item's normal stuff/quality
adjustments, component offsets, stat parts, and post-processing all still
run afterward exactly as they do for an unstyled item; only the starting
base is different for this specific styled instance.

```xml
<ThingStyleDef>
  <defName>Helldiver_LaserCannon</defName>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <statBases>
        <RangedWeapon_Cooldown>0.75</RangedWeapon_Cooldown>
        <Mass>3.5</Mass>
      </statBases>
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

A concrete, copyable version of the same idea, using two stats
`Gun_AssaultRifle` already defines (`Mass: 3.5` and
`RangedWeapon_Cooldown: 1.70`):

```xml
<ThingStyleDef>
  <defName>Style_AssaultRifle_LightweightConversion</defName>
  <overrideLabel>lightweight-converted assault rifle</overrideLabel>
  <graphicData>
    <texPath>Things/Item/Equipment/WeaponRanged/AssaultRifle</texPath>
    <graphicClass>Graphic_Single</graphicClass>
    <color>(200, 200, 200)</color>
  </graphicData>
  <modExtensions>
    <li Class="Styled_Identity_Framework.StyleIdentityExtension">
      <statBases>
        <Mass>2.8</Mass>
        <RangedWeapon_Cooldown>1.2</RangedWeapon_Cooldown>
      </statBases>
    </li>
  </modExtensions>
</ThingStyleDef>

<StyleCategoryDef>
  <defName>ExampleStyles_AssaultRifleLightweightConversion</defName>
  <label>lightweight-converted weapons</label>
  <thingDefStyles>
    <li>
      <thingDef>Gun_AssaultRifle</thingDef>
      <styleDef>Style_AssaultRifle_LightweightConversion</styleDef>
    </li>
  </thingDefStyles>
</StyleCategoryDef>
```

An unstyled `Gun_AssaultRifle` still shows `Mass: 3.5 kg` and
`Cooldown: 1.70s` (subject to its own stuff/quality, as usual). A rifle with
`Style_AssaultRifle_LightweightConversion` applied shows `Mass: 2.8 kg` and
`Cooldown: 1.2s` instead, with everything else (accuracy, range, `WorkToMake`,
etc.) unchanged; removing the style reverts it immediately. This is a base
replacement, not a new stat: it works here only because `Gun_AssaultRifle`
already defines both `Mass` and `RangedWeapon_Cooldown` in its own
`statBases`. This snippet targets the real vanilla `Gun_AssaultRifle` so it
works as a standalone copy-paste with no other dependency; the full worked
example (with explanatory comments) shipped in
`Example Mod/1.6/Defs/ThingStyleDefs/ExampleStyledStatBases.xml` applies the
same idea to `Gun_ExampleCarbine`, a small reference weapon that ships with
the example pack itself (`Example Mod/1.6/Defs/ThingDefs/ExampleCarbine.xml`)
so the demonstrated numbers never drift from Core's own.

Rules and limits:

- `statBases` only supports stats whose `StatDef.workerClass` is exactly
  `RimWorld.StatWorker` (the vanilla default), because that is the only
  worker whose base-value semantics ("substitute this number before
  stuff/quality") are generic enough to replace safely. `MarketValue` is
  rejected explicitly (its base is computed by `StatWorker_MarketValue`
  rather than read from `statBases`), and any other stat backed by a custom
  worker is rejected the same way.
- Every mapped target `ThingDef` must already define every overridden stat
  in its own `statBases`. `statBases` can only replace an existing base
  value; it cannot add a stat the item didn't already show on its stat
  card, and doing so is a def-validation error rather than a silent no-op.
- `statBases` is only used by a `ThingWithComps` mapped (via a
  `StyleCategoryDef`) to a styleable, non-pawn `ThingDef`
  (`ThingDef.CanBeStyled()`). Mapping to a non-styleable `ThingDef` is a
  def-validation error.
- The override is instance-scoped: it only ever changes the base seen by the
  exact styled `Thing`, through a small `ThingComp` added to the mapped
  `ThingDef`. Unstyled copies of the weapon, other styles, and any other
  instance keep the original `ThingDef.statBases` value untouched.
- Stuff-made items are handled correctly: the replacement base goes through
  the same stuff/quality factors and offsets the original base would have,
  so a styled item made from a different material or at a different quality
  still gets consistent, proportionate results rather than an oddly-scaled
  one.
- The framework adds no Harmony patch to any `StatWorker`, and does no
  per-tick work; the override is read the same way RimWorld already asks any
  `ThingComp` for a stat offset. Applying, removing, or switching a style
  updates the affected stats immediately, and a save/load cycle preserves
  the same values deterministically.
- Duplicate stat entries, a null/unset stat on an entry, an empty
  `<statBases>` list, `MarketValue`, a custom-worker stat, and a mapping to
  a non-styleable or stat-undefined target all fail during def validation
  instead of silently doing nothing.

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
