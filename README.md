# Othello Player
Full Project Write-Up available [here](https://github.com/SJVenable/OthelloPlayer/blob/master/Othello%20Player%20Project%20Write-Up.docx)

## How It Works
The program is an AI opponent for the board game Othello. It allows for two levels of difficulty to play against. If set to Amateur difficulty, the AI will always choose to make the move which flips the most counters that turn.
If set to Professional difficulty, the AI will use an implementation of the Minimax algorithm to evaluate the best move based on how good its position is likely to be after a few 
moves time, using a heuristic function to determine how good a given board state is for both players.
