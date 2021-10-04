using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TeamCloud
{
    internal static class GlobalExtensions
    {
        internal static void Add(this List<Task> tasks, Func<Task> callback)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            if (callback is null)
                throw new ArgumentNullException(nameof(callback));

            tasks.Add(callback());
        }

        internal static void WaitAll(this IEnumerable<Task> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            if (tasks.Any())
                Task.WaitAll(tasks.ToArray());
        }

        internal static Task WhenAll(this IEnumerable<Task> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            return tasks.Any()
                ? Task.WhenAll(tasks)
                : Task.CompletedTask;
        }

        internal static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            return tasks.Any()
                ? Task.WhenAll(tasks)
                : Task.FromResult(Array.Empty<T>());
        }

        internal static async IAsyncEnumerable<T> WhenAllAsync<T>(this IEnumerable<Task<T>> tasks)
        {
            if (tasks is null)
                throw new ArgumentNullException(nameof(tasks));

            if (tasks.Any())
            {
                var tasksMaterialized = tasks.ToList();

                while (tasksMaterialized.Any())
                {
                    var task = await Task
                        .WhenAny(tasksMaterialized)
                        .ConfigureAwait(false);

                    yield return await task
                        .ConfigureAwait(false);

                    tasksMaterialized.Remove(task);
                }
            }
        }

        internal static Guid ToGuid(this string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (Guid.TryParse(value, out var guid))
                return guid;

#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms

            var buffer = Encoding.UTF8.GetBytes(value);

            using var hasher = MD5.Create();

            return new Guid(hasher.ComputeHash(buffer));

#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
        }

        internal static Guid Combine(this Guid instance, Guid value, params Guid[] additionalValues)
        {
            var buffer = value.ToByteArray();

            var result = new Guid(instance.ToByteArray()
                .Select((b, i) => (byte)(b ^ buffer[i]))
                .ToArray());

            return additionalValues.Any()
                ? result.Combine(additionalValues.First(), additionalValues.Skip(1).ToArray())
                : result;
        }

        internal static bool IsGuid(this string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return Guid.TryParse(value, out var _);
        }

        private static readonly Regex EMailExpression = new Regex(
            @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        internal static bool IsEMail(this string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return EMailExpression.Match(value).Length > 0;
        }

        private static readonly Regex EnumPrettyPrintExpression = new Regex(
            @"\B[A-Z]",
            RegexOptions.Compiled);

        private static readonly string[] WellknownCompounds = new[]
        {
            "GitHub",
            "DevOps"
        };

        internal static string ToString(this Enum instance, bool prettyPrint, params string[] compounds)
        {
            var output = instance.ToString();

            if (prettyPrint)
            {
                output = EnumPrettyPrintExpression.Replace(output, " $0").Trim();

                WellknownCompounds
                    .Union(compounds)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .OrderByDescending(c => c)
                    .ToList()
                    .ForEach(c =>
                    {
                        var compoundSanitized = EnumPrettyPrintExpression.Replace(c, " $0").Trim();
                        output = output.Replace(compoundSanitized, c, StringComparison.Ordinal);
                    });
            }

            return output;
        }
    }
}
