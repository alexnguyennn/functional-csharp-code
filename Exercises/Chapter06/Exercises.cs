using LaYumba.Functional;
using static LaYumba.Functional.F;
using System;
using System.Linq;
using Examples.Chapter3;
using Shouldly;
using Xunit;
using Unit = System.ValueTuple;

namespace Exercises.Chapter6
{
    public static class Exercises
    {
        // 1. Write a `ToOption` extension method to convert an `Either` into an
        // `Option`. Then write a `ToEither` method to convert an `Option` into an
        // `Either`, with a suitable parameter that can be invoked to obtain the
        // appropriate `Left` value, if the `Option` is `None`. (Tip: start by writing
        // the function signatures in arrow notation)

        // Either<L, R> -> Option<R>
        public static Option<R> ToOption<L, R>(this Either<L, R> either) =>
            either.Match(
                _ => None,
                (r) => Some(r)
            );

        [Fact]
        public static void ToOption_HasExpectedOptionNone()
        {
            // arrange
            Either<string, int> either = Left("oops!");

            // act
            var result = either.ToOption();
            // assert
            result.ShouldBe(None);
            result.ShouldBeOfType<Option<int>>();
        }

        [Fact]
        public static void ToOption_HasExpectedOptionValue()
        {
            // arrange
            Either<string, int> either = Right(12);

            // act
            var result = either.ToOption();
            // assert
            result.ShouldBe(Some(12));
            result.ShouldBeOfType<Option<int>>();
        }

        // Option<R> -> () -> L -> Either<L, R>
        public static Either<L, R> ToEither<L, R>(this Option<R> opt, Func<Either<L, R>> lValueOnNone)
            =>
                opt.Match(lValueOnNone, r => Right(r));

        [Fact]
        public static void ToEither_HasExpectedValueLeft()
        {
            // arrange
            Option<int> opt = None;
            const string lMessage = "big oops! something went wrong.";
            Func<Either<string, int>> getLValue = () => Left(lMessage);

            // act
            var result = opt.ToEither(getLValue);
            // assert
            result.ShouldBe(Left(lMessage));
            result.ShouldBeOfType<Either<string, int>>();
        }

        [Fact]
        public static void ToEither_HasExpectedValueRight()
        {
            // arrange
            const int value = 12348;
            Option<int> opt = Some(value);
            const string lMessage = "big oops! something went wrong.";
            Func<Either<string, int>> getLValue = () => Left(lMessage);

            // act
            var result = opt.ToEither(getLValue);
            // assert
            result.ShouldBe(Right(value));
            result.ShouldBeOfType<Either<string, int>>();
        }

        // 2. Take a workflow where 2 or more functions that return an `Option`
        // are chained using `Bind`.


        // example taken from chapter 3
        // static Func<string, Option<Age>> parseAge = s
        //    => Int.Parse(s).ToEither<string, int>()
        //       .Bind(Age.Of);

        // Either<L,R> -> (Either<L, R> -> Option<RR>) -> Option<RR>

        // Then change the first one of the functions to return an `Either`.

        // This should cause compilation to fail. Since `Either` can be
        // converted into an `Option` as we have done in the previous exercise,
        // write extension overloads for `Bind`, so that
        // functions returning `Either` and `Option` can be chained with `Bind`,
        // yielding an `Option`.

        public static Option<RR> Bind<L, R, RR>(this Either<L, R> @this, Func<Either<L, R>, Option<RR>> f)
            =>
                @this.Match(
                    _ => None,
                    (r) => f(r)
                );


        // 3. Write a function `Safely` of type ((() → R), (Exception → L)) → Either<L, R> that will
        // run the given function in a `try/catch`, returning an appropriately
        // populated `Either`.
        public static Either<L, R> Safely<L, R>(Func<R> f, Func<Exception, L> getExceptionMessage)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                return getExceptionMessage(ex);
            }
        }

        [Fact]
        public static void WhenRFuncThrows_ReturnExceptionMessage()
        {
            // arrange
            const string message = "something went wrong!";
            Func<int> f = () => throw new ArgumentException(message);
            Func<Exception, string> getExceptionMessage = exception => exception.Message;

            // act
            var result = Safely(f, getExceptionMessage);
            // assert
            result.ShouldBe(Left(message));
            result.ShouldBeOfType<Either<string, int>>();
        }

        [Fact]
        public static void WhenRFuncWorks_ReturnR()
        {
            // arrange
            const int i = 12345;
            Func<int> f = () => i;
            Func<Exception, string> getExceptionMessage = exception => exception.Message;

            // act
            var result = Safely(f, getExceptionMessage);
            // assert
            result.ShouldBe(Right(i));
            result.ShouldBeOfType<Either<string, int>>();
        }

        // 4. Write a function `Try` of type (() → T) → Exceptional<T> that will
        // run the given function in a `try/catch`, returning an appropriately
        // populated `Exceptional`.
        public static Exceptional<T> Try<T>(Func<T> f)
        {
            try
            {
                return f();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [Fact]
        public static void WhenFuncThrows_ReturnsException()
        {
            // arrange
            const string message = "something went wrong!";
            Func<int> getter = () => throw new ArgumentException(message);

            // act
            var result = Try(getter);

            // assert
            result.Success.ShouldBeFalse();
            result.Exception.ShouldBeTrue();
            string shouldNotBeSet = null;
            result.Match(
                (ex) =>
                {
                    ex.Message.ShouldBe(message);
                    ex.ShouldBeOfType<ArgumentException>();
                },
                (_) => { shouldNotBeSet = "not empty!"; });

            shouldNotBeSet.ShouldBeNull();
        }

        [Fact]
        public static void WhenFuncReturnsValue_ReturnsValue()
        {
            // arrange
            const string message = "something went wrong!";
            var i = 234234;
            Func<int> getter = () => i;
            string shouldBeSet = null;
            string shouldNotBeSet = null;
            var notEmpty = "not empty!";

            // act
            var result = Try(getter);

            // assert
            result.Success.ShouldBeTrue();
            result.Exception.ShouldBeFalse();
            result.Match(
                (ex) =>
                {
                    shouldNotBeSet = notEmpty;
                },
                (val) =>
                {
                    shouldBeSet = notEmpty;
                    val.ShouldBe(i);
                });

            shouldBeSet.ShouldBe(notEmpty);
            shouldNotBeSet.ShouldBeNull();
        }
    }
}
