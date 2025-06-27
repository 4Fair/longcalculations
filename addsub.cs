using System;
using System.Text;

public class VeryLongNumber
{
    private const int Base = 10;
    private string integerPart;
    private string fractionalPart;
    private bool isNegative;

    public VeryLongNumber(string num)
    {
        if (string.IsNullOrEmpty(num))
            throw new ArgumentException("Число не може бути нульовим або не існувати. (•˕ •マ.ᐟ");

        if (num[0] == '-')
        {
            isNegative = true;
            num = num.Substring(1);
        }
        else
        {
            isNegative = false;
        }

        string[] parts = num.Split('.');
        if (parts.Length > 2)
            throw new ArgumentException("Такий формат числа неприпустимий.");

        integerPart = parts[0];
        fractionalPart = parts.Length > 1 ? parts[1] : "";

        foreach (char c in integerPart)
        {
            if (!char.IsDigit(c))
                throw new ArgumentException("Такий формат числа неприпустимий.");
        }

        foreach (char c in fractionalPart)
        {
            if (!char.IsDigit(c))
                throw new ArgumentException("Такий формат числа неприпустимий.");
        }

        integerPart = integerPart.TrimStart('0');
        if (string.IsNullOrEmpty(integerPart))
            integerPart = "0";

        fractionalPart = fractionalPart.TrimEnd('0');
    }

    public static VeryLongNumber operator +(VeryLongNumber a, VeryLongNumber b)
    {
        AlignFractionalParts(ref a, ref b);

        string intA = a.integerPart;
        string fracA = a.fractionalPart;
        string intB = b.integerPart;
        string fracB = b.fractionalPart;

        if (a.isNegative == b.isNegative)
        {
            string sumInt = AddStrings(intA, intB);
            string sumFrac = AddStrings(fracA, fracB);

            if (sumFrac.Length > Math.Max(fracA.Length, fracB.Length))
            {
                sumInt = AddStrings(sumInt, "1");
                sumFrac = sumFrac.Substring(1);
            }

            string result = sumInt + (string.IsNullOrEmpty(sumFrac) ? "" : "." + sumFrac);
            return new VeryLongNumber((a.isNegative ? "-" : "") + result);
        }

        if (a.isNegative)
            return b - new VeryLongNumber(a.integerPart + (string.IsNullOrEmpty(a.fractionalPart) ? "" : "." + a.fractionalPart));
        else
            return a - new VeryLongNumber(b.integerPart + (string.IsNullOrEmpty(b.fractionalPart) ? "" : "." + b.fractionalPart));
    }

    public static VeryLongNumber operator -(VeryLongNumber a, VeryLongNumber b)
    {
        AlignFractionalParts(ref a, ref b);

        string intA = a.integerPart;
        string fracA = a.fractionalPart;
        string intB = b.integerPart;
        string fracB = b.fractionalPart;

        if (!a.isNegative && !b.isNegative)
        {
            int comparison = CompareStrings(intA, intB);
            if (comparison == 0)
                comparison = CompareStrings(fracA, fracB);

            if (comparison >= 0)
            {
                string diffInt = SubtractStrings(intA, intB);
                string diffFrac = SubtractFractional(fracA, fracB);

                if (diffFrac.StartsWith("-"))
                {
                    diffInt = SubtractStrings(diffInt, "1");
                    diffFrac = AddStrings(diffFrac.Substring(1), GetPowerOfTen(fracA.Length));
                }

                string result = diffInt + (string.IsNullOrEmpty(diffFrac) ? "" : "." + diffFrac);
                return new VeryLongNumber(result);
            }
            else
            {
                string diffInt = SubtractStrings(intB, intA);
                string diffFrac = SubtractFractional(fracB, fracA);

                if (diffFrac.StartsWith("-"))
                {
                    diffInt = SubtractStrings(diffInt, "1");
                    diffFrac = AddStrings(diffFrac.Substring(1), GetPowerOfTen(fracA.Length));
                }

                string result = "-" + diffInt + (string.IsNullOrEmpty(diffFrac) ? "" : "." + diffFrac);
                return new VeryLongNumber(result);
            }
        }

        if (a.isNegative && b.isNegative)
        {
            // (-a) - (-b) = b - a
            return new VeryLongNumber(b.integerPart + (string.IsNullOrEmpty(b.fractionalPart) ? "" : "." + b.fractionalPart)) -
                   new VeryLongNumber(a.integerPart + (string.IsNullOrEmpty(a.fractionalPart) ? "" : "." + a.fractionalPart));
        }

        if (a.isNegative)
        {
            // (-a) - b = -(a + b)
            VeryLongNumber sum = new VeryLongNumber(a.integerPart + (string.IsNullOrEmpty(a.fractionalPart) ? "" : "." + a.fractionalPart)) +
                               new VeryLongNumber(b.integerPart + (string.IsNullOrEmpty(b.fractionalPart) ? "" : "." + b.fractionalPart));
            return new VeryLongNumber("-" + sum.ToString());
        }
        else
        {
            // a - (-b) = a + b
            return a + new VeryLongNumber(b.integerPart + (string.IsNullOrEmpty(b.fractionalPart) ? "" : "." + b.fractionalPart));
        }
    }

    private static void AlignFractionalParts(ref VeryLongNumber a, ref VeryLongNumber b)
    {
        int maxFracLength = Math.Max(a.fractionalPart.Length, b.fractionalPart.Length);
        a.fractionalPart = a.fractionalPart.PadRight(maxFracLength, '0');
        b.fractionalPart = b.fractionalPart.PadRight(maxFracLength, '0');
    }

    private static string SubtractFractional(string num1, string num2)
    {
        string result = SubtractStrings(num1, num2);
        if (result.StartsWith("-"))
        {
            return result;
        }
        return result.TrimEnd('0');
    }

    private static string GetPowerOfTen(int length)
    {
        return "1" + new string('0', length);
    }

    public static string AddStrings(string num1, string num2)
    {
        StringBuilder result = new StringBuilder();
        int carry = 0;
        int i = num1.Length - 1;
        int j = num2.Length - 1;

        while (i >= 0 || j >= 0 || carry > 0)
        {
            int digit1 = i >= 0 ? num1[i--] - '0' : 0;
            int digit2 = j >= 0 ? num2[j--] - '0' : 0;

            int sum = digit1 + digit2 + carry;
            carry = sum / Base;
            result.Insert(0, (sum % Base).ToString());
        }

        return result.ToString();
    }

    public static string SubtractStrings(string num1, string num2)
    {
        int comparison = CompareStrings(num1, num2);

        if (comparison == 0)
            return "0";

        bool resultIsNegative = false;
        string larger, smaller;

        if (comparison < 0)
        {
            larger = num2;
            smaller = num1;
            resultIsNegative = true;
        }
        else
        {
            larger = num1;
            smaller = num2;
            resultIsNegative = false;
        }

        StringBuilder result = new StringBuilder();
        int borrow = 0;
        int i = larger.Length - 1;
        int j = smaller.Length - 1;

        while (i >= 0)
        {
            int digit1 = larger[i--] - '0' - borrow;
            int digit2 = j >= 0 ? smaller[j--] - '0' : 0;

            borrow = 0;

            if (digit1 < digit2)
            {
                digit1 += Base;
                borrow = 1;
            }

            result.Insert(0, (digit1 - digit2).ToString());
        }

        string final = result.ToString().TrimStart('0');

        if (string.IsNullOrEmpty(final))
            return "0";

        return resultIsNegative ? "-" + final : final;
    }

    public static int CompareStrings(string num1, string num2)
    {
        string trimmed1 = num1.TrimStart('0');
        string trimmed2 = num2.TrimStart('0');

        if (string.IsNullOrEmpty(trimmed1)) trimmed1 = "0";
        if (string.IsNullOrEmpty(trimmed2)) trimmed2 = "0";

        if (trimmed1.Length != trimmed2.Length)
            return trimmed1.Length.CompareTo(trimmed2.Length);

        for (int i = 0; i < trimmed1.Length; i++)
        {
            if (trimmed1[i] != trimmed2[i])
                return trimmed1[i].CompareTo(trimmed2[i]);
        }

        return 0;
    }

    public override string ToString()
    {
        return (isNegative ? "-" : "") + integerPart +
               (string.IsNullOrEmpty(fractionalPart) ? "" : "." + fractionalPart);
    }
}