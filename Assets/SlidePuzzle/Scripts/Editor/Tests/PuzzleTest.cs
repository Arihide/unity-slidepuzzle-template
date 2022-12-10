using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PuzzleTest
{
    private List<int> tempList = new List<int>();

    [Test]
    public void CheckSolutionExistanceTest()
    {
        // Algorithm: https://manabitimes.jp/math/979

        Assert.IsTrue(Puzzle.CheckSolutionExistance(new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 }));
        Assert.IsTrue(Puzzle.CheckSolutionExistance(new List<int>() { 1, 0, 2, 3, 8, 5, 6, 7, 4 }));
        Assert.IsTrue(Puzzle.CheckSolutionExistance(new List<int>() { 6, 7, 1, 2, 3, 0, 8, 5, 4 }));
        Assert.IsTrue(Puzzle.CheckSolutionExistance(new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }));

        Assert.IsFalse(Puzzle.CheckSolutionExistance(new List<int>() { 0, 1, 2, 3, 4, 5, 6, 8, 7 }));
        Assert.IsFalse(Puzzle.CheckSolutionExistance(new List<int>() { 0, 1, 2, 3, 4, 5, 6, 8, 7 }));
        Assert.IsFalse(Puzzle.CheckSolutionExistance(new List<int>() { 3, 2, 7, 8, 0, 4, 1, 5, 6 }));
    }

    [Test]
    public void SolvableShuffleTest()
    {
        for (int i = 0; i < 1000; i++)
        {
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, 9)));
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, 16)));
        }
    }

    [Test]
    public void CheckAllSizeTest()
    {
        for (int i = GameManager.MinSize; i <= GameManager.MaxSize; i++)
        {
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, i * i)));
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, i * i)));
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, i * i)));
        }
    }

    [Test]
    public void SolvableShuffleDeepTest()
    {
        for (int i = 0; i < 1000000; i++)
        {
            Assert.IsTrue(Puzzle.CheckSolutionExistance(Puzzle.Shuffle(tempList, 16)));
        }
    }
}
