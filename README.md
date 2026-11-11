# HollowSpirit

A short souls-like set at a ruined fortress gate: fight through it, light the ember shrine and face its guardian.

Work in progress. There is no playable build yet.

## Controls

| Action | Keyboard and mouse | Gamepad |
|---|---|---|
| Move | W A S D | Left stick |
| Camera | Mouse | Right stick |
| Light attack | Left mouse button | West button or right bumper |
| Roll | Space | East button |
| Lock on | Middle mouse button or Q | Right stick press |

## About

I'm making this as a portfolio project: a compact, complete souls-like area with regular enemies, a checkpoint shrine, a shortcut and a boss.

So far it has a player with a two-hit light attack chain, a roll with invulnerability frames, stamina and a lock-on camera, and a skeleton soldier that sees, chases and attacks the player. The soldier flinches when hit during its wind-up. The player can be staggered, die and get back up at the start of the arena.

Built with Unity 6 and URP. The soldier's decisions come from a Unity Behavior graph and it moves on an AI Navigation NavMesh. Cameras use Cinemachine, input uses the Input System, and the gray-box arena is built with ProBuilder. Game rules such as the player action state machine, poise and stamina are plain C# covered by EditMode tests.

## Running the project

1. Open the project with Unity 6000.6.0f1.
2. Import these free Asset Store packs from the Package Manager (My Assets). They are not in the repository because their license does not allow redistribution:
   - RPG Character Mecanim Animation Pack FREE (Explosive)
   - RPG Animations Pack FREE (DoubleL)
   - Dungeon Skeletons Demo (Polygon Blacksmith)
   - GUI Parts (PONETI)
3. Open `Assets/_Project/Scenes/Boot.unity` and press Play. The arena loads on top of it.

The skeleton model is set to a Humanoid rig on import by an editor script in the project, so no manual import step is needed.

The WebGL build used by GitHub Pages will be in the `docs` folder.

## Credits

| Asset | Author | License |
|---|---|---|
| RPG Character Mecanim Animation Pack FREE | Explosive | Unity Asset Store Standard EULA |
| RPG Animations Pack FREE | DoubleL | Unity Asset Store Standard EULA |
| Dungeon Skeletons Demo | Polygon Blacksmith | Unity Asset Store Standard EULA |
| GUI Parts | PONETI | Unity Asset Store Standard EULA |

Code, gray-box level and gameplay data were made for this project.
