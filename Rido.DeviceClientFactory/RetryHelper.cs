
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rido
{
  public static class RetryHelper
  {

    public static async Task RetryOnExceptionAsync<TException>(int times, TimeSpan delay, Func<Task> operation) where TException : Exception
    {
      if (times <= 0)
        throw new ArgumentOutOfRangeException(nameof(times));

      var attempts = 0;
      do
      {
        try
        {
          attempts++;
          await operation();
          break;
        }
        catch (TException)
        {
          if (attempts == times)
            throw;

          await Task.Delay(delay.Add(TimeSpan.FromSeconds(attempts)));
        }
      } while (true);
 
     }
  }
}