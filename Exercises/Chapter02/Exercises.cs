using System;
using System.ComponentModel;
using Shouldly;
using Xunit;
using static System.Console;

namespace Exercises.Chapter2
{
    // 1. Write a console app that calculates a user's Body-Mass Index:
    //   - prompt the user for her height in metres and weight in kg
    //   - calculate the BMI as weight/height^2
    //   - output a message: underweight(bmi<18.5), overweight(bmi>=25) or healthy weight
    // 2. Structure your code so that structure it so that pure and impure parts are separate
    // 3. Unit test the pure parts
    // 4. Unit test the impure parts using the HOF-based approach

    public static class Bmi
    {
        public enum BmiRange
        {
            Underweight,
            Healthy,
            Overweight,
        }

        public static void Run()
        {
            Run(ReadLineInput, WriteLine);
        }

        private static double ReadLineInput(string s)
        {
            switch (s)
            {
                case "height":
                    WriteLine($"Please enter your {s} in m:");
                    break;
                case "weight":
                    WriteLine($"Please enter your {s} in kg:");
                    break;
                default:
                    throw new Exception("attempted to read input for unsupported dimension");
            }
            return Double.Parse(ReadLine() ?? "0");
        }

        public static void Run(
            Func<string, double> readLineInput,
            Action<string> writeLineInput
        )
        {
            double height = readLineInput("height");
            double weight = readLineInput("weight");
            // note: can make validator
            if (height <= 0)
            {
                WriteLine($"You entered an invalid {nameof(height)}. Can't calculate BMI so exiting.");
                return;
            }

            if (weight <= 0)
            {
                WriteLine($"You entered an invalid {nameof(weight)}. Can't calculate BMI so exiting.");
                return;
            }

            var bmi = Calculate(height, weight);
            var message = GetBmiResult(bmi);

            writeLineInput(FormatBmi(message));
        }

        internal static string FormatBmi(BmiRange bmiResult)
        {
            if (!Enum.IsDefined(typeof(BmiRange), bmiResult))
                throw new InvalidEnumArgumentException(nameof(bmiResult), (int)bmiResult, typeof(BmiRange));
            return bmiResult.ToString().ToLower();
        }

        internal static BmiRange GetBmiResult(double bmi) =>
            bmi >= 25
                ? BmiRange.Overweight
                : bmi >= 18.5
                    ? BmiRange.Healthy
                    : BmiRange.Underweight;

        internal static double Calculate(double height, double weight)
        {
            return weight / Math.Pow(height, 2);
        }
    }

    public static class BmiTest
    {
        [Theory]
        [InlineData(1.72, 72, "healthy")]
        public static void Run_ReturnsCorrectValue(double heightInM, double weightInKg, string expectedResult)
        {
            // arrange
            var readLine = new Func<string, double>(s => s == "height" ? heightInM : weightInKg);
            string actualResult = null;
            var writeLine = new Action<string>((writeString) => actualResult = writeString);
            // act
            Bmi.Run(readLine, writeLine);

            // assert
            actualResult.ShouldBe(expectedResult);
        }

        [Theory]
        [InlineData(Bmi.BmiRange.Overweight, "overweight")]
        [InlineData(Bmi.BmiRange.Healthy, "healthy")]
        [InlineData(Bmi.BmiRange.Underweight, "underweight")]
        public static void GetMessage_ReturnsRightMessageForInput(Bmi.BmiRange bmiResult, string result)
        {
            // arrange
            // act
            var formatted = Bmi.FormatBmi(bmiResult);
            // assert
            formatted.ShouldBe(result);
        }

        [Theory]
        [InlineData(25, Bmi.BmiRange.Overweight)]
        [InlineData(50, Bmi.BmiRange.Overweight)]
        [InlineData(10000000, Bmi.BmiRange.Overweight)]
        [InlineData(20, Bmi.BmiRange.Healthy)]
        [InlineData(24.999999, Bmi.BmiRange.Healthy)]
        [InlineData(18.5, Bmi.BmiRange.Healthy)]
        [InlineData(18, Bmi.BmiRange.Underweight)]
        [InlineData(0, Bmi.BmiRange.Underweight)]
        [InlineData(-1, Bmi.BmiRange.Underweight)]
        public static void GetBmiResult_ReturnsRightMessageForInput(double bmi, Bmi.BmiRange result)
        {
            // arrange
            // act
            var bmiResult = Bmi.GetBmiResult(bmi);
            // assert
            bmiResult.ShouldBe(result);
        }

        [Theory]
        [InlineData(1.72, 72, 24.34)]
        [InlineData(-1, -1, -1)]
        public static void Calculate_ReturnsCorrectValueForInputs(double height, double weight, double expectedBmi)
        {
            // arrange
            // act
            var bmi = Bmi.Calculate(height, weight);
            // assert
            Math.Round(bmi, 2).ShouldBe(Math.Round(expectedBmi, 2));
        }
    }
}
