
# Propeller Dweller

An AR game where you fight off hordes of enemies that try to get to you while using features from ARCore, such as Image Tracking, Occlusion and Plane Tracking.


## Authors

- [Asier Ulloa García-Obledo](https://github.com/AsiGamer29)
- [Aniol López Ortega](https://github.com/Aniolobolo)
- [Isaac Ramírez Prieto](https://github.com/Bekun67)
- [Clara Rodríguez Moreno](https://github.com/Kopeke4)
- [Xavier Chaparro Foyo](https://github.com/XaviFast05)



## The Game

The game can ONLY be played in Android devices, as it uses Unity's AR Foundation, exclusive to Android.

### Enemies

In this game, there are enemies that try to get close to you, the player, to defeat you. These enemies have three possible types that they can spawn as, these being fire, water and plant. Each type has a weakness, and enemies can only be defeated by the projectile of the type they are weak to. The types' interactions are:

- Fire: Weak to water
- Water: Weak to plant
- Plant: Weak to fire

### Projectiles

To combat the enemies, there are also three possible types of projectiles, which are the same as the enemies. The player can change projectiles by scanning images with their phone. These images correspond to each type, and depending on what image has been detected, the player's projectiles will change to the type that the image represents.

### Winning and losing

To win the game, the player has to defeat a certain amount of enemies before they get too close to the player. There is no time limit, and the enemies move slowly, so the player can have enough time to react before it's too late.


## Features

### Image Tracking

This game uses image tracking to change the player's projectiles to a type that is strong against the enemy that they want to defeat. The game detects the image, and according to the type it's assigned to, it spawns a particle and changes the projectile to that type.

### Occlusion

This game uses occlusion to shoot projectiles with physics. These projectiles have gravity and detect surfaces and enemies, making them defeat enemies if their type is strong against the enemy's type.

### Plane Tracking

This game uses plane tracking to spawn enemies to avoid spawning enemies in wrong places or have weird perspectives. This works by detecting the space that the camera shows in real time. It doesn't work perfectly, but the enemies spawn in places where the player would expect them to do.
## Images



