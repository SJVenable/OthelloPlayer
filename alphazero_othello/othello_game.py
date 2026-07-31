"""
OthelloGame — Python port of Board.cs
======================================
The board is represented as a 2D numpy array of shape (8, 8):
    0  = empty
    1  = current player's piece   (Black when it is Black's turn, White when White's)
   -1  = opponent's piece

To keep the network simple the board is always stored from the perspective of
the player whose turn it is (canonical form).  A positive value is always
"me", a negative value is always "opponent".  Internally, before returning the
canonical board we flip signs so the view is always consistent.

The eight directions checked in turnCounters are represented as (dr, dc) pairs.
"""

import numpy as np


# The eight cardinal/diagonal directions
DIRECTIONS = [(-1, -1), (-1, 0), (-1, 1),
              ( 0, -1),          ( 0, 1),
              ( 1, -1), ( 1, 0), ( 1, 1)]


class OthelloGame:
    """
    Manages Othello game state and rules.

    Internally the board stores:
        1  for Black pieces
       -1  for White pieces
        0  for empty squares

    Black always moves first.  'current_player' is either 1 (Black) or -1 (White).
    """

    def __init__(self):
        self.board = self._setup_board()
        self.current_player = 1  # Black goes first

    # ------------------------------------------------------------------
    # Board setup
    # ------------------------------------------------------------------

    def _setup_board(self):
        """Return an 8×8 board with the four starting discs."""
        board = np.zeros((8, 8), dtype=np.int8)
        # Mirrors Board.cs setUpBoard():
        #   BoardState[3,3]="W"  BoardState[4,4]="W"
        #   BoardState[4,3]="B"  BoardState[3,4]="B"
        board[3, 3] = -1   # White
        board[4, 4] = -1   # White
        board[4, 3] =  1   # Black
        board[3, 4] =  1   # Black
        return board

    # ------------------------------------------------------------------
    # Core move logic  (mirrors turnCounters in Board.cs)
    # ------------------------------------------------------------------

    def _get_flipped(self, board, row, col, player):
        """
        Return a list of (r, c) positions that would be flipped if 'player'
        places a disc at (row, col).  Returns [] if the move is invalid.

        Mirrors the logic in Board.cs turnCounters() with flip=False.
        """
        if board[row, col] != 0:
            return []

        opponent = -player
        all_flipped = []

        for dr, dc in DIRECTIONS:
            r, c = row + dr, col + dc
            line = []

            # Walk in this direction collecting opponent pieces
            while 0 <= r < 8 and 0 <= c < 8 and board[r, c] == opponent:
                line.append((r, c))
                r += dr
                c += dc

            # The line is only valid if it ends on one of our own pieces
            if line and 0 <= r < 8 and 0 <= c < 8 and board[r, c] == player:
                all_flipped.extend(line)

        return all_flipped

    def _apply_move(self, board, row, col, player):
        """
        Return a new board with the move applied (does not mutate the input).
        Raises ValueError if the move is invalid.
        """
        flipped = self._get_flipped(board, row, col, player)
        if not flipped:
            raise ValueError(f"Invalid move: ({row}, {col}) for player {player}")

        new_board = board.copy()
        new_board[row, col] = player
        for r, c in flipped:
            new_board[r, c] = player
        return new_board

    # ------------------------------------------------------------------
    # Valid move queries  (mirrors checkValidMove / canPlaceCounter)
    # ------------------------------------------------------------------

    def get_valid_moves(self, board=None, player=None):
        """
        Return a boolean numpy array of shape (64,) where True means the
        corresponding square (row-major) is a legal move for 'player'.

        Mirrors canPlaceCounter / checkValidMove in Board.cs.
        """
        if board is None:
            board = self.board
        if player is None:
            player = self.current_player

        valid = np.zeros(64, dtype=bool)
        for row in range(8):
            for col in range(8):
                if self._get_flipped(board, row, col, player):
                    valid[row * 8 + col] = True
        return valid

    def has_valid_move(self, board, player):
        """Return True if 'player' has at least one legal move."""
        for row in range(8):
            for col in range(8):
                if self._get_flipped(board, row, col, player):
                    return True
        return False

    # ------------------------------------------------------------------
    # Game flow
    # ------------------------------------------------------------------

    def make_move(self, action):
        """
        Apply an action (integer 0-63, row-major) for the current player.
        Advances current_player to the next player who has a legal move.
        Raises ValueError if the action is invalid.
        """
        row, col = divmod(action, 8)
        self.board = self._apply_move(self.board, row, col, self.current_player)

        # Switch to the other player if they have a move; otherwise keep the
        # same player (pass rule).  Mirrors the skip logic in Program.cs.
        next_player = -self.current_player
        if self.has_valid_move(self.board, next_player):
            self.current_player = next_player
        elif not self.has_valid_move(self.board, self.current_player):
            # Neither player can move — game over; player doesn't matter
            pass
        # else: current player retains their turn (opponent must pass)

    def is_terminal(self, board=None):
        """
        Return True when the game is over (neither player has a legal move).
        Mirrors isFull() in Board.cs — note: isFull() checked for valid moves,
        not whether every square was occupied.
        """
        if board is None:
            board = self.board
        return (not self.has_valid_move(board, 1) and
                not self.has_valid_move(board, -1))

    # ------------------------------------------------------------------
    # Scoring and result  (mirrors checkWinner / getBlackScore / getWhiteScore)
    # ------------------------------------------------------------------

    def get_score(self, board=None):
        """Return (black_score, white_score) disc counts."""
        if board is None:
            board = self.board
        black = int(np.sum(board == 1))
        white = int(np.sum(board == -1))
        return black, white

    def get_result(self, board=None):
        """
        Return the game result from Black's perspective:
            +1  Black wins
            -1  White wins
             0  Draw

        Mirrors checkWinner() in Board.cs.
        """
        if board is None:
            board = self.board
        black, white = self.get_score(board)
        if black > white:
            return 1
        if white > black:
            return -1
        return 0

    def get_result_for_player(self, player, board=None):
        """
        Return +1 if 'player' won, -1 if they lost, 0 for a draw.
        Used during MCTS backpropagation so each node sees the result
        from the perspective of the player who moved there.
        """
        result = self.get_result(board)
        return result * player

    # ------------------------------------------------------------------
    # Canonical board (AlphaZero convention)
    # ------------------------------------------------------------------

    def get_canonical_board(self, board=None, player=None):
        """
        Return the board from the perspective of 'player':
            +1 = player's own pieces
            -1 = opponent's pieces
             0 = empty

        When player == 1 (Black) the board is returned as-is.
        When player == -1 (White) the signs are flipped so the network
        always sees its own pieces as +1.
        """
        if board is None:
            board = self.board
        if player is None:
            player = self.current_player
        return board * player

    # ------------------------------------------------------------------
    # Neural-network input tensor
    # ------------------------------------------------------------------

    def get_nn_input(self, board=None, player=None):
        """
        Return a (2, 8, 8) float32 numpy array suitable as CNN input:
            channel 0: current player's pieces (1.0 where present)
            channel 1: opponent's pieces        (1.0 where present)

        This is the representation fed to OthelloNet.
        """
        canonical = self.get_canonical_board(board, player)
        planes = np.zeros((2, 8, 8), dtype=np.float32)
        planes[0] = (canonical ==  1).astype(np.float32)  # my pieces
        planes[1] = (canonical == -1).astype(np.float32)  # opponent pieces
        return planes

    # ------------------------------------------------------------------
    # Utility
    # ------------------------------------------------------------------

    def clone(self):
        """Return an independent deep copy of the game state."""
        new_game = OthelloGame.__new__(OthelloGame)
        new_game.board = self.board.copy()
        new_game.current_player = self.current_player
        return new_game

    def display(self, board=None):
        """Print the board to stdout (useful for debugging)."""
        if board is None:
            board = self.board
        symbols = {0: ".", 1: "B", -1: "W"}
        print("  A B C D E F G H")
        for row in range(8):
            print(row + 1, end=" ")
            for col in range(8):
                print(symbols[board[row, col]], end=" ")
            print()
        black, white = self.get_score(board)
        print(f"Black: {black}  White: {white}")
        print()
