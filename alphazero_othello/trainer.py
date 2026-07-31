import torch
import torch.nn.functional as F
import numpy as np
import os
from othello_game import OthelloGame
from OthelloNet import OthelloNet
from self_play import SelfPlay
from mcts import MCTS

class Trainer:
    # lr - learning rate - how big each weight update step is.
    # iterations = how many cycles of playing, training and evaluating to run
    # games_per_iteration = how many self-play games to generate per iteration before training
    # simulations - how many MCTS simulations per move
    # checkpoint_dir - where to save network weights after each iteration
    def __init__(self, network, lr=0.001, iterations=10, games_per_iteration=20,
                simulations=50, eval_games=40, checkpoint_dir='checkpoints'):

        self.network = network
        self.optimizer = torch.optim.Adam(network.parameters(), lr=lr)
        self.iterations = iterations
        self.games_per_iteration = games_per_iteration
        self.simulations = simulations
        self.eval_games = eval_games
        self.checkpoint_dir = checkpoint_dir
        os.makedirs(checkpoint_dir, exist_ok=True)

    def train_step(self, training_data):
        boards, policies, outcomes = zip(*training_data)

        boards = torch.tensor(np.array(boards), dtype=torch.float32)
        policies = torch.tensor(np.array(policies), dtype=torch.float32)
        outcomes = torch.tensor(np.array(outcomes), dtype=torch.float32).unsqueeze(1)

        self.network.train()
        self.optimizer.zero_grad()

        policy_logits, values = self.network(boards)

        policy_loss = -torch.mean(torch.sum(
            policies * F.log_softmax(policy_logits, dim=1), dim=1))
        value_loss = F.mse_loss(values, outcomes)

        loss = policy_loss + value_loss
        loss.backward()
        self.optimizer.step()

        return loss.item()

    def _play_match(self, game, mcts_black, mcts_white):
        while not game.is_terminal():
            if game.current_player == 1:
                policy = mcts_black.get_policy(game, temperature=0.1)
            else:
                policy = mcts_white.get_policy(game, temperature=0.1)
            action = np.random.choice(64, p=policy)
            game.make_move(action)
        return game.get_result()

    def evaluate(self, new_net, old_net, games=20):
        wins = 0
        for i in range(games):
            game = OthelloGame()
            mcts_new = MCTS(new_net, self.simulations)
            mcts_old = MCTS(old_net, self.simulations)

            if i % 2 == 0:
                result = self._play_match(game, mcts_new, mcts_old)
            else:
                result = -self._play_match(game, mcts_old, mcts_new)

            if result > 0:
                wins +=1

        win_rate = wins / games
        print(f'  New network win rate: {win_rate:.0%}')
        return win_rate > 0.55

    def run(self):
        sp = SelfPlay(self.network, self.simulations)
        for iteration in range(self.iterations):
            print(f'\nIteration {iteration + 1}/{self.iterations}')

            # Snapshot weights BEFORE training — compare trained vs pre-training
            old_net = OthelloNet()
            old_net.load_state_dict(self.network.state_dict())

            print(' Generating games')
            all_data = []
            for g in range(self.games_per_iteration):
                game = OthelloGame()
                data = sp.execute_game(game)
                all_data.extend(data)
                print(f'    Game {g+1}/{self.games_per_iteration} — {len(data)} positions')
            print(f'  Training on {len(all_data)} positions...')
            loss = self.train_step(all_data)
            print(f'  Loss: {loss:.4f}')

            if self.evaluate(self.network, old_net, games=self.eval_games):
                print('  New network is better — saving checkpoint')
                torch.save(self.network.state_dict(), f'{self.checkpoint_dir}/best.pt')
            else:
                print('  Old network retained')

            torch.save(self.network.state_dict(),
                f'{self.checkpoint_dir}/iteration_{iteration+1}.pt')