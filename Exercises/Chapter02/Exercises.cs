using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
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
      public static void Run()
      {
         WriteLine("Please enter your height in m:");
         double height = Double.Parse(ReadLine() ?? "0");
         WriteLine("Please enter your weight in kg:");
         double weight = Double.Parse(ReadLine() ?? "0");
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
         WriteLine(GetMessage(bmi));
         // TODO: update projects to use .net 5/6?

      }

      private static string GetMessage(double bmi) =>
         bmi >= 25
            ? "overweight"
            : bmi >= 18.5
               ? "healthy weight"
               : "underweight";

      private static double Calculate(double height, double weight)
      {
         return weight / Math.Pow(height, 2);
      }
   }
}
