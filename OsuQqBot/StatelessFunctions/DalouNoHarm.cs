﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OsuQqBot.QqBot;

namespace OsuQqBot.StatelessFunctions
{
    class DalouNoHarm : IStatelessFunction
    {
        const long dalou = 1061566571;

        static readonly ConcurrentQueue<(long group, string message)> queue;

        static readonly IReadOnlyCollection<long> valid = new List<long> { 514661057, 641236878 };

        static readonly ConcurrentDictionary<long, (string message, int num)> cache;

        static DalouNoHarm()
        {
            queue = new ConcurrentQueue<(long group, string message)>();
            cache = new ConcurrentDictionary<long, (string message, int num)>();
        }

        private static bool Update(long groupId, long fromQq, string message)
        {
            var nv = cache.AddOrUpdate(groupId, (message, 1), (g, p) =>
            {
                var (m, no) = p;
                if (m == message) return (m, ++no);
                return (message, 1);
            });
            if (nv.num == 4 && fromQq == dalou) return true;
            return false;
        }

        public bool ProcessMessage(EndPoint endPoint, MessageSource messageSource, string message)
        {
            if (!(endPoint is GroupEndPoint g)) return false;
            if (!valid.Any(group => group == g.GroupId)) return false;
            var result = Update(g.GroupId, messageSource.FromQq, message);
            if (result && message != "打断")
                OsuQqBot.QqApi.SendMessageAsync(endPoint, "打断");
            return false;
        }
    }
}