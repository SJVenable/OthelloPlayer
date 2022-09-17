﻿using System;
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

        public static string[,] BoardState = new string[8, 8];

        public Board()
        {
            setUpBoard();

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

        private int maxDepth = 5;
        /// <summary>
        /// Returns the best spot to go, looking ahead to maxDepth.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerTurn"></param>
        /// <param name="currentDepth"></param>
        /// <returns></returns>
        public coordinate minimaxResult(Board board, bool playerTurn, int currentDepth)
        {
            int tempScore = 0;
            int topScore = 0;
            for(int i = 0; i < 8; i++)
            {
                for(int a = 0; a < 8; a++)
                {
                    if(checkValidMove(ref board, playerTurn, i, a))
                    {
                        if(currentDepth < maxDepth)
                        {

                        }
                    }
                }
            }
        }

        public int evaluateBoard(Board board, bool playerTurn)
        {
            if(playerTurn)
            {
                return (getBlackScore() - getWhiteScore());
            }
            else
            {
                return (getWhiteScore() - getBlackScore());
            }
            
        }

        public void placeCounter(ref Board board, bool PlayerTurn, int row, int column)
        {
            
            turnCounters(ref board, row, column, true, PlayerTurn);

        }

        public static bool checkValidMove(ref Board board, bool PlayerTurn, int row, int column)
        {
            if(row > 7 || row < 0 || column > 7 || column < 0)
            {
                return false;
            }

            if (BoardState[row, column] != " ")
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
                    if(BoardState[i, a] == "B")
                    {
                        onePoints++;
                    }
                    if (BoardState[i, a] == "W")
                    {
                        twoPoints++;
                    }
                    
                }
            }
            if(onePoints > twoPoints)
            {
                return "Player One! (BLACK) Congratulations!";
            }
            if (onePoints < twoPoints)
            {
                return "Player Two! (WHITE) Congratulations!";
            }
            else return "Nobody, it was a draw!";
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
            if((column + 2) < 7)
            {
                if (BoardState[row, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row, (column + 1)));
                    for (int i = column + 2; i < 7; i++)
                    {
                        if (BoardState[row, i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row, i));
                            //add them to temporary list
                        }
                        if(BoardState[row, i] == myColour)
                        {
                            foreach(var item in tempDiscsToTurn)
                            {
                                discsToTurn.Add(item);
                            }
                            tempDiscsToTurn = new List<coordinate>();
                            i = 8;
                        }
                        if (i < 8)
                        {
                            if (BoardState[row, i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //checked horizontal right

            //check horizontal left
            if ((column - 2) > -1)
            {
                if (BoardState[row, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row, (column - 1)));
                    for (int i = column - 2; i > -1; i--)
                    {
                        if (BoardState[row, i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row, i));
                            //add them to temporary list
                        }
                        if (BoardState[row, i] == myColour)
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
                            if (BoardState[row, i] == " ")
                            {
                                i = -1;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //checked horizontal left.
            //check up:
            if ((row - 2) > -1)
            {
                if (BoardState[row - 1, column] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column));
                    for (int i = row - 2; i > -1; i--)
                    {
                        if (BoardState[i, column] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(i, column));
                            //add them to temporary list
                        }
                        if (BoardState[i, column] == myColour)
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
                            if (BoardState[i, column] == " ")
                            {
                                i = -1;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //checked up.
            //check down:
            if ((row + 2) < 8)
            {
                if (BoardState[row + 1, column] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column));
                    for (int i = row + 2; i < 8; i++)
                    {
                        if (BoardState[i, column] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(i, column));
                            //add them to temporary list
                        }
                        if (BoardState[i, column] == myColour)
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
                            if (BoardState[i, column] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //Checked down.
            //Check SE diagonal:
            if (((row + 2) < 8) && ((column + 2) < 8))
            {
                if (BoardState[row + 1, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column + 1));
                    for (int i = 2; row + i < 8 && column + i < 8; i++)
                    {
                        if (BoardState[row + i, column + i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row + i, column + i));
                            //add them to temporary list
                        }
                        if (BoardState[row + i, column + i] == myColour)
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
                            if (BoardState[row + i, column + i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //Checked SE diagonal
            //Check NW diagonal:
            if (((row - 2) > -1) && ((column - 2) > -1))
            {
                if (BoardState[row - 1, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column - 1));
                    for (int i = 2; row - i > -1 && column - i > -1; i++)
                    {
                        if (BoardState[row - i, column - i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row - i, column - i));
                            //add them to temporary list
                        }
                        if (BoardState[row - i, column - i] == myColour)
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
                            if (BoardState[row - i, column - i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //Checked NW diagonal.
            //Check NE diagonal:
            if (((row - 2) > -1) && ((column + 2) < 8))
            {
                if (BoardState[row - 1, column + 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row - 1, column + 1));
                    for (int i = 2; row - i > -1 && column + i < 8; i++)
                    {
                        if (BoardState[row - i, column + i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row - i, column + i));
                            //add them to temporary list
                        }
                        if (BoardState[row - i, column + i] == myColour)
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
                            if (BoardState[row - i, column + i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
                }
            }
            //Checked NE diagonal
            //Check SW diagonal
            if (((column - 2) > -1) && ((row + 2) < 8))
            {
                if (BoardState[row + 1, column - 1] == notMyColour)
                {
                    tempDiscsToTurn.Add(new coordinate(row + 1, column - 1));
                    for (int i = 2; column - i > -1 && row + i < 8; i++)
                    {
                        if (BoardState[row + i, column - i] == notMyColour)
                        {
                            tempDiscsToTurn.Add(new coordinate(row + i, column - i));
                            //add them to temporary list
                        }
                        if (BoardState[row + i, column - i] == myColour)
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
                            if (BoardState[row + i, column - i] == " ")
                            {
                                i = 8;
                                tempDiscsToTurn = new List<coordinate>();
                                //removes coordinates from the list as not a valid flank
                            }
                        }
                    }
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
                BoardState[row, column] = flipToColour;
                foreach (var item in discsToTurn)
                {
                    //R for recently placed/flipped
                    BoardState[item.row, item.column] = flipToColour;
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
            foreach(var item in BoardState)
            {
                if(item == "B")
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
                for(int a = 0; a < 8; a++)
                {
                    if(BoardState[i-1, a] == "B")
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
                    else if (BoardState[i-1, a] == "R")
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
                if(i == 1)
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
                else {
                    Console.WriteLine();
                    Console.WriteLine("    ------------------------------------------------");
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;

            if(recentlyFlipped.Count != 0)
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
            
            if(secondTimeBoard == true)
            {
                secondTimeBoard = false;
                Console.Clear();
                displayBoard();
            }
            


        }




    }
}
