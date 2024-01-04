using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.token.core.Helpers;

public class TimeHelper
{
    public static int ConvertStrDurationToSeconds(string durationIfo)
    {
        var durationType = durationIfo.Last();
        var numericPart = durationIfo.Substring(0, durationIfo.Length - 1);
        switch (durationType)
        {
            case 'm':
                return Convert.ToInt32(numericPart) * 60;
            case 'h':
                return Convert.ToInt32(numericPart) * 3600;
            case 's':
                return Convert.ToInt32(numericPart);
            default:
                break;
        }
        throw new FormatException();
    }
}
