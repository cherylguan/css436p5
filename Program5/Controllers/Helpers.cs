using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Program5.Controllers
{
    internal class Helpers
    {
        public static void Retry(
            Action action,
            int retryCount = 1)
        {
            while (retryCount > 0)
            {
                try
                {
                    action.Invoke();
                    return;
                }
                catch (Exception e)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw e;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}