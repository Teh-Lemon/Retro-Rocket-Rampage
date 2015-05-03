# Retro Rocket Rampage
## An offline multiplayer arcade racing game
-----
Created by the team of 3, Gecko Faction Games, for the final year module "Commercial Games Development".

The assignment was to create an offline multiplayer racing game based off the Bloodhound SSC project. Throughout the project the lecturers, who played the role of publishers, must be kept 'happy'. The game was targeted at Windows PC and the in-house arcade machine, running at 60 fps.

Download here: https://github.com/Teh-Lemon/Retro-Rocket-Rampage/releases
-----
## Technical Details
* The game was built using C# and the Microsoft XNA framework.
* Visual Studio was the IDE used and SVN was used for group collaboration
* The game features:
  * Offline multiplayer with support for 1-4 players
  * Random vehicle generation
  * Procedurally generated race tracks
  * Power ups including
    * Power ups that altered the player vehicle
    * Power ups that altered the effect and visuals of the environment
  * Particle effects using the [Dynamic Particle System Framework](http://www.xnaparticles.com/)
  * Bloom post processing
  * Configurable through a settings file
    * Including controls, window resolution and audio levels
  * Sound effects and music
  * A dynamic camera which adjusts to the player's position
  * Support for both keyboard and gamepad
  * 3D models built from the vertex up using our custom 3D model files

## About the game
* 2-4 players race to the end of the track, with the one who scored the most points crowned as the winner.
* Scores are earned by:
  * Winning each race
  * Holding onto the flag
  * Knocking other players off the track
* Score is lost when:
  * Being knocked off/falling off the track
  * Being blown up by the bomb
* Player can steer their vehicle using the side rockets and the boost rocket
  * Player can boost forwards, backwards or to the side depending on which side rockets are turned on when the player boosts
* See Documentation/InterfaceDesignDocumentation.pdf for the full controls
* Power ups can be picked up off the track. They include:
  * Terrain power up. Randomly alter the environment
  * Scale power up. Randomly alter the size and weight of your vehicle
  * Boost power up. Get a refill on your boost bar
* Players can randomly generate a new vehicle before the first race begins with the boost key
* Players can generate a new track if all 4 players hold their side rocket key
* The game starts once the countdown has reached 0
* Each game consists of 3 races
* The side ramps are removed from the track for the 3rd race
* The different environments activated by the terrain power up are:
  * City: Default, no effect
  * Sun: Every player has an infinite boost bar
  * Bounce: Surface of the track becomes bouncy
* Capture the flag by knocking into the player holding onto it
* The bomb power up will detonate after a certain period of time, killing all players near it
  * Pass on the bomb to other players by knocking into them
-----
## Screenshots
![](https://dl.dropboxusercontent.com/u/15765996/Images/Retro%20Rocket%20Rampage/2015-05-02_23-15-54.png)
![](https://dl.dropboxusercontent.com/u/15765996/Images/Retro%20Rocket%20Rampage/2015-05-02_23-18-10.png)
![](https://dl.dropboxusercontent.com/u/15765996/Images/Retro%20Rocket%20Rampage/2015-05-02_23-15-21.png)
![](https://dl.dropboxusercontent.com/u/15765996/Images/Retro%20Rocket%20Rampage/IMAG1059.jpg)

  