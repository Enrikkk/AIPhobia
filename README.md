# AIPhobia

[cite_start]**Developer:** Enrique Hernández Noguera [cite: 2]

## Overview
[cite_start]**AIPhobia** is a completely AI-based game centered around a ghost hunting theme[cite: 3, 8]. [cite_start]While drawing primary inspiration from the popular horror game *Phasmophobia* [cite: 14][cite_start], AIPhobia streamlines the experience into a more direct, combat-oriented single-scenario encounter[cite: 19, 21, 23]. [cite_start]Rather than using realistic horror graphics, the project targets a more colorful, low-poly, and cartoonish "Ghostbusters" art style[cite: 15, 16, 73]. 

[cite_start]To optimize the estimated 100-hour development cycle [cite: 79] and avoid spending excessive hours building a map from scratch, the game environment repurposes the layout from the classic Unity "John Lemon's Haunted Jaunt" tutorial. [cite_start]This allows the primary development focus to remain on AI systems, physics mechanics, and game design[cite: 80, 81, 82].

## Core Gameplay & Objectives
[cite_start]The game is built upon an asymmetric struggle between two entities: the Ghost Hunter and the Ghost[cite: 26]. [cite_start]The player can choose to control either character, while a real-time autonomous AI agent controls the opponent[cite: 38, 40].

* [cite_start]**The Ghost Hunter:** The hunter's objective is to explore the haunted house, interact with elements to find the ghost, and successfully capture it using a specialized ghost vacuum[cite: 10, 27]. [cite_start]However, the hunter must act quickly, as their "fear levels" will constantly increase over time and spike when subjected to scares[cite: 11]. [cite_start]The hunter utilizes a linear equipment loadout, primarily a flashlight and the vacuum tool[cite: 20].
* [cite_start]**The Ghost:** The ghost's objective is to protect its home by terrifying the hunter into fleeing[cite: 11, 12, 28]. [cite_start]The ghost can scare the player through poltergeist activity (moving objects, closing doors, turning on lights), floating through walls, or physically appearing in front of the player—a high-risk move that maximizes fear but leaves the ghost vulnerable to the vacuum[cite: 31].

## Technical Implementation: Under the Hood
[cite_start]The project is being developed using Unity as the game engine, with Visual Studio/VS Code for C# scripting, and GitHub for version control[cite: 70, 71, 72]. [cite_start]Additional assets are processed using Blender and Audacity, with Notion tracking project management[cite: 73, 74, 75].

### 1. The Tool Management System
The player's loadout is controlled by a centralized `ToolChanger` script. The system listens for numeric keystrokes (1, 2, 3) to synchronize the active 3D tool models with a 2D Canvas hotbar. When a tool is deactivated, it gracefully disables its localized components (stopping particles and audio) to prevent memory leaks and overlapping logic.

### 2. The Lantern Tool
The Lantern acts as the player's primary navigational aid. 
* **State Memory:** Because the entire Lantern GameObject is enabled/disabled by the Tool Manager, the child light source inherently remembers its last state (On or Off) when the tool is re-equipped.
* **Audio:** It uses the `PlayOneShot` method to trigger distinct mechanical "click" sounds for powering up and powering down without interrupting other ambient audio.

### 3. The Vacuum Tool (Core Mechanic)
The Vacuum is the mechanical centerpiece of the project, designed with a heavy emphasis on "Game Feel" and physics manipulation.
* **Raycasting & Targeting:** When activated, the vacuum fires an invisible Raycast from the center of the camera. 
 
This ray calculates the exact distance between the player and the target, generating a normalized `distanceMultiplier` (1.0 at point-blank range, scaling down at a distance).
* **Procedural Animation & Audio:** Instead of static keyframed animations, the script uses `Random.insideUnitSphere` to rapidly calculate random localized vectors, creating a procedural "rumble." A mathematical smoothing function (`Mathf.MoveTowards`) creates a realistic "spin-up" and "spin-down" effect that dynamically bends the pitch and volume of the vacuum's `AudioSource`.
* **Particle VFX:** A custom Particle System simulates a swirling vortex utilizing volume-based emission, orbital velocity, and negative radial velocity to enforce a tight suction cone shape.

### 4. The Universal `Vacuumable` Physics System
To prevent redundant code, interactions are handled by a single `Vacuumable` master script attached to interactable GameObjects. 
* **Rigidbodies (Furniture):** If a `Rigidbody` is detected, the script calculates a directional vector toward the player and applies raw force (`AddForce`). This allows Unity's internal physics engine to handle gravity and Linear Damping (friction), resulting in objects realistically dragging and scraping across the floor.
* **Transforms (Entities/Ghosts):** If no Rigidbody is present, the script defaults to a `Vector3.MoveTowards` translation to pull the entity through the air.

## Current State and Known Issues (Midterm Milestone)
[cite_start]As of the Midterm milestone (February 27), the project has successfully achieved its "Graybox" prototype goals[cite: 87, 91]. [cite_start]The basic gameplay loop is functional, the player can move, the flashlight operates, the ghost can randomly wander via pathfinding, and the foundational catch mechanics exist[cite: 92, 93, 94, 95]. 

However, the project is currently in a heavy tuning and debugging phase:
* ⚠️ **Physics Pulling (Work in Progress):** The logic for vacuuming physics-based furniture is implemented, but the exact interaction between the vacuum's applied force multiplier and Unity's Linear Damping/Mass properties is currently unstable. Objects may not move at the intended speeds and require further tuning.
* ⚠️ **Entity Combat (Untested Logic):** The math to drain a Ghost's Health Points (HP) based on proximity has been fully written into the `Vacuumable` script. However, this "tug-of-war" combat mechanism has not yet been playtested against the live enemy AI in the build. The logic is present, but the gameplay loop requires practical execution.

## Future Development Roadmap
Moving forward, the development timeline is structured around the following milestones:

* [cite_start]**March 27 - The "Brain" Update:** Implementation of robust Finite State Machines for the AI[cite: 97, 98, 99]. 
[cite_start]The Ghost AI will transition between Idle, Stalk, Spook, and Flee states based on sensory input, while the Hunter AI will feature Explore, Chase, Inspect, and Attack modes[cite: 100, 101]. [cite_start]Furthermore, a dynamic experience management system is planned to adjust AI aggression based on player performance[cite: 50, 52, 53, 55, 56, 57, 58, 59].
* [cite_start]**April 24 - The "Juice" Update:** Integration of final low-poly models, textures, critical audio feedback loops, and UI elements (Fear Meter, HUD, Menus)[cite: 102, 103, 104, 105, 106]. [cite_start]Initial development of a multi-agent system may begin here[cite: 107].
* [cite_start]**May 6 - Polish & Balancing:** The final weeks will focus strictly on numerical balancing (fear meter fill rates, ghost difficulty) and finalizing any multi-agent interactions[cite: 108, 109, 110, 111, 112].

[cite_start]**Scope Management:** If the project falls behind schedule, planned scope reductions include cutting the dynamic difficulty balancing, relying on basic polygonal art rather than custom assets, and completely abandoning the multiplayer/multi-agent systems[cite: 113, 114].

---
*Developed as an exploration of advanced Unity mechanics, physics manipulation, and artificial intelligence in game design.*
