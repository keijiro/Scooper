# Low-Level Digger

<img width="960" height="540" alt="Screenshot" src="https://github.com/user-attachments/assets/3a4b7518-cb8f-458e-a813-657ee7dd171c" />

**Low-Level Digger** is a sample project that demonstrates how to use the new
Low-Level 2D Physics feature in Unity 6.3.

[Play on Unity Play](https://play.unity.com/en/games/407dcb6e-d06c-4d61-95cb-a153a59f3190/scooper)

## System Requirements

- Unity 6.3

## What Is Low-Level 2D Physics?

**Low-Level 2D Physics** is a new feature in Unity 6.3 that allows you to
access the 2D physics engine (Box2D v3) through a very thin C# wrapper. This
design makes it possible to handle large numbers of physics objects
efficiently, without the overhead typically introduced by the GameObject- and
Component-based abstraction layers.

In this project, for example, the system simulates 1,024 rigid bodies
representing small dirt particles inside the bucket. In the Editor and
Standalone Players, the simulation runs across multiple worker threads, keeping
CPU usage low. Even in Web builds (WebGL/WebGPU), where execution is limited to
a single main thread, the simulation remains performant enough to run smoothly.

## Where Should I Start?

Unity provides an official [sandbox sample project] that showcases the
Low-Level 2D Physics features in a comprehensive way. It is highly recommended
to try this project first to understand what is available and to get a sense of
the performance characteristics.

[sandbox sample project]:
  https://github.com/Unity-Technologies/PhysicsExamples2D/tree/master/PhysicsCore2D/Projects/Sandbox

As a next step, you may consider using AI coding agents with proper guidance.
As described in the [AGENTS.md] file of this repository, you should instruct them
to refer to the latest Unity documentation and the official sample project
above.

[AGENTS.md]: /AGENTS.md

It is also recommended to explicitly mention Box2D v3 in the system prompt for
AI agents (as defined in AGENTS.md). While they may not have specific knowledge
about Unity’s Low-Level 2D Physics API, they generally have a solid
understanding of Box2D itself, and that knowledge transfers well because the
wrapper layer in Unity is very thin.

For further documentation, samples, and community-provided insights, see the
official [Unity Discussions thread].

[Unity Discussions thread]:
  https://discussions.unity.com/t/low-level-2d-physics-in-unity-6-3/1683247

## Brief Project Structure

There are two main categories of physics objects in this project:

- **Dirt particles managed by `DirtManager`** — The small dirt particles are
  handled by a single component called `DirtManager`. These particles (which
  are physics bodies internally) are generated, pooled, and recycled by this
  component. Rendering is handled by a dedicated `DirtRenderer`, which uses a
  specialized vertex shader to apply per-particle transforms to a shared mesh,
  allowing all particles to be drawn in a single draw call.

- **Physics bodies synchronized with GameObjects** — Other physics bodies, such
  as the bucket, gems, scoop, and tray, are handled by two bridge components:
  `DynamicBodyBridge` and `StaticBodyBridge`. These components create and
  manage physics bodies while synchronizing their transforms with GameObjects.
  This approach allows the key interactive elements of the game to be authored
  and controlled using the conventional GameObject-based workflow. In addition,
  the `CompositeShapeBuilder` component provides an editor-friendly way to
  author composite collision shapes.

## Generative AI Usage Disclosure

Some assets and code in this project were created with the assistance of
generative AI tools.

- **Sprite assets** (located in the `Assets/Sprites` folder) were generated
  using the Gemini image generation feature (Nano Banana).
- **Source code** was developed and finalized by the author; however, OpenAI
  Codex CLI was used during the development process for code generation,
  refactoring suggestions, and exploratory prototyping. All generated outputs
  were reviewed, modified, and integrated manually.
