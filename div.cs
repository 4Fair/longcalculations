using System;
using System.Text;

public static class BigDecimalDivision
{
    private static Func<string, string, int> CompareStrings;
    private static Func<string, string, string> SubtractStrings;
    private static Func<string, string, string> AddStrings;

    public static void Initialize(
        Func<string, string, int> compareMethod,
        Func<string, string, string> subtractMethod,
        Func<string, string, string> addMethod)
    {
        CompareStrings = compareMethod ?? throw new ArgumentNullException(nameof(compareMethod));
        SubtractStrings = subtractMethod ?? throw new ArgumentNullException(nameof(subtractMethod));
        AddStrings = addMethod ?? throw new ArgumentNullException(nameof(addMethod));
    }

    public static (string Quotient, string Remainder) DivideWithRemainder(string dividend, string divisor, int precision = 10)
    {
        if (CompareStrings == null || SubtractStrings == null || AddStrings == null)
        {
            throw new InvalidOperationException("Всі додаткові методи повинні бути ініціалізовані.");
        }

        var (dividendSign, dividendInt, dividendFrac) = ParseNumber(dividend);
        var (divisorSign, divisorInt, divisorFrac) = ParseNumber(divisor);

        int dividendScale = dividendFrac.Length;
        int divisorScale = divisorFrac.Length;
        int maxScale = Math.Max(dividendScale, divisorScale);

        string scaledDividend = dividendInt + dividendFrac.PadRight(maxScale, '0');
        string scaledDivisor = divisorInt + divisorFrac.PadRight(maxScale, '0');

        if (precision > 0)
        {
            scaledDividend += new string('0', precision);
            scaledDivisor += new string('0', precision);
        }

        scaledDividend = scaledDividend.TrimStart('0');
        scaledDivisor = scaledDivisor.TrimStart('0');

        if (string.IsNullOrEmpty(scaledDividend)) scaledDividend = "0";
        if (string.IsNullOrEmpty(scaledDivisor)) scaledDivisor = "0";

        if (scaledDivisor == "0") throw new DivideByZeroException();

        bool resultIsNegative = dividendSign ^ divisorSign;

        var (integerQuotient, integerRemainder) = PerformIntegerDivision(
            scaledDividend,
            scaledDivisor);

        integerRemainder = integerRemainder.TrimStart('-');

        string quotient = FormatDecimal(integerQuotient, maxScale, precision, resultIsNegative);
        string remainder = FormatDecimal(integerRemainder, maxScale, precision, dividendSign);

        return (quotient, remainder);
    }

    private static (bool isNegative, string intPart, string fracPart) ParseNumber(string number)
    {
        bool isNegative = number.StartsWith("-");
        string num = isNegative ? number.Substring(1) : number;

        string[] parts = num.Split('.');
        string intPart = parts[0].TrimStart('0');
        string fracPart = parts.Length > 1 ? parts[1].TrimEnd('0') : "";

        if (string.IsNullOrEmpty(intPart)) intPart = "0";

        return (isNegative, intPart, fracPart);
    }

    private static (string Quotient, string Remainder) PerformIntegerDivision(string dividend, string divisor)
    {
        if (divisor == "1") return (dividend, "0");

        int comparison = CompareStrings(dividend, divisor);
        if (comparison < 0) return ("0", dividend);
        if (comparison == 0) return ("1", "0");

        StringBuilder quotient = new StringBuilder();
        string currentRemainder = "0";

        for (int i = 0; i < dividend.Length; i++)
        {
            currentRemainder = currentRemainder == "0"
                ? dividend[i].ToString()
                : currentRemainder + dividend[i];

            int digit = 0;
            while (CompareStrings(currentRemainder, divisor) >= 0)
            {
                currentRemainder = SubtractStrings(currentRemainder, divisor);
                digit++;
            }
            quotient.Append(digit);
        }

        string quotientStr = quotient.ToString().TrimStart('0');
        string remainderStr = currentRemainder.TrimStart('0');

        return (
            string.IsNullOrEmpty(quotientStr) ? "0" : quotientStr,
            string.IsNullOrEmpty(remainderStr) ? "0" : remainderStr
        );
    }

    private static string FormatDecimal(string number, int scale, int precision, bool isNegative)
    {
        if (number == "0") return "0";

        if (precision == 0)
        {
            return isNegative ? "-" + number : number;
        }

        // Точка с учётом ебаного масштаба
        int decimalPos = number.Length - scale;
        if (decimalPos <= 0)
        {
            number = number.PadLeft(scale + 1, '0');
            decimalPos = 1;
        }

        string result = number.Insert(decimalPos, ".");
        result = result.TrimEnd('0').TrimEnd('.');

        if (result.Contains('.'))
        {
            int currentPrecision = result.Length - result.IndexOf('.') - 1;
            if (currentPrecision < precision)
            {
                result = result.PadRight(result.Length + (precision - currentPrecision), '0');
            }
        }
        else if (precision > 0)
        {
            result += "." + new string('0', precision);
        }

        return isNegative ? "-" + result : result;
    }
}