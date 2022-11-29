using System;
using System.Collections.Generic;
using System.Linq;

namespace DZarudnyi_Sudoku
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] testMatr1 =
            {
                { 0, 1, 3, 8, 0, 0, 4, 0 ,5 },
                { 0, 2, 4, 6, 0, 5, 0, 0, 0 },
                { 0, 8, 7, 0, 0, 0, 9, 3, 0 },
                { 4, 9, 0, 3, 0, 6, 0, 0, 0 },
                { 0, 0, 1, 0, 0, 0, 5, 0, 0 },
                { 0, 0, 0, 7, 0, 1, 0, 9, 3 },
                { 0, 6, 9, 0, 0, 0, 7, 4, 0 },
                { 0, 0, 0, 2, 0, 7, 6, 8, 0 },
                { 1, 0, 2, 0, 0, 8, 3, 5, 0 }
            };
            int[,] testMatr2 =
            {
                { 0, 0, 2, 0, 0, 0, 0, 4 ,1 },
                { 0, 0, 0, 0, 8, 2, 0, 7, 0 },
                { 0, 0, 0, 0, 4, 0, 0, 0, 9 },
                { 2, 0, 0, 0, 7, 9, 3, 0, 0 },
                { 0, 1, 0, 0, 0, 0, 0, 8, 0 },
                { 0, 0, 6, 8, 1, 0, 0, 0, 4 },
                { 1, 0, 0, 0, 9, 0, 0, 0, 0 },
                { 0, 6, 0, 4, 3, 0, 0, 0, 0 },
                { 8, 5, 0, 0, 0, 0, 4, 0, 0 }
            };

            List<string> backtrack = new List<string>();
            List<int>[] usedInRows = new List<int>[9];
            List<int>[] usedInColumns = new List<int>[9];
            List<int>[] usedInBlocks = new List<int>[9];
            for (int i = 0; i < 9; i++)
            {
                usedInRows[i] = new List<int>();
                usedInColumns[i] = new List<int>();
                usedInBlocks[i] = new List<int>();
            }
            Dictionary<string, string> coordinatesCandidates = new Dictionary<string, string>();

            int[,] matrix = testMatr1;

            bool solvable = Backtrack(ref matrix, ref backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
            if (!solvable)
                Console.WriteLine("Not solvable");
            else
                PrintMatrix(matrix);

            Console.ReadLine();
        }

        //Search if there is an empty cell left
        static bool FindEmptyCell(int[,] matrix)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] == 0)
                        return true;
                }
            }

            return false;
        }

        //Search for every cell with 0 in it
        static bool FindAllEmptyCells(int[,] matrix, ref List<string> coordinates)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        coordinates.Add(i.ToString() + j.ToString());
                    }
                }
            }

            if (coordinates.Count > 0)
                return true;
            else
                return false;
        }

        //Searches for all numbers, that been used in every row, column and block, to create a list of "restricted" elements
        static void FindAllUsedNumbers(int[,] matrix, ref List<int>[] usedInRows, ref List<int>[] usedInColumns, ref List<int>[] usedInBlocks)
        {
            for (int k = 0; k < 9; k++)
            {
                usedInRows[k].Clear();
                usedInColumns[k].Clear();
                usedInBlocks[k].Clear();
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (matrix[i, j] != 0)
                    {
                        usedInRows[i].Add(matrix[i, j]);
                        usedInColumns[j].Add(matrix[i, j]);
                        usedInBlocks[(i / 3 * 3) + (j / 3)].Add(matrix[i, j]);
                    }
                }
            }
        }

        //Creating list of possible elements to fill for every empty cell in matrix
        static bool FindAllCandidates(int[,] matrix, ref Dictionary<string, string> coordinatesCandidates, List<int>[] usedInRows, List<int>[] usedInColumns, List<int>[] usedInBlocks)
        {
            List<string> coordinates = new List<string>();
            coordinatesCandidates.Clear();
            if (!FindAllEmptyCells(matrix, ref coordinates)) return true;
            FindAllUsedNumbers(matrix, ref usedInRows, ref usedInColumns, ref usedInBlocks);

            for (int i = 0; i < coordinates.Count; i++)
            {
                int x = Int32.Parse(coordinates.ElementAt(i)[0].ToString());
                int y = Int32.Parse(coordinates.ElementAt(i)[1].ToString());
                string cand = "";
                for (int j = 1; j < 10; j++)
                {
                    if (!usedInRows[x].Contains(j) && !usedInColumns[y].Contains(j) && !usedInBlocks[(x / 3 * 3) + y / 3].Contains(j))
                        cand += j.ToString();
                }
                if (cand == "")
                    return false;
                else
                    coordinatesCandidates.Add(coordinates[i], cand);
            }

            return true;
        }

        //Search and fill a cell with only one candidate
        static bool FindNakedSingle(ref int[,] matrix, List<string> backtrack, ref Dictionary<string, string> coordinatesCandidates, ref List<int>[] usedInRows, ref List<int>[] usedInColumns, ref List<int>[] usedInBlocks)
        {
            foreach (KeyValuePair<string, string> cc in coordinatesCandidates)
            {
                if (cc.Value.Length == 1)
                {
                    AddValues(ref matrix, cc.Key, cc.Value, ref backtrack);

                    FindAllCandidates(matrix, ref coordinatesCandidates, usedInRows, usedInColumns, usedInBlocks);
                    return true;
                }
            }

            return false;
        }

        //Serach for naked single, if not -> use backtracking method. Repeat.
        static bool Backtrack(ref int[,] matrix, ref List<string> backtrack, ref Dictionary<string, string> coordinatesCandidates, ref List<int>[] usedInRows, ref List<int>[] usedInColumns, ref List<int>[] usedInBlocks)
        {
            if (!FindAllCandidates(matrix, ref coordinatesCandidates, usedInRows, usedInColumns, usedInBlocks))
                return false;

            bool nakedSingle = true;
            int nakedSinglesCycled = 0;
            while (nakedSingle)
            {
                nakedSingle = FindNakedSingle(ref matrix, backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
                if (nakedSingle)
                {
                    nakedSinglesCycled++;
                    if (!FindAllCandidates(matrix, ref coordinatesCandidates, usedInRows, usedInColumns, usedInBlocks) && FindEmptyCell(matrix))//Чи треба додаткова перевірка?
                    {
                        if (nakedSinglesCycled > 0)
                        {
                            for (int i = 0; i < nakedSinglesCycled; i++)
                            {
                                DeleteValues(ref matrix, backtrack.Last(), ref backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
                            }
                        }
                        return false;
                    }
                }
            }

            if (FindEmptyCell(matrix))
            {
                bool result = false;
                int length = 10;
                string key = "";

                foreach (KeyValuePair<string, string> cc in coordinatesCandidates)
                {
                    if (cc.Value.Length < length)
                    {
                        key = cc.Key;
                        length = cc.Value.Length;
                    }
                }
                if (key == "")
                    return false;

                string value = coordinatesCandidates[key];

                for (int i = 0; i < value.Length; i++)
                {
                    AddValues(ref matrix, key, value[i].ToString(), ref backtrack);

                    result = Backtrack(ref matrix, ref backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
                    
                    if (result)
                        break;
                    else
                        DeleteValues(ref matrix, key, ref backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
                }
                if (!result)
                {
                    for (int i = 0; i < nakedSinglesCycled; i++)
                        DeleteValues(ref matrix, backtrack.Last(), ref backtrack, ref coordinatesCandidates, ref usedInRows, ref usedInColumns, ref usedInBlocks);
                }
                return result;
            }
            else
                return true;
        }

        //Add value to matrix and to backtracking list
        static void AddValues(ref int[,] matrix, string coordinates, string value, ref List<string> backtrack)
        {
            int x = Int32.Parse(coordinates[0].ToString());
            int y = Int32.Parse(coordinates[1].ToString());

            matrix[x, y] = Int32.Parse(value);
            backtrack.Add(coordinates);
        }

        //Delete value from matrix and backtracking list, then remake candidates list
        static void DeleteValues(ref int[,] matrix, string coordinates, ref List<string> backtrack, ref Dictionary<string, string> coordinatesCandidates, ref List<int>[] usedInRows, ref List<int>[] usedInColumns, ref List<int>[] usedInBlocks)
        {
            int x = Int32.Parse(coordinates[0].ToString());
            int y = Int32.Parse(coordinates[1].ToString());

            matrix[x, y] = 0;
            backtrack.RemoveAt(backtrack.Count - 1);

            FindAllCandidates(matrix, ref coordinatesCandidates, usedInRows, usedInColumns, usedInBlocks);
        }

        static void PrintMatrix(int[,] matrix)
        {
            Console.Clear();
            for (int i = 0; i < 9; i++)
            {
                if (i % 3 == 0) Console.WriteLine("--------------------");
                string str = "";
                for (int j = 0; j < 9; j++)
                {
                    if (j % 3 == 0) str += "|";
                    str += matrix[i, j] + " ";
                }
                Console.WriteLine(str);
            }
        }
    }
}
