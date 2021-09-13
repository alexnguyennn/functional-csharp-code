using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using Boc.Commands;
//using System.Configuration;
using LaYumba.Functional;
using Shouldly;
using Xunit;
using static LaYumba.Functional.F;
using Enum = System.Enum;

namespace Exercises.Chapter3
{
   public static class Exercises
   {
      // 1 Write a generic function that takes a string and parses it as a value of an enum. It
      // should be usable as follows:

      // Enum.Parse<DayOfWeek>("Friday") // => Some(DayOfWeek.Friday)
      // Enum.Parse<DayOfWeek>("Freeday") // => None
      public static Option<DayOfWeek> Parse(string s) =>
         Enum.TryParse(s, out DayOfWeek result)
            ? Some(result)
            : None;

      [Fact]
      public static void WhenParseCalledOnDayOfWeek_GetOptionWithSome()
      {
         // arrange
         var day = "Friday";
         // act
         var result = Parse(day);
         // assert
         result.ShouldBe(Some(DayOfWeek.Friday));
         result.ShouldBe(DayOfWeek.Friday);
         result.ShouldBeOfType<Option<DayOfWeek>>();
      }

      [Fact]
      public static void WhenParseCalledWithInvalidDay_ReturnsNone()
      {
         // arrange
         var day = "Freeday";
         // act
         var result = Parse(day);
         // assert
         result.ShouldBe(None);
      }

      // 2 Write a Lookup function that will take an IEnumerable and a predicate, and
      // return the first element in the IEnumerable that matches the predicate, or None
      // if no matching element is found. Write its signature in arrow notation:

      // bool isOdd(int i) => i % 2 == 1;
      // new List<int>().Lookup(isOdd) // => None
      // new List<int> { 1 }.Lookup(isOdd) // => Some(1)
      // List<T> --> (T -> bool) --> Option<T>
      // using linq
      // public static Option<T> Lookup<T>(this List<T> list, Func<T, bool> predicate) =>
      //    list.Where(predicate).Any()
      //       ? Some(list.Where(predicate).FirstOrDefault())
      //       : None;

      public static Option<T> Lookup<T>(this List<T> list, Func<T, bool> predicate)
      {
         foreach (var item in list)
         {
            if (predicate(item)) return Some(item);
         }

         return None;
      }

      [Fact]
      public static void Lookup_WithNoMatch_ReturnNone()
      {
         // arrange
         var list = new List<int> { 5, 10, 15 };
         // act
         var result = list.Lookup((item) => item % 12 == 0);

         // assert
         result.ShouldBe(None);
         result.ShouldBeOfType<Option<int>>();
      }

      [Fact]
      public static void Lookup_WithMatch_ReturnFirstSome()
      {
         // arrange
         var list = new List<int> { 5, 10, 15 };
         // act
         var result = list.Lookup((item) => item % 5 == 0);

         // assert
         result.ShouldBe(5);
         result.ShouldBe(Some(5));
         result.ShouldBeOfType<Option<int>>();
      }


      // 3 Write a type Email that wraps an underlying string, enforcing that it’s in a valid
      // format. Ensure that you include the following:
      // - A smart constructor
      // - Implicit conversion to string, so that it can easily be used with the typical API
      // for sending emails

      public struct Email
      {
         private string Value { get;  }
         static readonly Regex Regex = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

         private Email(string value) => Value = value;
            // if (!IsValid(value))
            // {
            //    throw new ArgumentException($"{value} is not a valid email");
            // }

         public static Option<Email> Create(string value) => Regex.IsMatch(value) ? Some(new Email(value)) : None;

         // private static bool IsValid(string value) => value.Contains("@") && value.Length < 20;

         public static implicit operator string(Email e) => e.Value;
      }

      [Fact]
      public static void WhenCreated_ConvertsToString()
      {
         // arrange
         var expectedString = "abc@def.com";
         // act
         var result = Email.Create(expectedString);
         var email = result.Match(() => "email was empty.. womp womp", (e) => e);
         // assert
         email.ShouldBe(expectedString);
      }

      // 4 Take a look at the extension methods defined on IEnumerable inSystem.LINQ.Enumerable.
      // Which ones could potentially return nothing, or throw some
      // kind of not-found exception, and would therefore be good candidates for
      // returning an Option<T> instead?
      // TODO: check ienums

   }

   // 5.  Write implementations for the methods in the `AppConfig` class
   // below. (For both methods, a reasonable one-line method body is possible.
   // Assume settings are of type string, numeric or date.) Can this
   // implementation help you to test code that relies on settings in a
   // `.config` file?
   public class AppConfig
   {
      NameValueCollection source;

      //public AppConfig() : this(ConfigurationManager.AppSettings) { }

      public AppConfig(NameValueCollection source)
      {
         this.source = source;
      }

      public Option<T> Get<T>(string name)
      {
         return source[name] == null ? None : Some((T)Convert.ChangeType(source[name], typeof(T)));
      }

      public T Get<T>(string name, T defaultValue)
      {
         return Get<T>(name).Match((
            () => defaultValue),
            (val) => val);
      }
   }
}
