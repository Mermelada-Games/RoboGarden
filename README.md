# RoboGarden

> **A Cooperative Multiplayer Networking Project**

## Description

**RoboGarden** is a cooperative *party game* developed in Unity where **two players** control small **gardener robots**. Their goal is to **manage and organize all the seeds** in a large garden, ensuring that each seed reaches its correct location to **grow and bloom**.

To achieve this, players must **coordinate strategically**, as many tasks require both of them to work together and depend on each other to complete them.

This project focuses on the implementation of a **Custom Networking Solution** based on UDP to handle World State Replication.

---

## Gameplay Features

The game revolves around a logistics assembly line where coordination is key.

* **Box Management:**
    * **Box Generation:** Spawn boxes dynamically into the world.
    * **Lid Generator:** Create and attach lids to seal the boxes.
* **Labeling System:** Use the label dispenser to tag boxes correctly.
* **Logistics & Routing:** Interact with buttons to switch conveyor belts and select the final destination for each package.
* **Inspection:** View detailed information of the generated boxes (contents, destination, ID).

---

## Controls

| Action | Key | Description |
| :--- | :---: | :--- |
| **Move** | `W`, `A`, `S`, `D` | Move the robot around the factory. |
| **Jump** | `Space` | Jump over obstacles or gaps. |
| **Interact** | `E` | Press buttons, take labels, or grab objects. |

---

## Network Architecture (Technical Overview)

This project implements a custom networking engine from scratch using **UDP Sockets**. The system ensures a consistent **World State Replication** across multiple clients.


### Core Requirements Implemented:

1.  **UDP Communication:**
    * Fast, connectionless data transmission suitable for real-time multiplayer movement.
    
2.  **Replication Model:**
    * **Client-Server Architecture:** The server maintains the authority over the game state and broadcasts updates to connected clients (supporting min. 2 clients).
    * **World State Replication:** Synchronization of positions, rotations, and object states (e.g., box status, button activation).

3.  **Replication Manager:**
    * A centralized system that orchestrates all network entities.
    * Handles object instantiation, destruction, and state updates based on network IDs.

4.  **Custom Replication Packet:**
    * Serialized bit-stream packets designed for efficiency.
    * Supports multiple data types.
---

## Team Members

* **[Guillem Alqueza](https://github.com/guillemalqueza)**
* **[Sergio Fernández](https://github.com/Serfercont)**
* **[Carlos González](https://github.com/gosu00)**
* **[Miguel Iglesias](https://github.com/MiguelIglesiasAbarca)**
