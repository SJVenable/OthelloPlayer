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
            public int column;
            public int row;

            public coordinate(int setRow, int setColumn)
            {
                column = setColumn;
                row = setRow;
            }
        }

        public string[,] BoardState = new string[8, 8];

        public Board(bool setup = true)
        {
            if (setup)
            {
                setUpBoard();
            }
        }

        /// <summary>
        /// Evaluation function for a board position
        /// </summary>
        /// <param name="board">The board to evaluate</param>
        /// <param name="PlayerTurn">True if it is the player's turn, and False otherwise</param>
        /// <param name="row">The zero-indexed row to evaluate</param>
        /// <param name="column">The zero-indexed column to evaluate</param>
        /// <returns>An integer representing the goodness of the position</returns>
        public int evaluatePlace(ref Board board, bool PlayerTurn, int row, int column)
        {
            return turnCounters(ref board, row, column, false, PlayerTurn).Count();
        }

        private static int maxDepth = 7;

        public static coordinate minimaxCall(Board board)
        {
            int tempScore = 0;
            int topScore = 0;
            coordinate bestSpot = new coordinate(0, 0);
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    if (checkValidMove(ref board, false, i, a))
                    {
                        Board newBoard = Board.copyBoardWithExtraPiece(board, i, a, false);
                        tempScore = Board.minimaxResult(newBoard, false, 0);
                        if (tempScore > topScore)
                        {
                            topScore = tempScore;
                            bestSpot = new coordinate(i, a);
                        }
                    }
                }
            }
            return bestSpot;
        }

        public static Board copyBoardWithExtraPiece(Board board, int row, int column, bool PlayerTurn)
        {
            Board boardCopy = new Board(setup: false);
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    boardCopy.BoardState[i, a] = board.BoardState[i, a];
                }
            }
            boardCopy.placeCounter(ref boardCopy, PlayerTurn, row, column);
            return boardCopy;
        }

        /// <summary>
        /// Returns the best spot to go, looking ahead to maxDepth.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerTurn"></param>
        /// <param name="currentDepth"></param>
        /// <returns></returns>
        public static int minimaxResult(Board board, bool playerTurn, int currentDepth)
        {
            int bestScore = 0;

            if (playerTurn) bestScore = -100000;
            else bestScore = 100000;

            if (currentDepth != maxDepth)
            {
                if (Board.isFull(ref board))
                {
                    if (Board.checkWinner(ref board) == "Black")
                    {
                        return -1000;
                    }
                    if (Board.checkWinner(ref board) == "Draw")
                    {
                        return -100;
                    }
                    else return 1000;
                }
                for (int i = 0; i < 8; i++)
                {
                    for (int a = 0; a < 8; a++)
                    {
                        if (checkValidMove(ref board, false, i, a))
                        {
                            //check if game is won by either player, if so, give score of 10000
                            Board newBoard = new Board();
                            newBoard = copyBoardWithExtraPiece(board, i, a, playerTurn);//copies board and add new counter
                            int tempScore = minimaxResult(newBoard, !playerTurn, currentDepth + 1);
                            if (playerTurn)
                            {
                                if (tempScore < bestScore) bestScore = tempScore;
                            }
                            else
                            {
                                if (tempScore > bestScore) bestScore = tempScore;
                            }
                        }
                    }
                }
                return bestScore;
            }
            else
            {
                return Board.evaluateBoard(board, playerTurn);
            }
        }
        public static int evaluateBoard(Board board, bool playerTurn)
        {
            if (playerTurn)
            {
                return (board.getBlackScore() - board.getWhiteScore());
            }
            else
            {
                return (board.getWhiteScore() - board.getBlackScore());
            }

        }

        public void placeCounter(ref Board board, bool PlayerTurn, int row, int column)
        {

            turnCounters(ref board, row, column, true, PlayerTurn);

        }

        public static bool checkValidMove(ref Board board, bool PlayerTurn, int row, int column)
        {
            if (row > 7 || row < 0 || column > 7 || column < 0)
            {
                return false;
            }

            if (board.BoardState[row, column] != " ")
            {
                return false;
            }

            if (turnCounters(ref board, row, column, false, PlayerTurn).Count != 0)
            {
                return true;
            }
            return false;

        }

        public static bool isFull(ref Board board)
        {
            //loop through values in board:
            for (int i = 0; i < 7; i++)
            {
                for (int a = 0; a < 7; a++)
                {

                    if (turnCounters(ref board, i, a, false, true).Count != 0)
                    {
                        return false;
                    }
                    if (turnCounters(ref board, i, a, false, false).Count != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool canPlaceCounter(ref Board board, bool PlayerTurn)
        {
            for (int i = 0; i < 7; i++)
            {
                for (int a = 0; a < 7; a++)
                {
                    if (Board.checkValidMove(ref board, PlayerTurn, i, a))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string checkWinner(ref Board board)
        {
            int onePoints = 0;
            int twoPoints = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int a = 0; a < 7; a++)
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

        public static List<coordinate> turnCounters(ref Board board, int row, int column, bool flip, bool PlayerTurn)
        {
            List<coordinate> discsToTurn = new List<coordinate>();
            List<coordinate> tempDiscsToTurn = new List<coordinate>();

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
                    for (int i = column + 2; i < 7; i++)
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


            //flip counters
            if (flip)
            {


                string flipToColour;
                if (PlayerTurn)
                {
                    flipToColour = "B";
                }
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
            for (int i = 0; i < 8; i++)
            {
                for (int a = 0; a < 8; a++)
                {
                    BoardState[i, a] = " ";
                }
            }
            BoardState[3, 3] = "W"; // should be W
            BoardState[4, 4] = "W"; // should be W
            BoardState[4, 3] = "B"; // should be B
            BoardState[3, 4] = "B"; // should be B



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
                if (item == "W")
                {
                    white++;
                }
            }
            return white;

        }

        private bool secondTimeBoard = false;

        public void displayBoard()
        {

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

            if (secondTimeBoard == true)
            {
                secondTimeBoard = false;
                Console.Clear();
                displayBoard();
            }



        }




    }
}