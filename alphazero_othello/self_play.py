import numpy as np
from mcts import MCTS

class SelfPlay:
    def __init__(self, network, simulations=200, temperature_threshold=30):
        self.network = network
        self.simulations = simulations
        self.temperature_threshold = temperature_threshold

    def execute_game(self, game):
        mcts = MCTS(self.network, self.simulations)
        training_data = []
        move_number = 0

        while not game.is_terminal():
            temperature = 1.0 if move_number < self.temperature_threshold else 0

            policy = mcts.get_policy(game, temperature)

            board = game.get_nn_input()
            training_data.append((board, policy, game.current_player))

            action = np.random.choice(64, p=policy)
            game.make_move(action)
            move_number += 1

        result = game.get_result()

        final_data = []
        for board, policy, player in training_data:
            outcome = result * player
            final_data.append((board, policy, outcome))

        return final_data