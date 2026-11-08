# MapleStory Clone

A 2D multiplayer RPG inspired by MapleStory, built with Unity and C#. The project features real-time combat, character progression, and cooperative gameplay using Netcode for GameObjects.

**[View Project on itch.io](https://yairweizman.itch.io/maple-leaf-saga)**

## Overview

A personal project focused on recreating MapleStory's classic side-scrolling gameplay while implementing multiplayer functionality.

Players can host sessions, invite friends through join codes, and explore the game world together.

## Features

- **Multiplayer:** Player-hosted sessions using Netcode for GameObjects, Unity Relay, and Unity Lobby.
- **Networking:** Synchronized player movement, animations, and combat interactions.
- **Combat:** Real-time attacks, weapons, skills, and visual effects.
- **Inventory:** Item management and equipment systems.
- **RPG Systems:** Character stats, quests, NPC interactions, and enemies.
- **World:** 2D side-scrolling environments with exploration and interactive elements.

## Multiplayer Architecture

The multiplayer system follows a host-client architecture, where one player hosts the session and others connect using a join code.

- **Netcode for GameObjects** handles network synchronization and RPC communication.
- **Unity Relay** enables connections between players without a dedicated game server.
- **Unity Lobby** provides session creation and management.

## Tech Stack

- **Language:** C#
- **Engine:** Unity 2022.3 LTS
- **Networking:** Netcode for GameObjects
- **Services:** Unity Relay, Unity Lobby

## Screenshots

### Gameplay
![MapleStory Gameplay](gameplay.png)

### Multiplayer
![MapleStory Multiplayer](multiplayer.png)

### Inventory
![MapleStory Inventory](combat.png)

### Task System
![MapleStory Task System](combat.png)

## Running the Project

1. Clone the repository.
2. Open the project in Unity Hub using Unity 2022.3.35f1.
3. Open the relevant game scene and enter Play Mode.

Multiplayer features require Unity Gaming Services configuration.
