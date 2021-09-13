using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;
using LaYumba.Functional;
using LaYumba.Functional.Option;
using Shouldly;
using Xunit;

namespace Exercises.Chapter4
{
    using static F;

    public static class Exercises
    {
        // 1 Implement Map for ISet<T> and IDictionary<K, T>. (Tip: start by writing down
        // the signature in arrow notation.)
        public static ISet<R> Map<T, R>(this ISet<T> set, Func<T, R> f)
        {
            var newSet = new HashSet<R>();
            foreach (var item in set)
            {
                newSet.Add(f(item));
            }

            return newSet;
        }

        [Fact]
        public static void Map_AppliesInParseGivenFunc()
        {
            // arrange
            var f = new Func<string, int>((s) => Int32.Parse(s));
            var testSet = new HashSet<string>(new List<string>(new[] { "5", "10", "5" }));
            var expectedSet = new HashSet<int>(new List<int>(new[] { 5, 10 }));

            // act
            var result = testSet.Map(f);

            // assert
            result.ShouldBe(expectedSet);
        }

        public static IDictionary<K, R> Map<K, T, R>(this IDictionary<K, T> dict, Func<T, R> f)
        {
            var newDict = new Dictionary<K, R>();
            foreach (var key in dict.Keys)
            {
                newDict[key] = f(dict[key]);
            }

            return newDict;
        }

        [Fact]
        public static void Map_AppliesToValuesInParseGivenFunc()
        {
            // arrange
            var f = new Func<string, int>((s) => Int32.Parse(s));
            var testDict = new Dictionary<string, string>
            {
                { "one", "10" },
                { "asdfjsdkf", "100000" },
                { "values", "105" },
                { "again", "-1" },
            };
            var expectedDict = new Dictionary<string, int>
            {
                { "one", 10 },
                { "asdfjsdkf", 100000 },
                { "values", 105 },
                { "again", -1 },
            };

            // act
            var result = testDict.Map(f);

            // assert
            result.ShouldBe(expectedDict);
        }


        // 2 Implement Map for Option and IEnumerable in terms of Bind and Return.
        // use bind/ return
        public static Option<R> Map<T, R>(this Option<T> option, Func<T, R> f) =>
            option.Bind(innerVal => Some(f(innerVal)));

        public static IEnumerable<R> Map<T, R>(this IEnumerable<T> enumerable, Func<T, R> f) =>
            enumerable.Bind(t => List(f(t)));
        // TODO: test both

        // 3 Use Bind and an Option-returning Lookup function (such as the one we defined
        // in chapter 3) to implement GetWorkPermit, shown below.

        public static Option<R> Lookup<K, R>(this IDictionary<K, R> dict, K key) =>
            dict.TryGetValue(key, out R val) ? Some(val) : F.None;

        // Then enrich the implementation so that `GetWorkPermit`
        // returns `None` if the work permit has expired.

        static Option<WorkPermit> GetWorkPermit(Dictionary<string, Employee> people, string employeeId) =>
            people.Lookup(employeeId).Bind(employee => employee.WorkPermit);

        [Fact]
        public static void GetWorkPermit_GetsCorrectValue()
        {
            // arrange
            var empId = GetRandom.String(10);
            var workPermit = Builder<WorkPermit>.CreateNew().Build();
            var emp = Builder<Employee>
                .CreateNew()
                .With(m => m.WorkPermit, workPermit)
                .Build();
            var people = new Dictionary<string, Employee> { { empId, emp } };
            // act
            var result = GetWorkPermit(people, empId);
            // assert
            result.ShouldBe(Some(workPermit));
            result.ShouldBe(workPermit);
            result.ShouldBeOfType<Option<WorkPermit>>();
        }

        [Fact]
        public static void GetWorkPermit_EmployeeNotInPeople_GetsCorrectValue()
        {
            // arrange
            var empId = GetRandom.String(10);
            var workPermit = Builder<WorkPermit>.CreateNew().Build();
            var emp = Builder<Employee>
                .CreateNew()
                .With(m => m.WorkPermit, workPermit)
                .Build();
            var people = new Dictionary<string, Employee> { };
            // act
            var result = GetWorkPermit(people, empId);
            // assert
            result.ShouldBe(None);
            result.ShouldBeOfType<Option<WorkPermit>>();
        }

        // 4 Use Bind to implement AverageYearsWorkedAtTheCompany, shown below (only
        // employees who have left should be included).

        static double AverageYearsWorkedAtTheCompany(List<Employee> employees) =>
            employees
                .Bind(e => e.LeftOn.Map(leftOn => YearsBetween(leftOn, e.JoinedOn)))
                .Average();

        private static int YearsBetween(DateTime leftOn, DateTime joinedOn) => (leftOn - joinedOn).Days / 365;

        [Fact]
        public static void AverageYearsWorkedAtCompany_WithEmployeesWhoHaveAllLeft_GivesExpectedValue()
        {
            // arrange
            var startDateTime = GetRandom.DateTime();
            var duration = 30;
            var expectedEmployees = Builder<Employee>
                .CreateListOfSize(10)
                .All()
                .With(m => m.JoinedOn, startDateTime)
                .With(m => m.LeftOn, Some(startDateTime.AddYears(duration)))
                .Build()
                .ToList();

            // act
            var result = AverageYearsWorkedAtTheCompany(expectedEmployees);

            // assert
            result.ShouldBe(duration);
        }

        [Fact]
        public static void AverageYearsWorkedAtCompany_WithNoEmployeesThatHaveLeft_Throws()
        {
            // only supposed to count left employees
            // arrange
            var startDateTime = GetRandom.DateTime();
            var expectedEmployees = Builder<Employee>
                .CreateListOfSize(10)
                .All()
                .With(m => m.JoinedOn, startDateTime)
                .With(m => m.LeftOn, None)
                .Build()
                .ToList();

            // act
            var exception = Should.Throw<InvalidOperationException>(() => AverageYearsWorkedAtTheCompany(expectedEmployees));

            // assert
            exception.Message.ShouldBe("Sequence contains no elements");
        }
    }

    public struct WorkPermit
    {
        public string Number { get; set; }
        public DateTime Expiry { get; set; }
    }

    public class Employee
    {
        public string Id { get; set; }
        public Option<WorkPermit> WorkPermit { get; set; }

        public DateTime JoinedOn { get; set; }
        public Option<DateTime> LeftOn { get; set; }
    }
}
