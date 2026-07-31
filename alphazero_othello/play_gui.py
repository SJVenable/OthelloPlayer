"""
play_gui.py — Pygame GUI for playing Othello against the trained AI.

Run from inside the alphazero_othello directory:
    python3 play_gui.py                        # uses checkpoints/best.pt
    python3 play_gui.py checkpoints/best.pt    # explicit checkpoint
"""

import sys
import os
import threading
import numpy as np
import pygame

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import torch
from othello_game import OthelloGame
from OthelloNet import OthelloNet
from mcts import MCTS


# ---------------------------------------------------------------------------
# Layout & colour constants
# ---------------------------------------------------------------------------

CELL        = 80          # pixels per cell
BOARD_SIZE  = 8
BOARD_PX    = CELL * BOARD_SIZE          # 640
SIDEBAR     = 220                         # right info panel
WIN_W       = BOARD_PX + SIDEBAR
WIN_H       = BOARD_PX + CELL            # extra row at bottom for col labels
LABEL_H     = CELL // 2                  # row label column width

# Colours
C_BG         = (18,  18,  18)
C_BOARD      = (0,  130,  80)
C_GRID       = (0,  100,  60)
C_HINT       = (100, 220, 140)  # valid-move dot
C_BLACK      = (20,   20,  20)
C_WHITE      = (245, 245, 245)
C_HIGHLIGHT  = (255, 220,  60)  # last-move ring
C_SIDEBAR    = (28,   28,  28)
C_TEXT       = (230, 230, 230)
C_MUTED      = (120, 120, 120)
C_WIN        = (80,  200, 100)
C_LOSE       = (220,  80,  80)
C_DRAW       = (200, 200,  80)
C_LABEL      = (180, 220, 180)

COL_LETTERS  = "ABCDEFGH"

AI_SIMULATIONS = 400   # increase for a stronger (slower) AI


# ---------------------------------------------------------------------------
# Helper: board coordinates ↔ pixel positions
# ---------------------------------------------------------------------------

def cell_rect(row: int, col: int) -> pygame.Rect:
    """Return the pygame.Rect for board cell (row, col)."""
    x = col * CELL
    y = row * CELL
    return pygame.Rect(x, y, CELL, CELL)


def pixel_to_cell(px: int, py: int):
    """Convert a pixel position to (row, col), or None if outside the board."""
    if 0 <= px < BOARD_PX and 0 <= py < BOARD_PX:
        return py // CELL, px // CELL
    return None


# ---------------------------------------------------------------------------
# Drawing helpers
# ---------------------------------------------------------------------------

def draw_board(surface: pygame.Surface, game: OthelloGame,
               last_move, valid_mask: np.ndarray,
               show_hints: bool, font_sm, font_med) -> None:

    board = game.board

    # Board background
    surface.fill(C_BOARD, pygame.Rect(0, 0, BOARD_PX, BOARD_PX))

    # Grid lines
    for i in range(BOARD_SIZE + 1):
        pygame.draw.line(surface, C_GRID, (i * CELL, 0), (i * CELL, BOARD_PX), 1)
        pygame.draw.line(surface, C_GRID, (0, i * CELL), (BOARD_PX, i * CELL), 1)

    last_row = last_col = -1
    if last_move is not None:
        last_row, last_col = divmod(last_move, 8)

    for row in range(BOARD_SIZE):
        for col in range(BOARD_SIZE):
            rect = cell_rect(row, col)
            cx   = rect.centerx
            cy   = rect.centery
            val  = board[row, col]

            # Highlight last move
            if row == last_row and col == last_col:
                pygame.draw.rect(surface, C_HIGHLIGHT, rect.inflate(-4, -4), border_radius=6)

            if val == 1:       # Black
                pygame.draw.circle(surface, C_BLACK, (cx, cy), CELL // 2 - 6)
            elif val == -1:    # White
                pygame.draw.circle(surface, C_WHITE, (cx, cy), CELL // 2 - 6)
            elif show_hints and valid_mask[row * 8 + col]:
                # Small dot showing a valid move
                pygame.draw.circle(surface, C_HINT, (cx, cy), CELL // 8)

    # Column labels (A-H) along the bottom edge of the board
    for col in range(BOARD_SIZE):
        lbl = font_sm.render(COL_LETTERS[col], True, C_LABEL)
        surface.blit(lbl, (col * CELL + (CELL - lbl.get_width()) // 2, BOARD_PX + 4))

    # Row labels (1-8) along the right edge of the board
    for row in range(BOARD_SIZE):
        lbl = font_sm.render(str(row + 1), True, C_LABEL)
        x = BOARD_PX + (SIDEBAR - lbl.get_width()) // 2  # centred in first line of sidebar
        y = row * CELL + (CELL - lbl.get_height()) // 2
        # actually put them inside the left side of the board (overlaid, small)
        lbl2 = font_sm.render(str(row + 1), True, C_LABEL)
        surface.blit(lbl2, (4, row * CELL + (CELL - lbl2.get_height()) // 2))


def draw_sidebar(surface: pygame.Surface, game: OthelloGame,
                 status: str, ai_thinking: bool,
                 font_title, font_med, font_sm) -> None:

    x0 = BOARD_PX
    panel = pygame.Rect(x0, 0, SIDEBAR, WIN_H)
    surface.fill(C_SIDEBAR, panel)

    # Thin separator
    pygame.draw.line(surface, C_GRID, (x0, 0), (x0, WIN_H), 2)

    black_score, white_score = game.get_score()
    cy = 24

    def put(text, color, font, offset=0):
        nonlocal cy
        surf = font.render(text, True, color)
        surface.blit(surf, (x0 + 16 + offset, cy))
        cy += surf.get_height() + 4

    # Title
    put("OTHELLO", C_TEXT, font_title)
    cy += 8

    # Scores
    put("● Black (You)", C_BLACK if True else C_TEXT, font_med)
    # Draw the black disc icon inline
    pygame.draw.circle(surface, C_BLACK, (x0 + 20, cy - font_med.get_height() - 2), 8)
    pygame.draw.circle(surface, C_MUTED, (x0 + 20, cy - font_med.get_height() - 2), 8, 1)

    score_text = font_med.render(f"  {black_score}", True, C_TEXT)
    surface.blit(score_text, (x0 + 32, cy - font_med.get_height() - 6))

    # White
    pygame.draw.circle(surface, C_WHITE, (x0 + 20, cy + 10), 8)
    pygame.draw.circle(surface, C_MUTED,  (x0 + 20, cy + 10), 8, 1)
    score_white = font_med.render(f"  {white_score}", True, C_TEXT)
    surface.blit(score_white, (x0 + 32, cy + 6))
    cy += 44

    # Divider
    pygame.draw.line(surface, C_GRID, (x0 + 12, cy), (x0 + SIDEBAR - 12, cy), 1)
    cy += 12

    # Status / whose turn
    if ai_thinking:
        put("AI is thinking…", C_HINT, font_med)
    else:
        put(status, C_TEXT, font_med)

    cy += 8
    pygame.draw.line(surface, C_GRID, (x0 + 12, cy), (x0 + SIDEBAR - 12, cy), 1)
    cy += 12

    # Legend
    put("Controls", C_MUTED, font_sm)
    put("H  — toggle hints", C_MUTED, font_sm)
    put("R  — restart", C_MUTED, font_sm)
    put("Q / Esc — quit", C_MUTED, font_sm)


# ---------------------------------------------------------------------------
# Main game loop
# ---------------------------------------------------------------------------

def run(checkpoint_path: str) -> None:
    pygame.init()
    pygame.display.set_caption("Othello vs AI")

    screen = pygame.display.set_mode((WIN_W, WIN_H))
    clock  = pygame.time.Clock()

    font_title = pygame.font.SysFont("monospace", 26, bold=True)
    font_med   = pygame.font.SysFont("monospace", 18)
    font_sm    = pygame.font.SysFont("monospace", 14)

    # Load network
    net = OthelloNet()
    net.load_state_dict(torch.load(checkpoint_path, weights_only=True))
    net.eval()

    def new_game():
        game  = OthelloGame()
        mcts  = MCTS(net, simulations=AI_SIMULATIONS)
        return game, mcts

    game, mcts = new_game()

    last_move    = None      # int or None
    status       = "Your turn (Black)"
    show_hints   = True
    ai_thinking  = False
    game_over    = False
    ai_action    = []        # one-element shared buffer with AI thread

    def ai_worker():
        """Run MCTS in a background thread so the UI stays responsive."""
        policy = mcts.get_policy(game, temperature=0)
        ai_action.clear()
        ai_action.append(int(np.argmax(policy)))
        # Post a custom event so the main loop can pick up the result safely
        pygame.event.post(pygame.event.Event(pygame.USEREVENT, {}))

    valid_mask = game.get_valid_moves()

    running = True
    while running:
        clock.tick(60)

        # ---- event handling ----
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            elif event.type == pygame.KEYDOWN:
                if event.key in (pygame.K_q, pygame.K_ESCAPE):
                    running = False
                elif event.key == pygame.K_h:
                    show_hints = not show_hints
                elif event.key == pygame.K_r:
                    game, mcts   = new_game()
                    last_move    = None
                    status       = "Your turn (Black)"
                    ai_thinking  = False
                    game_over    = False
                    ai_action.clear()
                    valid_mask   = game.get_valid_moves()

            elif event.type == pygame.MOUSEBUTTONDOWN and not game_over:
                if not ai_thinking and game.current_player == 1:
                    cell = pixel_to_cell(*event.pos)
                    if cell is not None:
                        row, col = cell
                        action = row * 8 + col
                        if valid_mask[action]:
                            game.make_move(action)
                            last_move  = action
                            valid_mask = game.get_valid_moves()

                            if game.is_terminal():
                                game_over = True
                                result    = game.get_result()
                                b, w      = game.get_score()
                                if result == 1:
                                    status = f"You win!  {b}–{w}"
                                elif result == -1:
                                    status = f"AI wins!  {b}–{w}"
                                else:
                                    status = f"Draw!  {b}–{w}"
                            elif game.current_player == -1:
                                # AI's turn
                                ai_thinking = True
                                status      = "AI is thinking…"
                                t = threading.Thread(target=ai_worker, daemon=True)
                                t.start()
                            else:
                                # Human must move again (AI had no moves)
                                status = "Your turn (Black) — AI passed"

            elif event.type == pygame.USEREVENT and ai_thinking:
                # AI finished
                if ai_action:
                    action = ai_action[0]
                    game.make_move(action)
                    last_move   = action
                    valid_mask  = game.get_valid_moves()
                    ai_thinking = False

                    if game.is_terminal():
                        game_over = True
                        result    = game.get_result()
                        b, w      = game.get_score()
                        if result == 1:
                            status = f"You win!  {b}–{w}"
                        elif result == -1:
                            status = f"AI wins!  {b}–{w}"
                        else:
                            status = f"Draw!  {b}–{w}"
                    elif game.current_player == 1:
                        if game.has_valid_move(game.board, 1):
                            status = "Your turn (Black)"
                        else:
                            # Human has no moves — trigger AI again immediately
                            status      = "No moves for you — AI goes again"
                            ai_thinking = True
                            t = threading.Thread(target=ai_worker, daemon=True)
                            t.start()
                    else:
                        status = "Your turn (Black)"

        # ---- drawing ----
        screen.fill(C_BG)
        draw_board(screen, game, last_move, valid_mask, show_hints, font_sm, font_med)

        # Game-over overlay
        if game_over:
            result = game.get_result()
            overlay_color = C_WIN if result == 1 else (C_LOSE if result == -1 else C_DRAW)
            overlay = pygame.Surface((BOARD_PX, 80), pygame.SRCALPHA)
            overlay.fill((0, 0, 0, 160))
            screen.blit(overlay, (0, BOARD_PX // 2 - 40))
            msg = font_title.render(status, True, overlay_color)
            screen.blit(msg, ((BOARD_PX - msg.get_width()) // 2, BOARD_PX // 2 - 20))
            hint = font_sm.render("Press R to play again", True, C_MUTED)
            screen.blit(hint, ((BOARD_PX - hint.get_width()) // 2, BOARD_PX // 2 + 18))

        draw_sidebar(screen, game, status, ai_thinking, font_title, font_med, font_sm)

        pygame.display.flip()

    pygame.quit()


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    base_dir    = os.path.dirname(os.path.abspath(__file__))
    checkpoint  = sys.argv[1] if len(sys.argv) > 1 else os.path.join(base_dir, "checkpoints", "best.pt")

    if not os.path.exists(checkpoint):
        print(f"No checkpoint found at '{checkpoint}'.")
        print("Train the network first, or pass a checkpoint path as an argument.")
        sys.exit(1)

    run(checkpoint)
