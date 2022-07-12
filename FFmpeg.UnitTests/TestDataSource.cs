using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HanumanInstitute.FFmpeg.UnitTests;

public static class TestDataSource
{
    /// <summary>
    /// Returns data to test null or empty string parameters.
    /// The test function should receive an additional argument of type Exception to know the exception type to test for.
    /// </summary>
    /// <param name="paramCount">The number of string parameters in the test.</param>
    /// <returns>The test input data.</returns>
    public static IEnumerable<object[]> NullAndEmptyStrings(int paramCount)
    {
        // Test null values.
        yield return CreateParamValues(paramCount, -1, true);
        if (paramCount > 1)
        {
            for (var i = 0; i < paramCount; i++)
            {
                yield return CreateParamValues(paramCount, i, true);
            }
        }

        // Test empty strings.
        yield return CreateParamValues(paramCount, -1, false);
        if (paramCount > 1)
        {
            for (var i = 0; i < paramCount; i++)
            {
                yield return CreateParamValues(paramCount, i, false);
            }
        }
    }

    /// <summary>
    /// Creates a unit test parameters value collection for testing null or empty. 
    /// The test function should receive an additional argument of type Exception to know the exception type to test for.
    /// </summary>
    /// <param name="paramCount">The number of string parameters in the test.</param>
    /// <param name="index">-1 to create with all null or empty, or the index of the value to be null or empty.</param>
    /// <param name="isNull">True to set null values, false to set empty values.</param>
    private static object[] CreateParamValues(int paramCount, int index, bool isNull)
    {
        var result = new object[paramCount + 1];
        if (index < 0)
        {
            Populate(result, isNull ? null : string.Empty);
        }
        else
        {
            Populate(result, "a");
            result[index] = isNull ? null : string.Empty;
        }
        result[paramCount] = isNull ? typeof(ArgumentNullException) : typeof(ArgumentException);
        return result;
    }

    private static void Populate<T>(T[] arr, T value)
    {
        for (var i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }
}