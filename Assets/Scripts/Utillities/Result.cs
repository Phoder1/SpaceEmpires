﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoder1.Core.Result
{
    public delegate Result Operation();
    public delegate Result<T> Operation<T>();
    public readonly struct Result
    {
        public readonly string ErrorMessage;
        public static Result Success()
            => new Result(null);
        public static Result<T> Success<T>(T value)
            => Result<T>.Success(value);
        public static Result Failed(string errorMessage = null)
            => new Result(null);
        public static Result<T> Failed<T>(string errorMessage = null)
            => Result<T>.Failed(errorMessage);
        public Result(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        public bool Successful => string.IsNullOrEmpty(ErrorMessage);

        public static implicit operator bool(Result result)
            => result.Successful;
        public static implicit operator Result(bool result)
            => new Result(result ? null : "Failed");
    }
    public readonly struct Result<T>
    {
        public readonly string ErrorMessage;
        public readonly T Value;
        public static Result<T> Success(T value)
            => new Result<T>(null, value);
        public static Result<T> Failed(string errorMessage)
            => new Result<T>(errorMessage, default);

        public Result(string errorMessage, T value)
        {
            ErrorMessage = errorMessage;
            Value = value;
        }

        public bool Successful => string.IsNullOrEmpty(ErrorMessage);

        public static implicit operator bool(Result<T> result)
            => result.Successful;
        public static implicit operator T(Result<T> result)
            => result.Value;
        public static implicit operator Result(Result<T> result)
            => new Result(result.ErrorMessage);
    }

    public static class ResultExt
    {
        #region Run
        public static IEnumerable<Result> Run(this IEnumerable<Operation> operations)
        {
            foreach (var op in operations)
            {
                if (op == null)
                    continue;

                yield return op.Invoke();
            }
        }
        public static IEnumerable<Result<T>> Run<T>(this IEnumerable<Operation<T>> operations)
        {
            foreach (var op in operations)
            {
                if (op == null)
                    continue;

                yield return op.Invoke();
            }
        }
        #endregion
        #region Run until
        /// <summary>
        /// Runs through all results and returns the first matching one, null if none matched.
        /// </summary>
        public static Result? RunUntil(this IEnumerable<Result> results, Predicate<Result> predicate)
        {
            foreach (var result in results)
                if (predicate.Invoke(result))
                    return result;

            return null;
        }
        /// <summary>
        /// Runs through all results and returns the first matching one, null if none matched.
        /// </summary>
        public static Result<T>? RunUntil<T>(this IEnumerable<Result<T>> results, Predicate<Result<T>> predicate)
        {
            foreach (var result in results)
                if (predicate.Invoke(result))
                    return result;

            return null;
        }
        /// <summary>
        /// Runs through all results and returns the first matching one, null if none matched.
        /// </summary>
        public static Result? RunUntil(this IEnumerable<Operation> operations, Predicate<Result> predicate)
            => operations.Run().RunUntil(predicate);
        /// <summary>
        /// Runs through all results and returns the first matching one, null if none matched.
        /// </summary>
        public static Result<T>? RunUntil<T>(this IEnumerable<Operation<T>> operations, Predicate<Result<T>> predicate)
            => operations.Run().RunUntil(predicate);
        /// <summary>
        /// Runs through all results and returns the first failed one, success if none failed.
        /// </summary>
        public static Result RunUntilFailure(this IEnumerable<Result> results)
            => results.RunUntil((x) => !x) ?? Result.Success();
        /// <summary>
        /// Runs through all results and returns the first failed one, success if none failed.
        /// </summary>
        public static Result<T> RunUntilFailure<T>(this IEnumerable<Result<T>> results)
            => results.RunUntil((x) => !x) ?? Result<T>.Success(default);
        /// <summary>
        /// Runs through all results and returns the first failed one, success if none failed.
        /// </summary>
        public static Result RunUntilFailure(this IEnumerable<Operation> operations)
            => operations.RunUntil((x) => !x) ?? Result.Success();
        /// <summary>
        /// Runs through all results and returns the first failed one, success if none failed.
        /// </summary>
        public static Result<T> RunUntilFailure<T>(this IEnumerable<Operation<T>> operations)
            => operations.RunUntil((x) => !x) ?? Result<T>.Success(default);
        /// <summary>
        /// Runs through all results and returns the first successful one, failed if none were successful.
        /// </summary>
        public static Result RunUntilSuccess(this IEnumerable<Result> results)
            => results.RunUntil((x) => !x) ?? Result.Failed();
        /// <summary>
        /// Runs through all results and returns the first successful one, failed if none were successful.
        /// </summary>
        public static Result<T> RunUntilSuccess<T>(this IEnumerable<Result<T>> results)
            => results.RunUntil((x) => !x) ?? Result<T>.Failed("No matching result");
        /// <summary>
        /// Runs through all results and returns the first successful one, failed if none were successful.
        /// </summary>
        public static Result RunUntilSuccess(this IEnumerable<Operation> operations)
            => operations.RunUntil((x) => x) ?? Result.Failed();
        /// <summary>
        /// Runs through all results and returns the first successful one, failed if none were successful.
        /// </summary>
        public static Result<T> RunUntilSuccess<T>(this IEnumerable<Operation<T>> operations)
            => operations.RunUntil((x) => x) ?? Result<T>.Failed("No matching result");
        #endregion
        #region Ignore
        public static IEnumerable<Result> Ignore(this IEnumerable<Result> results, Predicate<Result> predicate)
        {
            foreach (var result in results)
                if (predicate.Invoke(result))
                    yield return result;
        }
        public static IEnumerable<Result<T>> Ignore<T>(this IEnumerable<Result<T>> results, Predicate<Result<T>> predicate)
        {
            if (predicate == null)
                throw new NullReferenceException(nameof(predicate));

            foreach (var result in results)
                if (predicate.Invoke(result))
                    yield return result;
        }
        public static IEnumerable<Result> IgnoreFailures(this IEnumerable<Result> results)
            => results.Ignore((x) => !x);
        public static IEnumerable<Result<T>> IgnoreFailures<T>(this IEnumerable<Result<T>> results)
            => results.Ignore((x) => !x);
        public static IEnumerable<Result> IgnoreSuccessful(this IEnumerable<Result> results)
            => results.Ignore((x) => x);
        public static IEnumerable<Result<T>> IgnoreSuccessful<T>(this IEnumerable<Result<T>> results)
            => results.Ignore((x) => x);
        #endregion
        #region Run and ignore
        public static IEnumerable<Result> RunAndIgnore(this IEnumerable<Operation> operations, Predicate<Result> predicate)
            => operations.Run().Ignore(predicate);
        public static IEnumerable<Result<T>> RunAndIgnore<T>(this IEnumerable<Operation<T>> operations, Predicate<Result<T>> predicate)
            => operations.Run().Ignore(predicate);
        public static IEnumerable<Result> RunAndIgnoreFailure(this IEnumerable<Operation> operations)
            => operations.Run().IgnoreFailures();
        public static IEnumerable<Result<T>> RunAndIgnoreFailure<T>(this IEnumerable<Operation<T>> operations)
            => operations.Run().IgnoreFailures();
        public static IEnumerable<Result<T>> RunAndIgnoreSuccessful<T>(this IEnumerable<Operation<T>> operations)
            => operations.Run().IgnoreSuccessful();
        #endregion
        #region Converters
        public static IEnumerable<Result> ToBaseResult<T>(this IEnumerable<Result<T>> results)
            => results.Convert<Result<T>, Result>((x) => x);
        public static Result<T> WithValue<T>(this Result result, Func<T> value)
        {
            if (result)
                return Result<T>.Success(value.Invoke());
            else
                return Result<T>.Failed(result.ErrorMessage);
        }
        public static IEnumerable<T> Values<T>(this IEnumerable<Result<T>> results)
        {
            foreach (var result in results)
                if (result)
                    yield return result;
        }
        public static IEnumerable<string> ErrorMessages<T>(this IEnumerable<Result<T>> results)
        {
            foreach (var result in results)
                if (!result)
                    yield return result.ErrorMessage;
        }
        public static IEnumerable<string> ErrorMessages(this IEnumerable<Result> results)
        {
            foreach (var result in results)
                if (!result)
                    yield return result.ErrorMessage;
        }
        public static int CountFailures(this IEnumerable<Result> results)
            => results.Count((x) => !x);
        public static int CountSuccessful(this IEnumerable<Result> results)
            => results.Count((x) => x);
        #endregion
        #region Asserts
        public static Result Assert(this bool result, string errorMessage)
            => result ? Result.Success() : Result.Failed(errorMessage);
        public static Result AssertNull<T>(this T obj, string errorMessage)
            where T : class
            => (obj != null).Assert(errorMessage);
        public static Result AssertNull<T>(this T? obj, string errorMessage)
            where T : struct
            => (obj.HasValue).Assert(errorMessage);
        public static Result AssertNullOrEmpty<T>(this IEnumerable<T> objects, string errorMessage)
            => (objects == null || !objects.Any()).Assert(errorMessage);
        #endregion
        #region Utillities
        public static void DebugLog(this Result result, ILogger logger, LogType logType = LogType.Log, string tag = null, bool forcePrint = false)
        {
            string msg = result.Successful.ToString();
            if (!result)
                msg += $" {result.ErrorMessage}";

            logger?.Log(msg, logType, tag, forcePrint);
        }
        public static void DebugLog<T>(this Result<T> result, ILogger logger, LogType logType = LogType.Log, string tag = null, bool forcePrint = false)
        {
            string msg = result.Successful.ToString();
            if (!result)
                msg += $" {result.ErrorMessage}";
            else
                msg += $" Result value: {result.Value}";

            logger?.Log(msg, logType, tag, forcePrint);
        }
        #endregion
    }
}