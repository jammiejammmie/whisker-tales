import json
import os

def generate_levels(count):
    levels = []
    for i in range(1, count + 1):
        difficulty = (i - 1) // 10 + 1
        level = {
            "level_id": i,
            "grid_size": [8, 8],
            "move_limit": 20 + (i // 2),
            "target_score": 500 * i,
            "objectives": [
                {"type": "collect_item", "item_id": "cat_toy", "count": 5 + i}
            ],
            "obstacles": [],
            "difficulty_rating": difficulty
        }
        # Add obstacles for higher levels
        if i > 10:
            level["obstacles"].append({"type": "crate", "count": i - 10})
        levels.append(level)
    return levels

if __name__ == "__main__":
    os.makedirs("/home/ubuntu/WhiskerTales/Assets/Data/Levels", exist_ok=True)
    level_data = generate_levels(50)
    for level in level_data:
        with open(f"/home/ubuntu/WhiskerTales/Assets/Data/Levels/level_{level['level_id']:02d}.json", "w") as f:
            json.dump(level, f, indent=2)
    print("50 levels generated successfully.")
