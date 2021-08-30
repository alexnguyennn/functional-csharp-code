using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Exercises.Chapter1
{
    public static class Exercises
    {
        // 1. Write a function that negates a given comparison: whenvever the given comparison
        // evaluates to `true`, the resulting function evaluates to `false`, and vice versa.
        static bool Negate(Func<bool> predicate) => !predicate();

        // 2. Write a method that uses quicksort to sort a `List<int>` (return a new list,
        // rather than sorting it in place).
        static List<int> QuickSort(this List<int> list)
        {
            if (list.Count == 0) return new List<int>();

            var pivot = list.First();
            var rest = list.Skip(1);
            var smaller = rest.Where(elt => elt <= pivot).ToList();
            var larger = rest.Where(elt => elt > pivot).ToList();

            return smaller.QuickSort()
                .Append(pivot)
                .Concat(larger.QuickSort())
                .ToList();
        }

        [Test]
        public static void TestQuickSort()
        {
            var list = new List<int> { -100, 63, 30, 45, 1, 1000, -23, -67, 1, 2, 56, 75, 975, 432, -600, 193, 85, 12 };
            var expected = new List<int>
                { -600, -100, -67, -23, 1, 1, 2, 12, 30, 45, 56, 63, 75, 85, 193, 432, 975, 1000 };
            var actual = list.QuickSort();
            Console.WriteLine(JsonConvert.SerializeObject(actual));
            Console.WriteLine($"expected: {JsonConvert.SerializeObject(expected)}");
             Assert.AreEqual(expected, actual);
        }

        // 3. Generalize your implementation to take a `List<T>`, and additionally a
        // `Comparison<T>` delegate.

        static List<T> QuickSort<T>(this List<T> list, Comparison<T> comparison)
        {
            if (list.Count == 0) return new List<T>();

            var pivot = list.First();
            var rest = list.Skip(1);
            var smaller = rest.Where(elt => comparison(elt, pivot) <= 0).ToList();
            var larger = rest.Where(elt => comparison(elt, pivot) > 0).ToList();

            return smaller.QuickSort(comparison)
                .Append(pivot)
                .Concat(larger.QuickSort(comparison))
                .ToList();
        }

        [Test]
        public static void TestQuickSortGeneric()
        {
            var list = new List<int> { -100, 63, 30, 45, 1, 1000, -23, -67, 1, 2, 56, 75, 975, 432, -600, 193, 85, 12 };
            var expected = new List<int>
                { -600, -100, -67, -23, 1, 1, 2, 12, 30, 45, 56, 63, 75, 85, 193, 432, 975, 1000 };

            var comparison = new Comparison<int>((elt, pivot) => elt - pivot <= 0 ? -1 : 1);
            var actual = list.QuickSort(comparison);
             Assert.AreEqual(expected, actual);
        }


        // 4. In this chapter, you've seen a `Using` function that takes an `IDisposable`
        // and a function of type `Func<TDisp, R>`. Write an overload of `Using` that
        // takes a `Func<IDisposable>` as first
        // parameter, instead of the `IDisposable`. (This can be used to fix warnings
        // given by some code analysis tools about instantiating an `IDisposable` and
        // not disposing it.)
        static TReturn Using<TDisp, TReturn>(
            Func<TDisp> createDisposable,
            Func<TDisp, TReturn> func
            ) where TDisp : IDisposable
        {
            using (var disposable = createDisposable()) return func(disposable);
        }
    }
}
