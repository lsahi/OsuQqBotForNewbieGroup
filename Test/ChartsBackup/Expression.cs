﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OsuQqBot.Charts
{
    public class Expression<T>
    {
        private readonly string _expression;
        private readonly IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> _operatorsMap;
        private readonly IReadOnlyDictionary<string, Func<T, double>> _valuesMap;
        private readonly IEnumerable<string> _Rpn;

        /// <summary>
        /// 给一个表达式，指定如何处理变量和运算符。
        /// </summary>
        /// <param name="expression">计算的表达式。</param>
        /// <param name="operatorsMap">操作符。不能包括字母、数字、下划线、括号以及空白字符。</param>
        /// <param name="valuesMap">值。字母或下划线开头，可以包括字母数字下划线。</param>
        /// <exception cref="FormatException">表达式出现任何问题都会抛出 <see cref="FormatException"/>。</exception>
        public Expression(string expression, IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> operatorsMap, IReadOnlyDictionary<string, Func<T, double>> valuesMap)
        {
            _expression = expression;
            _operatorsMap = new Dictionary<string, (Func<double, double, double> function, int priority)>(operatorsMap);
            _valuesMap = new Dictionary<string, Func<T, double>>(valuesMap);
            var data = Init();
            _Rpn = data.Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        /// <exception cref="InvalidOperationException">如果抛出异常，说明有 bug。</exception>
        /// <returns></returns>
        public double Evaluate(T arg) => Evaluate(_Rpn, _operatorsMap, _valuesMap, arg);

        private ProcessingData Init()
        {
            var processingData = new ProcessingData(_operatorsMap, _valuesMap);
            ProcessExpression(_expression, processingData);
            return processingData;
        }

        private static double Evaluate(IEnumerable<string> rpn, IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> operatorsMap, IReadOnlyDictionary<string, Func<T, double>> valuesMap, T arg)
        {
            var stack = new Stack<double>();
            foreach (string item in rpn)
            {
                if (TryValue(item, valuesMap, arg, out double value))
                {
                    stack.Push(value);
                    continue;
                }
                if (TryOperator(item, operatorsMap, out Func<double, double, double> function))
                {
                    double y = stack.Pop(), x = stack.Pop(); // Error if there are less than 2 values in stack.
                    stack.Push(function(x, y));
                    continue;
                }
                throw new InvalidOperationException("Invalid operation.");
            }
            double result = stack.Pop();
            if (stack.TryPop(out _)) throw new InvalidOperationException("Stack should be empty.");
            return result;
        }

        private static bool TryValue(string item, IReadOnlyDictionary<string, Func<T, double>> valuesMap, T arg, out double result)
        {
            if (double.TryParse(item, out double num))
            {
                result = num;
                return true;
            }
            if (valuesMap.TryGetValue(item, out Func<T, double> func))
            {
                result = func(arg);
                return true;
            }
            result = default(double);
            return false;
        }

        private static bool TryOperator(string item, IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> operatorsMap, out Func<double, double, double> function)
        {
            bool success = operatorsMap.TryGetValue(item, out var functionInfo);
            function = functionInfo.function;
            return success;
        }

        /// <summary>
        /// 处理表达式。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        private static void ProcessExpression(string expression, ProcessingData data)
        {
            int index = 0;
            ProcessExpression(expression, data, ref index);

            if (index < expression.Length) throw new FormatException("Analyzing error. Most possibly you lost an open parenthesis in the beginning.");

            data.Complete();
        }
        private static void ProcessExpression(string expression, ProcessingData data, ref int index)
        {
            data.Mark();
            while (index < expression.Length)
            {
                string next;

                // want: value or "("
                next = NextValueOrOpenParenthesis(expression, ref index);

                // 括号
                if (next == null) ProcessExpression(expression, data, ref index);
                // 数字入表达式
                else data.FeedValue(next);

                // want: ")" or symbol or end
                next = NextSymbolOrEnd(expression, ref index);

                // 括号或结束
                if (next == null)
                {
                    data.Recycle();
                    break;
                }
                // 入栈
                else data.SmartPush(next);

                // want: value or "("
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <exception cref="FormatException">下一项不是数字、变量或左括号，或者读取已经结束。</exception>
        /// <returns>该数字或变量；如果为左括号，则为 <c>null</c>。</returns>
        private static string NextValueOrOpenParenthesis(string expression, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(expression, ref index);

            if (string.IsNullOrEmpty(next)) throw new FormatException("The expression ends, but we expect a number, a variable or an open parenthesis.");

            if (next.Equals("(", StringComparison.Ordinal)) return null;

            return IsValue(next) ? next : throw new FormatException($"Unexpected symbol at {oldIndex}.");
        }

        // 转换出问题的时候考虑改用下面的方法。
        // private static bool IsOkayForValuePosition(string subExpression) => subExpression == "(" || !string.IsNullOrEmpty(subExpression) && IsValue(subExpression);

        private static bool IsValue(string next) => char.IsLetterOrDigit(next[0]) || next[0] == '.' || next[0] == '_';

        /// <summary>
        /// 读下一个符号，或者后括号，或者结束。
        /// </summary>
        /// <exception cref="FormatException">读到了值或者前括号。</exception>
        /// <returns>如果是后括号或者结束，<c>null</c>；否则，下一个符号。</returns>
        private static string NextSymbolOrEnd(string expression, ref int index)
        {
            int oldIndex = index;

            string next = ReadNext(expression, ref index);

            if (string.IsNullOrEmpty(next) || next == ")") return null;

            return !(IsValue(next) || next == "(") ? next : throw new FormatException($"Unexpected number or open parenthesis at {oldIndex}.");
        }

        /// <summary>
        /// 从指定索引开始读取下一项。
        /// </summary>
        /// <exception cref="FormatException"></exception>
        /// <returns>如果是末尾，则返回<c>null</c>；否则返回下一项。</returns>
        private static string ReadNext(string expression, ref int index)
        {
            index = SkipWhiteSpace(expression, index);

            if (index >= expression.Length) return null;

            var match = Regex.Match(expression.Substring(index),
                @"^(?:" +
                @"(?:[\d]+\.?|\.)[\d]*|" /* 是数字的情况 */ +
                @"\w+|" /* 字母 */ +
                @"\(|\)|" /* 左右括号 */ +
                @"[^\w\(\)\s]+" /* 其他 */ +
                @")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!match.Success) throw new FormatException($"Invalid expression. First error is at {index}.");

            index += match.Value.Length;
            return match.Value;
        }

        private static int SkipWhiteSpace(string s, int index)
        {
            while (index < s.Length && char.IsWhiteSpace(s[index]))
            {
                index++;
            }

            return index;
        }

        private class ProcessingData
        {
            private readonly IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> _operators;
            private readonly IReadOnlyDictionary<string, Func<T, double>> _variablesMap;
            private int _operatorsCount = 0;
            private int _variablesCount = 0;

            private readonly Stack<string> _stack = new Stack<string>();
            private readonly LinkedList<string> _expression = new LinkedList<string>();

            private const string Flag = "_stack_"; // flag for stack.

            public ProcessingData(IReadOnlyDictionary<string, (Func<double, double, double> function, int priority)> operators, IReadOnlyDictionary<string, Func<T, double>> variablesMap)
            {
                _operators = operators;
                _variablesMap = variablesMap;
            }

            public IEnumerable<string> Result => _expression;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="symbol"></param>
            /// <exception cref="FormatException"><c>symbol</c> 不是合法的运算符。</exception>
            public void SmartPush(string symbol)
            {
                // TODO: 阻止操作符数量超过操作数的数量。
                if (!_operators.TryGetValue(symbol, out var oInfo)) throw new FormatException($"Invalid operator {symbol}.");
                int newPriority = oInfo.priority;
                while (_stack.TryPeek(out string top) && top != Flag && _operators[top].priority >= newPriority)
                {
                    _stack.Pop();
                    _expression.AddLast(top);
                }
                _stack.Push(symbol);
                _operatorsCount++;
            }

            public void FeedValue(string value)
            {
                if (!double.TryParse(value, out _) && !_variablesMap.ContainsKey(value)) throw new FormatException("Invalid value or varaiable name.");

                _expression.AddLast(value);
                _variablesCount++;
            }

            public void Mark() => _stack.Push(Flag);

            public void Recycle()
            {
                while (_stack.TryPop(out string top) && top != Flag) _expression.AddLast(top);
            }

            public void Complete()
            {
                Recycle();
                if (_stack.TryPeek(out _)) throw new FormatException("Stack error.");
            }
        }
    }
}
