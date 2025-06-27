using System;

public static class KaratsubaMultiplier
{
    public static Func<string, string, string> AddStrings { get; set; }
    public static Func<string, string, string> SubtractStrings { get; set; }

    public static void Initialize(
        Func<string, string, string> addStrings,
        Func<string, string, string> subtractStrings)
    {
        AddStrings = addStrings;
        SubtractStrings = subtractStrings;
    }

    public static string Multiply(string num1, string num2)
    {
        if (AddStrings == null || SubtractStrings == null)
            throw new InvalidOperationException("Допоміжні методи не були ініційовані.");

        bool isNegative = (num1[0] == '-') ^ (num2[0] == '-');

        num1 = num1.TrimStart('-');
        num2 = num2.TrimStart('-');

        if (num1 == "0" || num2 == "0")
            return "0";

        string[] parts1 = num1.Split('.');
        string[] parts2 = num2.Split('.');

        string intPart1 = parts1[0].TrimStart('0');
        string fracPart1 = parts1.Length > 1 ? parts1[1].TrimEnd('0') : "";

        string intPart2 = parts2[0].TrimStart('0');
        string fracPart2 = parts2.Length > 1 ? parts2[1].TrimEnd('0') : "";

        if (fracPart1 == "" && fracPart2 == "")
        {
            string result = PureKaratsuba(intPart1, intPart2);
            return isNegative ? "-" + result : result;
        }

        int totalFracDigits = fracPart1.Length + fracPart2.Length;

        string x = intPart1 + fracPart1;
        string y = intPart2 + fracPart2;

        if (x == "") x = "0";
        if (y == "") y = "0";

        string product = PureKaratsuba(x, y);

        if (totalFracDigits > 0)
        {
            if (product.Length <= totalFracDigits)
                product = product.PadLeft(totalFracDigits + 1, '0');

            int decimalPos = product.Length - totalFracDigits;
            product = product.Insert(decimalPos, ".").TrimEnd('0').TrimEnd('.');
        }

        return isNegative ? "-" + product : product;
    }

    private static string PureKaratsuba(string x, string y)
    {
        int maxLength = Math.Max(x.Length, y.Length);
        x = x.PadLeft(maxLength, '0');
        y = y.PadLeft(maxLength, '0');

        if (maxLength == 1)
        {
            int product = (x[0] - '0') * (y[0] - '0');
            return product.ToString();
        }

        int half = maxLength / 2;
        int secondHalf = maxLength - half;

        string a = x.Substring(0, secondHalf);
        string b = x.Substring(secondHalf);
        string c = y.Substring(0, secondHalf);
        string d = y.Substring(secondHalf);

        // Рекурс. произведения
        string ac = PureKaratsuba(a, c);
        string bd = PureKaratsuba(b, d);

        // (a+b)*(c+d) - ac - bd = ad + bc
        string aPlusB = AddStrings(a, b);
        string cPlusD = AddStrings(c, d);
        string adPlusBc = SubtractStrings(PureKaratsuba(aPlusB, cPlusD), AddStrings(ac, bd));

        // ac*10^(2*half) + (ad+bc)*10^half + bd
        string term1 = ac + new string('0', 2 * half);
        string term2 = adPlusBc + new string('0', half);

        return AddStrings(AddStrings(term1, term2), bd);
    }
}