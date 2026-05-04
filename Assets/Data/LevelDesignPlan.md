# Whisker Tales: 50-Level Puzzle Design & Balancing Plan

This document outlines the structure, difficulty curve, and mechanics for the initial 50 levels of 'Whisker Tales'.

## 1. Difficulty Curve Overview
The difficulty will follow a 'sawtooth' pattern: a gradual increase in difficulty followed by a 'relief' level to maintain player engagement and prevent frustration.

| Level Range | Difficulty Tier | Core Objective | New Mechanics Introduced |
| :--- | :--- | :--- | :--- |
| 1-5 | Tutorial | Basic matching | Basic match-3, score targets |
| 6-10 | Easy | Learning combos | 4-match (Rocket), 5-match (Bomb) |
| 11-20 | Normal | Obstacle clearing | Obstacles (e.g., crates, yarn balls) |
| 21-35 | Hard | Strategic play | Limited moves, complex board shapes |
| 36-50 | Very Hard | Mastery | Combined objectives, multi-layered obstacles |

## 2. Level Data Structure (JSON)
Each level will be defined in a JSON file for easy loading and balancing.

```json
{
  "level_id": 1,
  "grid_size": [8, 8],
  "move_limit": 25,
  "target_score": 1000,
  "objectives": [
    {"type": "collect_item", "item_id": "cat_toy", "count": 10}
  ],
  "obstacles": [],
  "difficulty_rating": 1
}
```

## 3. Balancing Strategy
*   **Move Limit Adjustment:** If a level is too hard, increase the move limit by 10-20%.
*   **Target Score Calibration:** Ensure the target score is achievable within the move limit based on average match values.
*   **Obstacle Density:** Gradually increase the number of obstacles to ramp up difficulty.

## 4. Performance Optimization for Global Devices
*   **Asset Compression:** Use ASTC or ETC2 compression for textures to ensure compatibility with low-end global devices.
*   **Object Pooling:** Implement object pooling for tiles and VFX to reduce memory overhead and prevent lag.
*   **UI Batching:** Minimize draw calls by batching UI elements where possible.
