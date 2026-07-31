import numpy as np
import math

class MCTS:
    def __init__(self, network, simulations=200, c=1.0):
        self.network = network
        self.simulations = simulations
        self.c = c

        self.N = {} # The number of times a particular node was visited
        self.Q = {} # The average win probability from this move
        self.P = {} # The prior probabilities from network
        self.valid = {} # legal moves mask

    def _board_key(self, board):
        return board.tobytes()


    def get_policy(self, game, temperature=1.0):
        for _ in range(self.simulations):
            self._simulate(game.clone())
        
        state = self._board_key(game.get_canonical_board())
        counts = np.array([
            self.N[state].get(a, 0) for a in range(64)
        ])
        if temperature == 0:
            best = np.argmax(counts)
            policy = np.zeros(64)
            policy[best] = 1.0
            return policy

        counts = counts ** (1.0 / temperature)
        return counts / counts.sum()

    def _simulate(self, game):
        if game.is_terminal():
            return -game.get_result_for_player(game.current_player)

        state = self._board_key(game.get_canonical_board())
        if state not in self.P:
            return self._expand(game, state)

        # If we get here, the state is already in the tree (not a leaf node)
        valid = self.valid[state]
        N_total = sum(self.N[state].values())

        best_ucb = -float('inf')
        best_action = -1

        for a in range(64):
            if not valid[a]:
                continue

            q = self.Q[state].get(a, 0)
            n = self.N[state].get(a, 0)
            p = self.P[state][a]

            ucb = q + self.c * p * math.sqrt(N_total) / (1 + n)

            if ucb > best_ucb:
                best_ucb = ucb
                best_action = a

        game.make_move(best_action)
        value = self._simulate(game)

        # update visit count
        self.N[state][best_action] = self.N[state].get(best_action, 0) + 1

        # Update average value
        n = self.N[state][best_action]
        old_q = self.Q[state].get(best_action, 0)
        self.Q[state][best_action] = (old_q * (n-1) + (-value)) / n

        return -value

    def _expand(self, game, state):
        valid = game.get_valid_moves()
        board = game.get_nn_input()

        policy, value = self.network.predict(board, valid)

        self.P[state] = policy
        self.valid[state] = valid
        self.N[state] = {}
        self.Q[state] = {}

        return -value