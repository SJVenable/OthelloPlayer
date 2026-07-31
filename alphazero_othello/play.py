import numpy as np
import torch
import sys
import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from othello_game import OthelloGame
from OthelloNet import OthelloNet
from mcts import MCTS


def get_human_move(game):
    valid = game.get_valid_moves()
    while True:
        try:
            col = input("Enter column (A-H): ").strip().upper()
            row = input("Enter row (1-8): ").strip()
            c = ord(col) - ord('A')
            r = int(row) - 1
            action = r * 8 + c
            if 0 <= r < 8 and 0 <= c < 8 and valid[action]:
                return action
            print("Invalid move, try again.")
        except (ValueError, IndexError):
            print("Invalid input, try again.")


def play(checkpoint_path, simulations=100):
    net = OthelloNet()
    net.load_state_dict(torch.load(checkpoint_path, weights_only=True))
    net.eval()

    game = OthelloGame()
    mcts = MCTS(net, simulations=simulations)

    print("\nYou are Black, AI is White.")
    print("Columns are A-H, rows are 1-8.\n")
    game.display()

    while not game.is_terminal():
        if game.current_player == 1:
            print("Your turn (Black)")
            if not game.has_valid_move(game.board, 1):
                print("No valid moves — your turn is skipped.")
            else:
                action = get_human_move(game)
                game.make_move(action)
        else:
            print("AI thinking...")
            policy = mcts.get_policy(game, temperature=0)
            action = np.argmax(policy)
            r, c = divmod(action, 8)
            col_letter = chr(ord('A') + c)
            print(f"AI plays {col_letter}{r + 1}")
            game.make_move(action)

        game.display()

    result = game.get_result()
    black, white = game.get_score()
    print(f"Game over! Black: {black}  White: {white}")
    if result == 1:
        print("You win!")
    elif result == -1:
        print("AI wins!")
    else:
        print("Draw!")


if __name__ == "__main__":
    checkpoint = sys.argv[1] if len(sys.argv) > 1 else 'checkpoints/best.pt'
    if not os.path.exists(checkpoint):
        print(f"No checkpoint found at '{checkpoint}'.")
        print("Train the network first, or pass a checkpoint path as an argument.")
        sys.exit(1)
    play(checkpoint)
