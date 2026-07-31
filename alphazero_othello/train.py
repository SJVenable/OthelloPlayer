import sys
import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from OthelloNet import OthelloNet
from trainer import Trainer

# ── Training settings ──────────────────────────────────────────────────────
ITERATIONS         = 50    # number of self-play → train → evaluate cycles
GAMES_PER_ITER     = 70    # self-play games generated per iteration
SIMULATIONS        = 70    # MCTS simulations per move (higher = stronger but slower)
EVAL_GAMES         = 50    # evaluation games per iteration (more = more reliable signal)
LEARNING_RATE      = 0.0005
CHECKPOINT_DIR     = 'checkpoints'
# ───────────────────────────────────────────────────────────────────────────

if __name__ == "__main__":
    net = OthelloNet()

    # Resume from best checkpoint if one exists
    import torch
    best = os.path.join(CHECKPOINT_DIR, 'best.pt')
    if os.path.exists(best):
        print(f"Resuming from {best}")
        net.load_state_dict(torch.load(best, weights_only=True))
    else:
        print("Starting from scratch")

    trainer = Trainer(
        net,
        lr=LEARNING_RATE,
        iterations=ITERATIONS,
        games_per_iteration=GAMES_PER_ITER,
        simulations=SIMULATIONS,
        eval_games=EVAL_GAMES,
        checkpoint_dir=CHECKPOINT_DIR,
    )

    trainer.run()
    print("\nTraining complete. Run play.py to play against the trained network.")
