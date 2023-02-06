using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloPlayer
{
    class Board
    {
        public struct coordinate
        {
            //Create structure to hold coordinates of a counter
            public int column;
            public int row;

            //Create constructor to set values
            public coordinate(int setRow, int setColumn)
            {
                column = setColumn;
                row = setRow;
            }
        }

        //Create 2d array to hold board state
        public string[,] BoardState = new string[8, 8];

        //Allows for making a board without setting it up
        public Board(bool setup = true)
        {
            if (setup)
            {
                setUpBoard();
            }
        }

        //Method to find best spot to play based on amateur AI ( spot which turns most counters )
        public static coordinate getBestPlace(ref Board board, bool playerTurn)
        {
            Board.coordinate bestPlace = new coordinate(0, 0);
            int topScore = 0;
            //Loops through every square
            for (int i = 0; i < 7; i++)
            {
                for (int a = 0; a < 7; a++)
                {
                    //if valid move
                    if (Board.checkValidMove(ref board, false, i, a))
                    {
                        //if it turns more counters than the previous highest
                        if (turnCounters(ref board, i, a, false, false).Count() > topScore)
                        {
                            //set as new best place and set topScore to the new highest number of counters turned
                            bestPlace = new coordinate(i, a);
                            topScore = turnCounters(ref board, i, a, false, false).Count();
                        }
                    }
                }
            }
            return bestPlace;
        }

        //Depth for minimax algorithm to run
        private static int maxDepth = 4;

        //Minimax call
        public static coordinate minimaxCall(Board board)
        {
            //base scores set
            int tempScore = 0;
            int lowestScore = 100000;
            coordinate bestSpot = new coordinate(0, 0);
            //for every move on the board
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    //if it's valid
                    if (checkValidMove(ref board, false, i, a))
                    {
                        Board newBoard = Board.copyBoardWithExtraPiece(board, i, a, false);
                        //call minimax on this move, player's turn
                        tempScore = Board.minimaxResult(newBoard, true, 0);
                        //minimise the player's score
                        if (tempScore <= lowestScore)
                        {
                            lowestScore = tempScore;
                            //sets bestSpot to best coordinate for minimising player's score
                            bestSpot = new coordinate(i, a);
                        }
                    }
                }
            }
            return bestSpot;
        }

        public static Board copyBoardWithExtraPiece(Board board, int row, int column, bool PlayerTurn)
        {
            //returns a copy of the board once another piece has been places at (row, column)
            Board boardCopy = new Board(setup: false);
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    boardCopy.BoardState[i, a] = board.BoardState[i, a];
                }
            }
            Board.turnCounters(ref boardCopy, row, column, true, PlayerTurn);
            return boardCopy;
        }

        //Called from minimax call on the move AFTER each possible move from the board state given and returns a value representing how good that spot is (for the player)
        public static int minimaxResult(Board board, bool playerTurn, int currentDepth)
        {

            int bestScore = 0;

            if (playerTurn) bestScore = 100000;
            else bestScore = -100000;

            if (currentDepth != maxDepth)
            {
                if (Board.isFull(ref board))
                {
                    //if someone has wins give appropriate points:
                    if(playerTurn)
                    {
                        if (Board.checkWinner(ref board) == "Black")
                        {
                            return 1000;
                        }
                        if (Board.checkWinner(ref board) == "Draw")
                        {
                            return -10;
                        }
                        else return -1000;
                    }
                    else
                    {
                        if (Board.checkWinner(ref board) == "Black")
                        {
                            return -1000;
                        }
                        if (Board.checkWinner(ref board) == "Draw")
                        {
                            return -10;
                        }
                        else return 1000;
                    }
                }

                int nextGoScore = 0;
                Board finalBoard = new Board();
                //for each square on the board
                for (int i = 0; i < 8; i++)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        //if it's a valid move
                        if (checkValidMove(ref board, false, i, a))
                        {
                            Board newBoard = new Board();
                            newBoard = copyBoardWithExtraPiece(board, i, a, playerTurn); //copies board and adds new counter
                            //calls minimax again on this move, with depth increased and on the other player's turn
                            nextGoScore = minimaxResult(newBoard, !playerTurn, currentDepth+1);
                            if (playerTurn)
                            {
                                //If the AI's score next go is worse (for the AI) than the previous score, replace it.
                                if (nextGoScore < bestScore)
                                {
                                    bestScore = nextGoScore;
                                }
                            }
                            if (!playerTurn)
                            {
                                //If the player's score next go is better than the best previous, replace it.
                                if (nextGoScore > bestScore)
                                {
                                    bestScore = nextGoScore;
                                }
                            }
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                //if it's at it's max depth, evaluate the board state at this point for the player who's go it is.
                int evaluate = Board.evaluateBoard(board, playerTurn);
                return evaluate;
            }
        }
        public static int evaluateBoard(Board board, bool playerTurn)
        {
            int score = 0;
            if (playerTurn)
            {
                //base score given from difference between the number of counters on the board (player - opponent's)
                score = board.getBlackScore() - board.getWhiteScore();
                for(int i = 2; i < 6; i++)
                {
                    //+2 for edges
                    if(board.BoardState[0, i] == "B") score+= 2;
                    if (board.BoardState[i, 0] == "B") score+= 2;
                    if (board.BoardState[7, i] == "B") score+= 2;
                    if (board.BoardState[i, 7] == "B") score+= 2;
                    //take off points for opponent holding edges
                    if (board.BoardState[0, i] == "W") score -= 2;
                    if (board.BoardState[i, 0] == "W") score -= 2;
                    if (board.BoardState[7, i] == "W") score -= 2;
                    if (board.BoardState[i, 7] == "W") score -= 2;
                }

                //+20 for corners
                if (board.BoardState[0, 0] == "B") score += 20;
                if (board.BoardState[7, 0] == "B") score += 20;
                if (board.BoardState[7, 7] == "B") score += 20;
                if (board.BoardState[0, 7] == "B") score += 20;
                //take off points for opponent holding corners
                if (board.BoardState[0, 0] == "W") score -= 20;
                if (board.BoardState[7, 0] == "W") score -= 20;
                if (board.BoardState[7, 7] == "W") score -= 20;
                if (board.BoardState[0, 7] == "W") score -= 20;

            }

            else
            {
                //repeated in case of other player's go
                score = board.getWhiteScore() - board.getBlackScore();
                for (int i = 2; i < 6; i++)
                {
                    //+2 for edges
                    if (board.BoardState[0, i] == "W") score+=2;
                    if (board.BoardState[i, 0] == "W") score+=2;
                    if (board.BoardState[7, i] == "W") score+=2;
                    if (board.BoardState[i, 7] == "W") score+=2;

                    if (board.BoardState[0, i] == "B") score -= 2;
                    if (board.BoardState[i, 0] == "B") score -= 2;
                    if (board.BoardState[7, i] == "B") score -= 2;
                    if (board.BoardState[i, 7] == "B") score -= 2;

                }

                //+3 for corners
                //must check for R too while the board displays last move
                if (board.BoardState[0, 0] == "W") score += 20;
                if (board.BoardState[7, 0] == "W") score += 20;
                if (board.BoardState[7, 7] == "W") score += 20;
                if (board.BoardState[0, 7] == "W") score += 20;

                if (board.BoardState[0, 0] == "B") score -= 20;
                if (board.BoardState[7, 0] == "B") score -= 20;
                if (board.BoardState[7, 7] == "B") score -= 20;
                if (board.BoardState[0, 7] == "B") score -= 20;

            }
            //returns the 'goodness' of the board state
            return score;

        }

        public static bool checkValidMove(ref Board board, bool PlayerTurn, int row, int column)
        {
            //checks if out of bounds of the board
            if (row > 7 || row < 0 || column > 7 || column < 0)
            {
                return false;
            }
            //checks if counter already there
            if (board.BoardState[row, column] != " ")
            {
                return false;
            }
            //checks if it turns any counters
            if (turnCounters(ref board, row, column, false, PlayerTurn).Count != 0)
            {
                return true;
            }
            return false;

        }

        public static bool isFull(ref Board board)
        {
            //loop through values in board:
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    //if there is a valid move for neither player, return true as the board is full/game is ended
                    if (Board.checkValidMove(ref board, true, i, a))
                    {
                        return false;
                    }
                    if (Board.checkValidMove(ref board, false, i, a))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool canPlaceCounter(ref Board board, bool PlayerTurn)
        {
            //checks if a player can make a move
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    if (Board.checkValidMove(ref board, PlayerTurn, i, a))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //check who won the game
        public static string checkWinner(ref Board board)
        {
            int onePoints = 0;
            int twoPoints = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    if (board.BoardState[i, a] == "B")
                    {
                        onePoints++;
                    }
                    if (board.BoardState[i, a] == "W")
                    {
                        twoPoints++;
                    }

                }
            }
            if (onePoints > twoPoints)
            {
                return "Black";
            }
            if (onePoints < twoPoints)
            {
                return "White";
            }
            else return "Draw";
        }

        //checks which counters (if any) a move turns and changes the board accordingly if 'flip' is true
        public static List<coordinate> turnCounters(ref Board board, int row, int column, bool flip, bool PlayerTurn)
        {
            //create lists to turn discs permanantly and temporarily to facilitate yellow highlighting
            List<coordinate> discsToTurn = new List<coordinate>();
            List<coordinate> tempDiscsToTurn = new List<coordinate>();

            //checks that there is not a counter already there
            if (!(board.BoardState[row, column] == " ")) return discsToTurn;

            //variables holding 'B' or 'W' representing the counters how they are held in the board state array
            string myColour;
            string notMyColour;
            if (PlayerTurn)
            {
                myColour = "B";
                notMyColour = "W";
            }
            else
            {
                myColour = "W";
                notMyColour = "B";
            }

            //check horizontal right:
            if ((column + 2) < 7)
            {
                if (board.BoardState[row, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row, (column + 1)));
                    for (int i = column + 2; i < 8; i++)
                    {
                        if (board.BoardState[row, i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row, i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row, i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (i < 8)
                        {
                            if (board.BoardState[row, i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //checked horizontal right

            //check horizontal left
            if ((column - 2) > -1)
            {
                if (board.BoardState[row, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row, (column - 1)));
                    for (int i = column - 2; i > -1; i--)
                    {
                        if (board.BoardState[row, i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row, i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row, i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = -1;
                        }
                        if (i > -1)
                        {
                            if (board.BoardState[row, i] == " ")
                            {
                                i = -1;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //checked horizontal left.
            //check up:
            if ((row - 2) > -1)
            {
                if (board.BoardState[row - 1, column] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column));
                    for (int i = row - 2; i > -1; i--)
                    {
                        if (board.BoardState[i, column] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(i, column));
                            //add them to temporary list
                        }
                        if (board.BoardState[i, column] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = -1;
                        }
                        //REMOVE THIS IF EVERYWHERE?
                        if (i > -1)
                        {
                            if (board.BoardState[i, column] == " ")
                            {
                                i = -1;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //checked up.
            //check down:
            if ((row + 2) < 8)
            {
                if (board.BoardState[row + 1, column] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column));
                    for (int i = row + 2; i < 8; i++)
                    {
                        if (board.BoardState[i, column] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(i, column));
                            //add them to temporary list
                        }
                        if (board.BoardState[i, column] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (i < 8)
                        {
                            if (board.BoardState[i, column] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //Checked down.
            //Check SE diagonal:
            if (((row + 2) < 8) && ((column + 2) < 8))
            {
                if (board.BoardState[row + 1, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column + 1));
                    for (int i = 2; row + i < 8 && column + i < 8; i++)
                    {
                        if (board.BoardState[row + i, column + i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row + i, column + i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row + i, column + i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (i < 8)
                        {
                            if (board.BoardState[row + i, column + i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //Checked SE diagonal
            //Check NW diagonal:
            if (((row - 2) > -1) && ((column - 2) > -1))
            {
                if (board.BoardState[row - 1, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column - 1));
                    for (int i = 2; row - i > -1 && column - i > -1; i++)
                    {
                        if (board.BoardState[row - i, column - i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row - i, column - i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row - i, column - i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (row - i > -1)
                        {
                            if (board.BoardState[row - i, column - i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //Checked NW diagonal.
            //Check NE diagonal:
            if (((row - 2) > -1) && ((column + 2) < 8))
            {
                if (board.BoardState[row - 1, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column + 1));
                    for (int i = 2; row - i > -1 && column + i < 8; i++)
                    {
                        if (board.BoardState[row - i, column + i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row - i, column + i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row - i, column + i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (row - i > -1)
                        {
                            if (board.BoardState[row - i, column + i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //Checked NE diagonal
            //Check SW diagonal
            if (((column - 2) > -1) && ((row + 2) < 8))
            {
                if (board.BoardState[row + 1, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column - 1));
                    for (int i = 2; column - i > -1 && row + i < 8; i++)
                    {
                        if (board.BoardState[row + i, column - i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row + i, column - i));
                            //add them to temporary list
                        }
                        if (board.BoardState[row + i, column - i] == myColour)
                        {
                            foreach (var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (row + i < 8)
                        {
                            if (board.BoardState[row + i, column - i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                    tempDiscsToTurn = new List<coordinate>();
                }
            }
            //Checked SW diagonal
            //CHECKED ALL DIRECTIONS


            //flip counters if flip is true
            if (flip)
            {


                string flipToColour;
                if (PlayerTurn)
                {
                    flipToColour = "B";
                }
                //flip to R yellow counters which have just been flipped
                else flipToColour = "R";
                board.BoardState[row, column] = flipToColour;
                foreach (var item in discsToTurn)
                {
                    //R for recently placed/flipped
                    board.BoardState[item.row, item.column] = flipToColour;
                }
            }
            return discsToTurn;

        }






        public void setUpBoard()
        {
            //sets up board at start of the game
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    BoardState[i, a] = " ";
                }
            }
            BoardState[3, 3] = "W";
            BoardState[4, 4] = "W";
            BoardState[4, 3] = "B";
            BoardState[3, 4] = "B";

        }

        public int getBlackScore()
        {
            int black = 0;
            foreach (var item in BoardState)
            {
                if (item == "B")
                {
                    black++;
                }
            }
            return black;

        }
        public int getWhiteScore()
        {
            int white = 0;
            foreach (var item in BoardState)
            {
                if (item == "W" || item == "R")
                {
                    white++;
                }
            }
            return white;

        }

        //represents whether board is 'flashing' yellow or not
        private bool secondTimeBoard = false;

        public void displayBoard()
        {
            //prints board and scores in console
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write("    - A --- B --- C --- D --- E --- F --- G --- H --");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("  SCORES:");
            Console.WriteLine();
            Console.BackgroundColor = ConsoleColor.DarkGreen;

            List<coordinate> recentlyFlipped = new List<coordinate>();

            for (int i = 1; i < 9; i++)
            {
                Console.Write(" " + i + " |");
                for (int a = 0; a < 8; a++)
                {
                    if (BoardState[i - 1, a] == "B")
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write("  █");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    else if (BoardState[i - 1, a] == "W")
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("  █");

                    }
                    else if (BoardState[i - 1, a] == "R")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write("  █");
                        Console.ForegroundColor = ConsoleColor.White;
                        recentlyFlipped.Add(new coordinate(i - 1, a));

                    }
                    else
                    {
                        Console.Write("   ");
                    }

                    Console.Write("  |");


                }
                if (i == 1)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" Black: " + getBlackScore());
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine();
                    Console.Write("    ------------------------------------------------");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(" White: " + getWhiteScore());
                    Console.BackgroundColor = ConsoleColor.DarkGreen;

                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("    ------------------------------------------------");
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;

            if (recentlyFlipped.Count != 0)
            {
                //if it's the first display, this allows the recently flipped counters to show orange for 2 seconds
                if (secondTimeBoard == false)
                {
                    System.Threading.Thread.Sleep(2000);
                    foreach (var item in recentlyFlipped) 
                    {
                        BoardState[item.row, item.column] = "W";
                    }
                    recentlyFlipped.Clear();
                    secondTimeBoard = true;
                }
            }

            //prints normal board with no yellows, after highlighting
            if (secondTimeBoard == true)
            {
                secondTimeBoard = false;                          
                Console.Clear();
                displayBoard();
            }
        }
    }
}