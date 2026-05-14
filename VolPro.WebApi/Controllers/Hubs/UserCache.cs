using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace VolPro.WebApi.Controllers.Hubs
{
    public static class UserCache
    {
        public static ConcurrentDictionary<string, string> ConnectionIds = new ConcurrentDictionary<string, string>();

        public static ConcurrentDictionary<string, int> Online = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 根据用户名获取所有的客户端
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetCnnectionIds(string username)
        {
            foreach (var item in ConnectionIds)
            {
                if (item.Value == username)
                {
                    yield return item.Key;
                }
            }
        }

        public static int GetOnline(string username)
        {

            if (Online.TryGetValue(username, out int val))
            {
                return val;
            }
            return 0;
        }



        public static void Add(HubCallerContext context)
        {
            string userName = context.GetHttpContext().Request.Query["userName"].ToString();
            if (string.IsNullOrEmpty(userName))
            {
                return;
            }
            Online[userName] = 1;
            ConnectionIds[context.ConnectionId] = userName;
        }

        public static void Remove(HubCallerContext context)
        {
            var cid = context.ConnectionId;

            //移除缓存
            if (ConnectionIds.TryRemove(cid, out string value))
            {
                Online[value] = 0;
            }
        }
    }
}
