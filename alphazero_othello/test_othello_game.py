"""
Tests for OthelloGame — verifies the Python port matches the C# Board.cs behaviour.

Run with:
    python -m pytest alphazero_othello/test_othello_game.py -v
or simply:
    python alphazero_othello/test_othello_game.py
"""

import numpy as np
import sys
import os
sys.path.insert(0, os.path.dirname(__file__))

from othello_game import OthelloGame


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def fresh():
    """Return a freshly initialised game."""
    return OthelloGame()


def action(row, col):
    """Convert (row, col) to a flat action index."""
    return row * 8 + col


# ---------------------------------------------------------------------------
# 1. Board setup
# ---------------------------------------------------------------------------

def test_initial_board_setup():
    """Mirrors setUpBoard() — four starting discs in the centre."""
    g = fresh()
    b = g.board
    assert b[3, 3] == -1, "Expected White at (3,3)"
    assert b[4, 4] == -1, "Expected White at (4,4)"
    assert b[4, 3] ==  1, "Expected Black at (4,3)"
    assert b[3, 4] ==  1, "Expected Black at (3,4)"
    # Everything else should be empty
    assert np.sum(b != 0) == 4
    print("PASS  test_initial_board_setup")


def test_initial_player_is_black():
    g = fresh()
    assert g.current_player == 1
    print("PASS  test_initial_player_is_black")


# ---------------------------------------------------------------------------
# 2. Valid moves at game start
# ---------------------------------------------------------------------------

def test_initial_valid_moves():
    """
    From the standard starting position Black has exactly 4 legal moves:
    (2,3), (3,2), (4,5), (5,4)
    """
    g = fresh()
    valid = g.get_valid_moves()
    expected = {action(2, 3), action(3, 2), action(4, 5), action(5, 4)}
    found = {i for i in range(64) if valid[i]}
    assert found == expected, f"Expected {expected}, got {found}"
    print("PASS  test_initial_valid_moves")


def test_invalid_move_occupied_square():
    """Can't place on an occupied square."""
    g = fresh()
    flipped = g._get_flipped(g.board, 3, 3, 1)  # (3,3) already has White
    assert flipped == []
    print("PASS  test_invalid_move_occupied_square")


def test_invalid_move_no_flip():
    """A square that flips nothing is not a valid move."""
    g = fresh()
    flipped = g._get_flipped(g.board, 0, 0, 1)  # corner — nothing to flip
    assert flipped == []
    print("PASS  test_invalid_move_no_flip")


# ---------------------------------------------------------------------------
# 3. Flip logic
# ---------------------------------------------------------------------------

def test_flip_count_move_c3():
    """
    Black plays (2,3) — should flip exactly one disc at (3,3).
    Board.cs turnCounters equivalent.
    """
    g = fresh()
    flipped = g._get_flipped(g.board, 2, 3, 1)
    assert (3, 3) in flipped, f"Expected (3,3) to flip, got {flipped}"
    assert len(flipped) == 1
    print("PASS  test_flip_count_move_c3")


def test_board_state_after_move():
    """After Black plays (2,3), (3,3) should become Black."""
    g = fresh()
    g.make_move(action(2, 3))
    assert g.board[2, 3] ==  1, "Placed piece should be Black"
    assert g.board[3, 3] ==  1, "Flipped piece should now be Black"
    assert g.board[4, 3] ==  1, "Untouched Black piece should still be Black"
    assert g.board[3, 4] ==  1, "Untouched Black piece should still be Black"
    assert g.board[4, 4] == -1, "Untouched White piece should still be White"
    print("PASS  test_board_state_after_move")


def test_player_switches_after_move():
    """After Black moves, it should be White's turn."""
    g = fresh()
    g.make_move(action(2, 3))
    assert g.current_player == -1
    print("PASS  test_player_switches_after_move")


# ---------------------------------------------------------------------------
# 4. Scores  (mirrors getBlackScore / getWhiteScore)
# ---------------------------------------------------------------------------

def test_initial_scores():
    g = fresh()
    black, white = g.get_score()
    assert black == 2
    assert white == 2
    print("PASS  test_initial_scores")


def test_scores_after_move():
    """After Black plays (2,3), Black gains one disc, White loses one."""
    g = fresh()
    g.make_move(action(2, 3))
    black, white = g.get_score()
    assert black == 4, f"Expected 4 Black, got {black}"
    assert white == 1, f"Expected 1 White, got {white}"
    print("PASS  test_scores_after_move")


# ---------------------------------------------------------------------------
# 5. Terminal detection  (mirrors isFull())
# ---------------------------------------------------------------------------

def test_initial_not_terminal():
    g = fresh()
    assert not g.is_terminal()
    print("PASS  test_initial_not_terminal")


def test_full_board_is_terminal():
    """A board where neither player has a legal move should be terminal."""
    g = fresh()
    # Fill entire board with Black — no moves possible for either player
    g.board[:] = 1
    assert g.is_terminal()
    print("PASS  test_full_board_is_terminal")


# ---------------------------------------------------------------------------
# 6. Game result  (mirrors checkWinner())
# ---------------------------------------------------------------------------

def test_result_black_wins():
    g = fresh()
    g.board[:] = 0
    g.board[0, 0] = 1   # 1 Black disc
    assert g.get_result() == 1
    print("PASS  test_result_black_wins")


def test_result_white_wins():
    g = fresh()
    g.board[:] = 0
    g.board[0, 0] = -1  # 1 White disc
    assert g.get_result() == -1
    print("PASS  test_result_white_wins")


def test_result_draw():
    g = fresh()
    g.board[:] = 0
    g.board[0, 0] =  1   # 1 Black
    g.board[0, 1] = -1   # 1 White
    assert g.get_result() == 0
    print("PASS  test_result_draw")


# ---------------------------------------------------------------------------
# 7. Canonical board and NN input
# ---------------------------------------------------------------------------

def test_canonical_board_black_perspective():
    """For Black (player=1) the board should be unchanged."""
    g = fresh()
    canonical = g.get_canonical_board(player=1)
    np.testing.assert_array_equal(canonical, g.board)
    print("PASS  test_canonical_board_black_perspective")


def test_canonical_board_white_perspective():
    """For White (player=-1) the signs should be flipped."""
    g = fresh()
    canonical = g.get_canonical_board(player=-1)
    np.testing.assert_array_equal(canonical, -g.board)
    print("PASS  test_canonical_board_white_perspective")


def test_nn_input_shape():
    g = fresh()
    nn_in = g.get_nn_input()
    assert nn_in.shape == (2, 8, 8)
    assert nn_in.dtype == np.float32
    print("PASS  test_nn_input_shape")


def test_nn_input_planes():
    """Channel 0 = my pieces, channel 1 = opponent's pieces."""
    g = fresh()
    nn_in = g.get_nn_input(player=1)
    # Black's pieces are at (4,3) and (3,4)
    assert nn_in[0, 4, 3] == 1.0
    assert nn_in[0, 3, 4] == 1.0
    # White's pieces are at (3,3) and (4,4)
    assert nn_in[1, 3, 3] == 1.0
    assert nn_in[1, 4, 4] == 1.0
    # All other squares empty
    assert nn_in[0].sum() == 2.0
    assert nn_in[1].sum() == 2.0
    print("PASS  test_nn_input_planes")


# ---------------------------------------------------------------------------
# 8. Clone independence
# ---------------------------------------------------------------------------

def test_clone_is_independent():
    """Mutating the clone should not affect the original."""
    g = fresh()
    g2 = g.clone()
    g2.make_move(action(2, 3))
    # Original board should be unchanged
    assert g.board[2, 3] == 0
    assert g.board[3, 3] == -1
    print("PASS  test_clone_is_independent")


# ---------------------------------------------------------------------------
# 9. Multi-move sequence
# ---------------------------------------------------------------------------

def test_two_move_sequence():
    """
    Play Black (2,3) then White (2,4) and check the resulting board
    matches manually computed expectations.
    """
    g = fresh()
    g.make_move(action(2, 3))   # Black plays C3
    g.make_move(action(2, 4))   # White plays D3

    # After Black C3: Black at (2,3), (3,3), (3,4), (4,3); White at (4,4)
    # After White D3: White flips (3,4) → White at (2,4), (3,4), (4,4); Black keeps (2,3),(3,3),(4,3)
    assert g.board[2, 3] ==  1  # Black placed
    assert g.board[3, 3] ==  1  # Flipped by Black's move, not re-flipped
    assert g.board[2, 4] == -1  # White placed
    assert g.board[3, 4] == -1  # Flipped back to White
    print("PASS  test_two_move_sequence")


# ---------------------------------------------------------------------------
# Run all tests
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    tests = [
        test_initial_board_setup,
        test_initial_player_is_black,
        test_initial_valid_moves,
        test_invalid_move_occupied_square,
        test_invalid_move_no_flip,
        test_flip_count_move_c3,
        test_board_state_after_move,
        test_player_switches_after_move,
        test_initial_scores,
        test_scores_after_move,
        test_initial_not_terminal,
        test_full_board_is_terminal,
        test_result_black_wins,
        test_result_white_wins,
        test_result_draw,
        test_canonical_board_black_perspective,
        test_canonical_board_white_perspective,
        test_nn_input_shape,
        test_nn_input_planes,
        test_clone_is_independent,
        test_two_move_sequence,
    ]

    passed = 0
    failed = 0
    for t in tests:
        try:
            t()
            passed += 1
        except Exception as e:
            print(f"FAIL  {t.__name__}: {e}")
            failed += 1

    print(f"\n{passed} passed, {failed} failed out of {len(tests)} tests.")
    if failed:
        sys.exit(1)
