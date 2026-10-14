# Inferno Jockey Game Design Document

Locked context for the simplest playable version of Inferno Jockey.

Guidance only, not a build order.

Do not invent systems the core loop does not need.

Do not violate the fairness rule.

---

# Product

Inferno Jockey is a mobile infinite runner in Unity 6.

It is not a 3-lane runner. The playfield is a 3×3 grid of 9 lanes.

The player pilots a drone through refinery machinery, shoots and tags raw Kytherium, and stays alive by dodging or clearing obstacles while collecting fuel and laser cores.

Tagging is the in-run action.

Hauling, contracts, and payout live in the menu and stay out of scope until a later request.

---

# Fiction (Context Only)

The player is an independent Inferno Jockey on the molten world of Krneal, tagging Kytherium for collection.

Kytherium is a natural superconductor found in solid, liquid, or gas form. Refinery machinery already harvests it from the lava.

In the run, the player only pilots the drone through tight refinery machinery.

Raw Kytherium forces exposed technology into permanent overdrive.

Speed only goes up.

It never resets during a run.

A machinery cap stops the climb at a speed that is extremely difficult but still playable.

---

# Simplest Version — In Scope

- One drone on a 3×3 lane grid.
- One input: tap a lane to move or fire.
- Forward auto-scroll. The player does not control forward speed directly.
- Obstacles that must be dodged or shot clear.
- Raw Kytherium that must be shot to melt hardened lava, then tagged.
- In-run fuel and ammo (replacement laser cores).
- Permanent speed increase on Kytherium exposure, with a hard cap.
- A run that ends on crash or empty fuel.
- The drone is destroyed at the end of every run.
- Deterministic safe-lane and ore-lane placement from pi, defined below.
- Tight camera with lane-limited visibility.
- HUD-based lane selection and camera recentering.

---

# Out of Scope Until a Later Request

Contracts, quests, refinery payout UI, haul simulation, drone shop, meta progression, story scenes, multiple drone types, and anything that does not directly support the run loop above.

Drone cost, fuel cost, and ammo cost should exist as data.

Do not build economy UI unless requested.

---

# Lane Model

Number the lanes row-major.

This numbering is the project convention:

```text
1  2  3
4  5  6
7  8  9
```

Adjacent means Chebyshev distance 1.

This includes:

- Horizontal movement
- Vertical movement
- Diagonal movement

Neighbor counts:

- Corner lane: 3 neighbors
- Edge lane: 5 neighbors
- Center lane: 8 neighbors

Movement is discrete and lane-locked.

No free flight exists.

The lane grid is not world geometry.

The lane grid is a screen-space navigation HUD that maps player input to lane positions.

---

# Input

There is no separate fire button.

Tapping a lane is the only player input.

The 3×3 lane grid is displayed as a HUD overlay that remains fixed relative to the screen.

The grid represents the drone's available movement lanes.

Rules:

- Tap an adjacent lane: move there.
- Tap the lane currently occupied by the drone: fire.
- Tap any non-adjacent lane: do nothing.

Movement is lane-locked and discrete.

The drone transitions between valid lane positions but does not freely fly through open space.

Firing spends one ammo.

Ammo behavior:

- If ammo > 0, tapping the occupied lane fires.
- If ammo = 0, tapping the occupied lane does nothing.
- The player can still move while out of ammo.

The lane grid is always the authoritative input surface.

Players move by tapping their destination lane directly.

Do not add joystick movement, swipe controls, virtual directional buttons, or free-flight steering.

---

# Camera

The game is presented as a first-person drone camera.

The visible 3×3 lane grid is rendered as a HUD overlay on top of the drone camera feed.

The HUD remains fixed relative to the screen.

When the player changes lanes:

- The drone's logical lane changes.
- The camera shifts to center the selected lane.
- The HUD remains stationary.
- World geometry, obstacles, pickups, and ore move relative to the camera as it recenters.

This creates the illusion that the player is piloting the drone camera directly while still preserving lane-based gameplay.

The camera view remains intentionally tight.

Visibility rules:

- A player in an outer lane cannot see the full 3×3 playfield.
- Opposite-side lanes may be completely outside the visible area.
- Hidden lanes continue to exist and follow all generation and fairness rules.
- Do not reveal the entire grid as a convenience feature.
- Do not add a minimap.

Lane visibility is a gameplay constraint, not a technical limitation.

---

# Fairness Invariant

There is never a no-win obstacle set.

Every obstacle set must leave at least one open lane.

If generation would block all 9 lanes, refinery equipment automatically clears enough obstacles to restore a valid path.

This is a safety net, not the normal gameplay path.

A skilled player must always have a legal dodge option, including with zero ammo.

Failure must come from:

- Missing a move
- Choosing a poor route
- Running out of fuel after failing to collect available fuel

Do not generate:

- Unavoidable walls
- Impossible gaps
- Fuel starvation scenarios with no prior player mistake

---

# Shooting, Ammo, and Fuel

Fuel is required to keep flying.

Running out of fuel immediately ends the run.

Running out of ammo does not end the run.

Ammo is spent only when performing a fire action by tapping the occupied lane.

Without ammo:

- Obstacles cannot be cleared.
- Ore cannot be prepared.
- Movement remains fully available.

A shot clears an obstacle occupying the current lane's firing path.

For Kytherium:

- A shot melts the hardened lava surrounding the ore.
- Only melted ore can be tagged.
- Tagging occurs automatically as the next interaction step.
- Tagging is not a second button press.
- Unmelted ore cannot be collected.

Fuel and ammo pickups must appear on survivable routes frequently enough that a skilled player can maintain flight and continue using the laser.

Do not place required pickups exclusively on blocked lanes.

---

# Deterministic Pattern

Safe lanes are determined by the first 30 digits of pi, including the leading 3:

```text
3 1 4 1 5 9 2 6 5 3
5 8 9 7 9 3 2 3 8 4
6 2 6 4 3 3 8 3 2 7
```

For obstacle set index `i`:

- Use digit `(i mod 30)`.
- That digit becomes the guaranteed open lane.

No digit is 0.

All values are valid lane numbers 1-9.

Ore spawns at:

```text
Ore Lane = Safe Lane + 7
Wrap after 9
```

| Safe Lane | Ore Lane |
|------------|------------|
| 1 | 8 |
| 2 | 9 |
| 3 | 1 |
| 4 | 2 |
| 5 | 3 |
| 6 | 4 |
| 7 | 5 |
| 8 | 6 |
| 9 | 7 |

Additional lanes may contain obstacles.

Rules:

- Obstacles may not block the safe lane.
- The ore lane may contain ore requiring melt-then-tag interaction.
- The ore lane may be dangerous.
- The safe lane must remain traversable.

---

# Run Economy

Every run consumes one drone.

The drone is destroyed when the run ends, regardless of cause.

Valid causes:

- Crash
- Empty fuel

Drone cost, fuel cost, and ammo cost should exist as data definitions.

Do not build economy UI unless requested.

Speed permanently increases when the drone is exposed to raw Kytherium.

Speed never decreases during a run.

Speed growth is limited by a machinery cap.

The cap is a designer-controlled constant.

It must feel severe but remain playable.

---

# Playability-First Development Rule

The project must remain playable throughout development.

Validation comes from two sources:

1. Automated tests.
2. Actual gameplay.

Whenever practical, new features should be implemented in a form that allows immediate testing inside a playable scene.

A mechanic is not fully validated simply because automated tests pass.

It should also be evaluated through gameplay for:

- Responsiveness
- Readability
- Difficulty
- Fairness
- Player understanding
- Overall feel

Prefer vertical slices over isolated systems.

The goal is to achieve a playable run as early as possible and to keep the game playable throughout development.

Development should optimize for a short feedback loop between writing code and experiencing the result in gameplay.

---

# Engineering Rules

- Unity 6.
- Mobile-first.
- Test-first development.
- Write the failing test before writing implementation code.
- Prefer pure C# for:
  - Lane rules
  - Adjacency checks
  - Pi sequencing
  - Lane wrapping
  - Tap resolution
  - Move/fire/ignore decisions
  - Melt-then-tag behavior
  - Speed progression
  - Fairness validation
  - No-dead-end generation checks
- Keep game rules independent of scene dependencies whenever practical.
- Maintain a playable scene throughout development whenever possible.
- Prefer vertical slices that can be played and evaluated.
- The first goal is achieving a complete playable run, not system completeness.
- Use temporary visuals if necessary to preserve playability.
- Every major mechanic should be verified through:
  - Automated tests
  - Direct gameplay testing
- If gameplay testing reveals a problem, fix it even if automated tests pass.
- Do not add features that the current tests or playable prototype do not require.

The first playable scene should prove:

- HUD lane selection
- Tap-to-move
- Camera recentering
- Tap-to-fire
- Obstacle clearing
- One obstacle set
- One melt-and-tag interaction
- Fuel consumption
- Ammo consumption
- Pickup collection
- Speed escalation
- Speed cap enforcement
- Crash failure
- Fuel depletion failure

The project should always move toward a playable game, not merely a collection of tested systems.