﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloPlayer
{
    class Program
    {

        public static string difficulty = "Professional";


        static void Main(string[] args)
        {
            //set up board with starting discs

            Board gameBoard = new Board();

            //returns a menu option which sends you down one of three switch statements.
            int menuOption = displayMenu();

            while (menuOption != 1)
            {
                switch (menuOption)
                {
                    case 2:
                        Console.Clear();
                        displayDifficulty();
                        Console.Clear();
                        menuOption = displayMenu();
                        break;
                    case 3:
                        Console.Clear();
                        Console.WriteLine("Thanks for Playing!");
                        break;
                }

            }
            Console.Clear();
            gameBoard.displayBoard();
            playGame(ref gameBoard);

        }

        public static void displayDifficulty()
        {
            Console.WriteLine(" Select Difficulty: ");
            Console.WriteLine(" > Amateur");
            Console.WriteLine("   Professional");
            Console.WriteLine("   Exit to Menu");
            Console.WriteLine("   Currently: " + difficulty);


            ConsoleKeyInfo choice = Console.ReadKey(true);
            int option = 1;

            while (!(choice.Key == ConsoleKey.Enter && option == 3))
            {

                if (choice.Key == ConsoleKey.Enter)
                {
                    switch (option)
                    {
                        case 1:
                            difficulty = "Amateur";
                            Console.Clear();
                            displayDifficulty();
                            return;
                            break;
                        case 2:
                            difficulty = "Professional";
                            Console.Clear();
                            displayDifficulty();
                            return;
                            break;


                    }
                }

                if (choice.Key == ConsoleKey.UpArrow && option > 1)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option - 1;
                    Console.CursorLeft = 0;
                    option--;
                    Console.Write(" > ");
                }
                if (choice.Key == ConsoleKey.DownArrow && option < 3)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    option++;
                    Console.Write(" > ");
                }

                choice = Console.ReadKey(true);
            }




        }

        public static int LetterToInt(char letter)
        {
            string strLetter = letter.ToString().ToUpper();
            char UpperCaseChar = strLetter.ToCharArray()[0];

            return UpperCaseChar - (int)'A';
        }

        public static char intToLetter(int num)
        {
            num += (int)'A';
            char UpperCaseChar = (char)num;

            return UpperCaseChar;
        }

        public static void playGame(ref Board board)
        {
            bool PlayerTurn = true;
            int row = -1;
            int column = -1;
            while (!Board.isFull(ref board))
            {
                //Carries out the players turn
                if (PlayerTurn)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Player One (Black)'s turn...");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (Board.canPlaceCounter(ref board, PlayerTurn))
                    {
                        int tries = 0;
                        while (!Board.checkValidMove(ref board, true, row, column))
                        {
                            tries++;
                            if (tries > 1)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Please enter a correct column and row...");
                                Console.ForegroundColor = ConsoleColor.White;
                            }

                            Console.Write("Enter column: ");
                            try
                            {
                                column = LetterToInt(char.Parse(Console.ReadLine().Substring(0, 1)));
                            }
                            catch (Exception e)
                            {
                                column = -1;
                            }
                            Console.Write("Enter row: ");
                            try
                            {
                                row = int.Parse(Console.ReadLine()) - 1;
                            }
                            catch (Exception e)
                            {
                                row = -1;
                            }


                        }


                        board.placeCounter(ref board, PlayerTurn, row, column);
                        Console.Clear();
                        board.displayBoard();
                    }
                    else
                    {

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Player 1 can't go anywhere! Go skipped...");
                        Console.ForegroundColor = ConsoleColor.White;
                        System.Threading.Thread.Sleep(2000);

                    }
                    PlayerTurn = false;
                }

                else
                {

                    //Carries out the AI's turn
                    if (Board.canPlaceCounter(ref board, PlayerTurn))
                    {

                        Board.coordinate spotPick = new Board.coordinate(0, 0);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("AI (White)'s turn, thinking...");
                        Console.ForegroundColor = ConsoleColor.White;



                        if (difficulty == "Amateur")
                        {
                            System.Threading.Thread.Sleep(1500);
                            spotPick = Board.getBestPlace(ref board, false);
                        }

                        if (difficulty == "Professional")
                        {

                            spotPick = Board.minimaxCall(board);

                        }


                        board.placeCounter(ref board, PlayerTurn, spotPick.row, spotPick.column);
                        Console.Clear();
                        board.displayBoard();
                        Console.WriteLine("AI placed at (" + intToLetter(spotPick.column) + ", " + (spotPick.row + 1) + "), the counters flipped turn yellow shortly.");


                    }

                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Player 2 can't go anywhere! Go skipped...");
                        Console.ForegroundColor = ConsoleColor.White;

                    }
                    PlayerTurn = true;
                }
            }
            Console.WriteLine("GAME OVER, winner was...");
            System.Threading.Thread.Sleep(3000);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Board.checkWinner(ref board) + "!");
            Console.WriteLine("Black Score: " + board.getBlackScore() + ", White Score: " + board.getWhiteScore());

            Console.ReadKey();
        }

        public static int displayMenu()
        {
            Console.WriteLine("Welcome to Othello Player!");
            Console.WriteLine(" > Play Game");
            Console.WriteLine("   Select difficulty");
            Console.WriteLine("   Exit");

            ConsoleKeyInfo choice = Console.ReadKey(true);
            int option = 1;
            while (choice.Key != ConsoleKey.Enter)
            {

                if (choice.Key == ConsoleKey.UpArrow && option > 1)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option - 1;
                    Console.CursorLeft = 0;
                    option--;
                    Console.Write(" >");
                }
                if (choice.Key == ConsoleKey.DownArrow && option < 3)
                {
                    Console.CursorLeft = 0;
                    Console.CursorTop = option;
                    Console.Write("  ");
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    option++;
                    Console.Write(" >");
                }

                choice = Console.ReadKey(true);

            }
            return option;


        }
    }
}