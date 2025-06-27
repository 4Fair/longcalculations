using System;
using System.Text;

public static class BigDecimalExponentiation
{
    public static Func<string, string, string> AddStrings { get; private set; }
    public static Func<string, string, string> SubtractStrings { get; private set; }
    public static Func<string, string, string> MultiplyStrings { get; private set; }
    public static Func<string, string, int, (string Quotient, string Remainder)> DivideWithRemainder { get; private set; }
    public static Func<string, string, int> CompareStrings { get; private set; }

    public static void Initialize(
        Func<string, string, string> addStrings,
        Func<string, string, string> subtractStrings,
        Func<string, string, string> multiplyStrings,
        Func<string, string, int, (string Quotient, string Remainder)> divideWithRemainder,
        Func<string, string, int> compareStrings)
    {
        AddStrings = addStrings ?? throw new ArgumentNullException(nameof(addStrings));
        SubtractStrings = subtractStrings ?? throw new ArgumentNullException(nameof(subtractStrings));
        MultiplyStrings = multiplyStrings ?? throw new ArgumentNullException(nameof(multiplyStrings));
        DivideWithRemainder = divideWithRemainder ?? throw new ArgumentNullException(nameof(divideWithRemainder));
        CompareStrings = compareStrings ?? throw new ArgumentNullException(nameof(compareStrings));
    }

    public static string Pow(string baseNumber, string exponent, int precision = 20)
    {
        ValidateInitialization();

        baseNumber = NormalizeNumber(baseNumber);
        exponent = NormalizeNumber(exponent);

        // Обработка специальных случаев
        if (exponent == "0") return "1";
        if (baseNumber == "1") return "1";
        if (baseNumber == "0") return exponent.StartsWith("-") ?
            throw new DivideByZeroException("Нуль не можна звести до від'ємного степеня") : "0";

        var (expInt, expFrac) = SplitNumber(exponent);
        bool isNegative = expInt.StartsWith("-");
        expInt = expInt.TrimStart('-');

        string result = IntegerPow(baseNumber, expInt, precision);

        if (!string.IsNullOrEmpty(expFrac))
        {
            string fraction = "0." + expFrac;
            string denominator = GetDenominator(fraction);
            string numerator = MultiplyStrings(expFrac, denominator).Split('.')[0];

            string powNumerator = IntegerPow(baseNumber, numerator, precision + 4);
            string root = NthRoot(powNumerator, denominator, precision + 4);
            result = MultiplyStrings(result, root);
            result = TruncateToPrecision(result, precision);
        }

        return isNegative ? Reciprocal(result, precision) : result;
    }

    private static string IntegerPow(string baseNumber, string exponent, int precision)
    {
        if (exponent == "0") return "1";
        if (baseNumber == "0") return "0";

        string result = "1";
        string currentBase = baseNumber;
        string remainingExp = exponent;

        while (CompareStrings(remainingExp, "0") > 0)
        {
            var (quotient, remainder) = DivideWithRemainder(remainingExp, "2", 0);

            if (remainder == "1")
            {
                result = MultiplyStrings(result, currentBase);
                result = TruncateToPrecision(result, precision);
            }

            currentBase = MultiplyStrings(currentBase, currentBase);
            currentBase = TruncateToPrecision(currentBase, precision);
            remainingExp = quotient;
        }

        return result;
    }

    private static string NthRoot(string number, string n, int precision)
    {
        if (number == "0") return "0";
        if (n == "1") return number;

        int iterations = (int)Math.Ceiling(Math.Log(precision + 1) * 1.5) + 5;
        string approx = InitialRootApproximation(number, n, precision);

        for (int i = 0; i < iterations; i++)
        {
            var (term1, _) = DivideWithRemainder(number, IntegerPow(approx, SubtractStrings(n, "1"), precision + 4), precision + 4);
            string term2 = MultiplyStrings(SubtractStrings(n, "1"), approx);
            var (newApprox, _) = DivideWithRemainder(AddStrings(term1, term2), n, precision + 4);

            if (CompareStrings(TruncateToPrecision(approx, precision), TruncateToPrecision(newApprox, precision)) == 0)
                break;

            approx = newApprox;
        }

        return TruncateToPrecision(approx, precision);
    }

    private static string InitialRootApproximation(string number, string n, int precision)
    {
        var (result, _) = DivideWithRemainder(AddStrings(number, n), "2", precision);
        return result;
    }

    private static string GetDenominator(string fraction)
    {
        string numerator = fraction.Split('.')[1].TrimEnd('0');
        return "1" + new string('0', numerator.Length);
    }

    private static string Reciprocal(string number, int precision)
    {
        var (result, _) = DivideWithRemainder("1", number, precision);
        return result;
    }

    private static void ValidateInitialization()
    {
        if (AddStrings == null || SubtractStrings == null || MultiplyStrings == null ||
            DivideWithRemainder == null || CompareStrings == null)
        {
            throw new InvalidOperationException("Всі додаткові методи повинні буди спочатку ініційовані.");
        }
    }

    private static string NormalizeNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number)) return "0";

        bool isNegative = number.StartsWith("-");
        string num = isNegative ? number.Substring(1) : number;

        string[] parts = num.Split('.');
        string intPart = parts[0].TrimStart('0');
        if (string.IsNullOrEmpty(intPart)) intPart = "0";
        string fracPart = parts.Length > 1 ? parts[1].TrimEnd('0') : "";

        string normalized = intPart;
        if (!string.IsNullOrEmpty(fracPart)) normalized += "." + fracPart;

        return isNegative ? "-" + normalized : normalized;
    }

    private static (string intPart, string fracPart) SplitNumber(string number)
    {
        string[] parts = number.Split('.');
        string intPart = parts[0];
        if (string.IsNullOrEmpty(intPart)) intPart = "0";
        string fracPart = parts.Length > 1 ? parts[1].TrimEnd('0') : "";
        return (intPart, fracPart);
    }

    private static string TruncateToPrecision(string number, int precision)
    {
        if (!number.Contains('.') || precision <= 0)
            return number.Split('.')[0];

        string[] parts = number.Split('.');
        string frac = parts[1].Length > precision ? parts[1].Substring(0, precision) : parts[1];
        frac = frac.TrimEnd('0');
        return string.IsNullOrEmpty(frac) ? parts[0] : $"{parts[0]}.{frac}";
    }
}