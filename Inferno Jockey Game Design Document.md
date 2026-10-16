# Inferno Jockey Game Design Document

Locked context for the simplest playable version of Inferno Jockey.

Guidance only, not a build order.

Do not invent systems the core loop does not need.

Do not violate the fairness rule.

---

# Product

Inferno Jockey is a mobile infinite runner built in Unity 6.

It is not a 3-lane runner.

The playfield is a 3×3 grid consisting of 9 lanes.

The player pilots a drone through refinery machinery, shoots and tags raw Kytherium, and stays alive by dodging or clearing obstacles while collecting fuel and replacement laser cores.

Tagging is the in-run action.

Hauling, contracts, and payout are menu systems and remain out of scope until a later request.

---

# Fiction (Context Only)

The player is an independent Inferno Jockey on the molten world of Krneal.

The player's job is to tag Kytherium for collection.

Kytherium is a naturally occurring superconductor found in solid, liquid, and gas forms.

Refinery machinery already extracts it from the lava.

During a run, the player pilots only the drone.

Raw Kytherium forces exposed technology into permanent overdrive.

Speed only increases.

It never decreases during a run.

The physical limits of the machinery prevent speed from increasing beyond a level that would become unplayable.

---

# Simplest Playable Version (In Scope)

- One drone.
- One 3×3 lane grid.
- One tap-based input system.
- Forward auto-scroll.
- Obstacles that must be dodged or shot.
- Raw Kytherium that must be melted and tagged.
- Fuel pickups.
- Ammo pickups.
- Speed increases caused by Kytherium exposure.
- A maximum speed cap.
- Run ends on crash or fuel depletion.
- Drone is destroyed at run end.
- Deterministic safe-lane generation using pi.
- Deterministic ore placement using pi.
- Camera-anchored lane navigation grid.
- Limited lane visibility.

---

# Out of Scope Until Later Requests

- Contracts
- Refinery payout screens
- Hauling simulation
- Drone shop
- Meta progression
- Story scenes
- Multiple drone types
- Anything unrelated to the core run loop

Drone, fuel, and ammo costs should exist as data.

Do not build economy UI unless requested.

---

# Lane Model

Lane numbering uses the project-wide convention:

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

There is no free flight.

---

# Lane Representation

The lane system consists of two separate concepts.

## Logical Grid

The logical grid drives:

- Movement
- Adjacency
- Obstacle placement
- Ore placement
- Fairness validation
- Input resolution

The logical grid exists independently of scene objects.

## Lane Areas

Each lane occupies a visible square area in the 3×3 lane grid.

Each lane consists of:

- A lane identifier (1-9)
- A selectable lane area
- A lane center position

The lane area receives player input.

The lane center defines the drone's occupied position when the drone is in that lane.

The player may tap anywhere inside a lane's area to select that lane.

The exact touch or click position inside a lane is ignored once the lane has been identified.

The drone never moves to the touched point.

The drone always moves to the center position of the selected lane.

## Lane Occupancy

The drone occupies exactly one lane at all times.

The drone's position is aligned to the center position of the occupied lane.

When changing lanes, the destination is always the center position of the destination lane.

The drone never occupies multiple lanes simultaneously.

## Lane Alignment

All lane-based gameplay elements align to the logical lane system.

This includes:

- The drone
- Obstacles
- Ore
- Fuel pickups
- Ammo pickups

Gameplay entities that occupy a lane are considered to occupy that lane's center position for gameplay purposes.

## Lane Selection

The entire visible area of a lane acts as a selection button.

Examples:

- Tap anywhere within Lane 2 → Select Lane 2
- Tap anywhere within Lane 5 → Select Lane 5

Input is resolved at the lane level rather than the exact touch location.

Gameplay behavior must remain identical regardless of the underlying implementation.

Lane identification may be performed through colliders, raycasts, touch detection, mouse detection, or other implementation details.

## Lane Markers

The current implementation contains lane marker GameObjects attached to the camera hierarchy.

Each lane marker corresponds directly to one logical lane.

```text
1  2  3
4  5  6
7  8  9
```

Lane markers provide:

- Lane identification
- Input targeting
- Visual feedback
- Debugging support

The lane marker grid remains fixed relative to the camera.

The lane grid functions as the drone's navigation interface.

---

# Input

There is no separate fire button.

Selecting a lane is the only player input.

Rules:

- Select an adjacent lane → Move the drone to the center position of that lane.
- Select the current lane → Fire.
- Select a non-adjacent lane → Ignore.

Movement is lane-locked.

The selected lane determines the intended destination.

The player never drags, steers, or freely pilots the drone.

There are no virtual joysticks.

There are no swipe gestures.

---

# Ammo

Firing consumes one ammo.

Rules:

- Ammo > 0 → Fire succeeds.
- Ammo = 0 → Fire fails.
- Movement remains available even with zero ammo.

Running out of ammo does not end the run.

Ammo only affects combat and ore processing.

---

# Camera

The game uses a first-person drone camera.

The lane navigation grid remains fixed relative to the camera.

The lane grid functions as the drone's targeting and navigation system.

When changing lanes:

- The logical lane changes.
- The drone moves to the center position of the selected lane.
- The camera follows the drone.
- The lane navigation grid remains fixed relative to the camera.
- World geometry shifts relative to the camera.
- Obstacles, pickups, ore, and refinery structures move relative to the camera.

The selected lane remains visually stationary before and after movement.

The world appears to shift beneath the navigation grid.

This creates the feeling of piloting a drone using an onboard navigation and targeting system rather than moving a cursor around a screen.

---

# Visibility Rules

The view is intentionally tight.

Players should not see the entire playfield simultaneously.

Rules:

- Center lanes provide the best visibility.
- Edge lanes provide reduced visibility.
- Corner lanes provide the least visibility.
- Opposite-side lanes may be completely off-screen.

Hidden lanes still exist.

Hidden lanes still obey all generation rules.

Do not reveal the entire grid for convenience.

Do not add a minimap.

Visibility limitations are a gameplay feature.

---

# Fairness Invariant

The game must never generate an impossible obstacle pattern.

Each obstacle set must leave at least one valid lane.

If generation would block all nine lanes:

- Refinery machinery automatically removes enough obstacles to restore a valid path.

This is a safety mechanism.

It should rarely occur.

A skilled player must always have a survivable route available even at zero ammo.

Failure should only result from:

- Missing movement
- Poor routing decisions
- Failure to collect available fuel

Do not generate:

- Unavoidable crashes
- Impossible walls
- Fuel starvation scenarios without player error

---

# Shooting, Ore, and Tagging

A shot can:

- Clear an obstacle.
- Melt hardened lava around Kytherium.

If the player drone impacts Raw ore it will crash.

Ore must first be melted.

Tagging occurs automatically after successful preparation.

To tag the drone simply flies through the melted ore.

Ore lifecycle:

1. Ore appears.
2. Ore is shot.
3. Hardened lava melts.
4. Ore becomes taggable.
5. Drone flies through ore.
6. Ore is tagged.

---

# Fuel

Fuel is required for flight.

Fuel reaching zero immediately ends the run.

Fuel pickups must appear often enough for a skilled player to maintain flight.

Do not place required fuel pickups exclusively on impossible routes.

---

# Deterministic Pattern

Safe lanes are driven by the first 30 digits of pi including the leading 3:

```text
3 1 4 1 5 9 2 6 5 3
5 8 9 7 9 3 2 3 8 4
6 2 6 4 3 3 8 3 2 7
```

For obstacle pattern index `i`:

```text
SafeLane = PiDigits[i mod 30]
```

The safe lane is guaranteed open.

No digit is zero.

All values are valid lane IDs.

---

# Ore Placement

Ore uses:

```text
OreLane = SafeLane + 7
Wrap past 9
```

| Safe Lane | Ore Lane |
|-----------|-----------|
| 1 | 8 |
| 2 | 9 |
| 3 | 1 |
| 4 | 2 |
| 5 | 3 |
| 6 | 4 |
| 7 | 5 |
| 8 | 6 |
| 9 | 7 |

Rules:

- Safe lane always remains traversable.
- Drops will always spawn in the safe lane.
- Ore lane may contain ore.
- Ore lane may be dangerous.
- Ore lane may require shots.

---

# Run Economy

Every run consumes one drone.

The drone is destroyed when the run ends.

Causes:

- Crash
- Empty fuel

Persist as data:

- Drone cost
- Fuel cost
- Ammo cost

Do not build economy UI unless requested.

---

# Speed Progression

Exposure to raw Kytherium permanently increases speed.

Speed never decreases during a run.

Speed progression stops at the machinery cap.

The cap is designer-controlled.

The cap must be difficult but fair.

---

# Playability-First Development Rule

The game must remain playable throughout development.

Validation comes from:

1. Automated tests.
2. Actual gameplay.

Gameplay testing is considered equally important to code testing.

A mechanic is not proven simply because automated tests pass.

Major mechanics should be evaluated for:

- Responsiveness
- Readability
- Fairness
- Difficulty
- Clarity
- Fun

Prefer playable vertical slices over isolated systems.

The goal is to achieve a fully playable run as early as possible.

Development should minimize the time between writing code and experiencing the result in-game.

---

# Engineering Rules

- Unity 6.
- Mobile-first.
- Test-first development.
- Write the failing test before implementation.
- Prefer pure C# for gameplay rules.
- Keep game rules independent of scenes whenever practical.
- Maintain a playable build throughout development.
- Prefer vertical slices.
- Validate mechanics through gameplay and automated tests.
- Fix gameplay problems even if all tests pass.
- Do not add features not required by current tests or the current playable prototype.

Pure C# systems should include:

- Lane rules
- Adjacency
- Pi sequencing
- Lane wrapping
- Input resolution
- Move vs fire decision logic
- Ore processing
- Speed progression
- Fairness validation
- No-dead-end generation

The first playable scene should demonstrate:

- Lane selection
- Tap-to-move
- Tap-to-fire
- Camera-follow movement
- Obstacle clearing
- Ore melting
- Ore tagging
- Fuel consumption
- Ammo consumption
- Pickup collection
- Speed growth
- Speed cap
- Crash loss condition
- Fuel loss condition

The project should always move toward a complete playable game rather than a collection of disconnected systems.