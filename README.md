# Unity Kitchen Interaction Prototype

A small-scale Unity gameplay prototype focused on **interaction systems** and **workplace simulation**, built as a playable vertical slice rather than a full commercial game.

This project demonstrates how core gameplay loops, UI feedback, and system architecture can support a time-pressured work scenario inside a compact kitchen environment.

---

## 🎮 Project Overview

The player takes on a temporary kitchen job and performs basic food-preparation tasks under time pressure.

Core gameplay loop:

1. Read incoming orders from the order board  
2. Take ingredients from storage  
3. Chop ingredients at the prep table  
4. Submit prepared items to fulfill orders  
5. Earn or lose money based on performance  
6. End the shift and view settlement results  

The prototype prioritizes **clarity, responsiveness, and system readability** over content scale.

---

## 🧩 Key Features

### Order System
- Randomized ingredient orders with required quantities  
- Time-limited orders with countdown pressure  
- Automatic penalties when orders time out  
- Real-time order board UI updates  

### Food Preparation (Chopping)
- Ingredient-based chopping interactions  
- Fixed cut counts per ingredient (defined in `ItemDef`)  
- Waste tracking when:
  - Exiting mid-process
  - Submitting incorrect ingredients  

### Economy & Work Session
- Base pay per shift  
- Bonus per completed order  
- Penalty per wasted ingredient or timed-out order  
- End-of-shift settlement summary  

### Story Introduction
- Image-only story intro shown at game start  
- Click to advance through images  
- Final click enters gameplay (does not start work automatically)  
- Designed to provide narrative context without interrupting flow  

---

## 🗂️ Script Structure

Scripts are organized by responsibility to keep systems modular and easy to expand.
```text
Scripts/
├─ Core/
│ └─ WorkSession.cs
│ - Manages work states, session lifecycle, and earnings
│
├─ Systems/
│ └─ OrderManager.cs
│ - Handles order generation, timers, submission, and penalties
│
├─ Gameplay/
│ └─ ChoppingController.cs
│ - Controls food preparation logic and player input
│
├─ UI/
│ ├─ OrderBoardUI.cs
│ │ - Displays current order, quantity, and countdown timer
│ └─ StoryIntroUI.cs
│ - Handles image-based story introduction
│
└─ .keep
```
This structure reflects common practices in small-to-mid scale Unity projects and is intended to remain readable for technical review.

---

## 🛠️ Technical Notes

- Engine: Unity (HDRP)
- Input: Unity Input System
- UI: Unity UI + TextMeshPro
- Architecture: MonoBehaviour-based, single-scene prototype
- Focus: Gameplay logic, interaction flow, and system design

This repository intentionally includes **scripts only**.  
Scenes, prefabs, models, textures, and audio are excluded to keep the repository lightweight and focused on code structure.

---

## 🎯 Purpose of This Prototype

This project was created to:

- Explore **workplace simulation gameplay** at a small, focused scale  
- Serve as a **funding application prototype**  
- Act as a **technical portfolio example** demonstrating gameplay systems  

It is not intended as a finished product, but as a foundation that can be expanded with:

- Additional kitchen roles (chef, dishwasher, cleaner)  
- More complex order chains and dependencies  
- Narrative progression  
- Difficulty scaling and performance evaluation  

---

## 🚧 Current Status

- Core gameplay loop: Implemented  
- UI and interaction flow: Functional  
- Content scope: Intentionally limited  
- Development status: Ongoing  

---

## 📌 Author

Developed by **ImLeiSu**

This project represents an early-stage independent Unity gameplay prototype focused on interaction systems and systemic design.


