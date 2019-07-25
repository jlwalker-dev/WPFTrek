using System;
using System.Threading;

/*
 * StackOverflow to the rescue
 *      questions/8546/is-there-a-try-to-lock-skip-if-timeed-out-operation
 * 
 * lock() does not provide any type of control on what to do if
 * there is a conflct.
 *
 * The following can be used to retry until successful lock
 * ============================================================== 
 * var obj = new object();
 * 
 * while(true) 
 * {
 *      using(var tryLock = new TryLock(obj)) 
 *      {
 *          if(tryLock.HasLock)
 *          {
 *              ... do your thing
 *              break;
 *          }
 *      }
 *  }
 *  
 */
namespace WPFTrek.Utilities
{
    class TryLock : IDisposable
    {
        private object locked;
        public bool HasLock { get; private set; }

        public TryLock(object obj)
        {
            if(Monitor.TryEnter(obj))
            {
                HasLock = true;
                locked = obj;
            }
        }

        public void Dispose()
        {
            if(HasLock)
            {
                Monitor.Exit(locked);
                locked = null;
                HasLock = false;
            }
        }
    }
}
