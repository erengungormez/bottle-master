using System;
using System.Linq;
using System.Collections.Generic;
using Random = System.Random;
using UnityEngine;

public class GameUtils
{
    public static readonly Random Random = new Random();

    public static float SelectRandom(float min, float max)
    {
        min *= 1000;
        max *= 1000;
        var integerMin = (int)min;
        var integerMax = (int)max;
        var randomNumber = Random.Next(integerMin, integerMax) / 1000f;
        return randomNumber;
    }

    public static int SelectRandom(int min, int max)
    {
        return Random.Next(min, max + 1);
    }

    public static int[] SelectRandomNumbers(int min, int max, int total, bool reselect)
    {
        if (max - min < total - 1 && !reselect)
        {
            throw new Exception("Could not enforce condition of reselect");
        }
        var numbers = new int[total];
        var selected = 0;
        while (selected < total)
        {
            var number = SelectRandom(min, max);
            if (numbers.Contains(number) && !reselect) continue;
            numbers[selected] = number;
            selected += 1;
        }
        return numbers;
    }

    public static List<List<T>> DivideArray<T>(T[] array, int elementsPerList)
    {
        int arrayLength = array.Length;
        int numberOfLists = (int)Math.Ceiling((double)arrayLength / elementsPerList);

        List<List<T>> dividedList = new List<List<T>>(numberOfLists);

        for (int i = 0; i < numberOfLists; i++)
        {
            int startIndex = i * elementsPerList;
            int count = Math.Min(elementsPerList, arrayLength - startIndex);

            List<T> sublist = array.Skip(startIndex).Take(count).ToList();
            dividedList.Add(sublist);
        }

        return dividedList;
    }

    public static T[] RepeatElements<T>(T[] array, int repeatCount)
    {
        return array.SelectMany(element => Enumerable.Repeat(element, repeatCount)).ToArray();
    }

    public static T[] ShuffleArray<T>(T[] array)
    {
        Random random = new();
        T[] shuffledArray = array.Clone() as T[];

        // Fisher-Yates shuffle algorithm
        for (int i = shuffledArray.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            // Swap elements
            (shuffledArray[j], shuffledArray[i]) = (shuffledArray[i], shuffledArray[j]);
        }

        // Check if any adjacent elements are the same and reshuffle if needed
        while (HasAdjacentDuplicates(shuffledArray))
        {
            shuffledArray = ShuffleArray(array);
        }

        return shuffledArray;
    }

    private static bool HasAdjacentDuplicates<T>(T[] array)
    {
        for (int i = 0; i < array.Length - 1; i++)
        {
            if (EqualityComparer<T>.Default.Equals(array[i], array[i + 1]))
            {
                return true;
            }
        }
        return false;
    }
}