# othello_net.py
import torch
import torch.nn as nn
import torch.nn.functional as F

class OthelloNet(nn.Module):
    def __init__(self):
        # Call the nn init function to set it all up
        super().__init__()
        # Create my layers; first, we have mine and my opponent's pieces (two channels), we then pass 128 filters
        # over it, which produce an output map per filter, which then get passed along
        self.conv1 = nn.Conv2d(in_channels=2, out_channels=128, kernel_size=3, padding=1)
        self.conv2 = nn.Conv2d(in_channels=128, out_channels=128, kernel_size=3, padding=1)
        self.conv3 = nn.Conv2d(in_channels=128, out_channels=128, kernel_size=3, padding=1)

        self.policy_fc = nn.Linear(128*8*8, 64)

        # Value head
        self.value_fc1 = nn.Linear(128 * 8 * 8, 64)
        self.value_fc2 = nn.Linear(64, 1)

    def forward(self, x):
        x = F.relu(self.conv1(x))
        x = F.relu(self.conv2(x))
        x = F.relu(self.conv3(x))

        x = x.view(x.size(0), -1)

        policy = self.policy_fc(x)

        value = F.relu(self.value_fc1(x))
        value = torch.tanh(self.value_fc2(value))

        return policy, value

    def predict(self, board, valid_moves):
        x = torch.tensor(board, dtype=torch.float32).unsqueeze(0)
        self.eval()

        with torch.no_grad():
            policy_logits, value = self.forward(x)

            policy_logits = policy_logits.squeeze(0)
            policy_logits[~valid_moves] = float('-inf')
            policy = F.softmax(policy_logits, dim=0)

            policy = policy.numpy()
            value = value.item()
            return policy, value