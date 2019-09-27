using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Application.Interface;

namespace Application.Service
{
    public class UseCaseBase
    {
        public async Task<ApplicationBaseResult> DoAsync(Func<ApplicationBaseResult, Task> runMethod)
        {
            var result = new ApplicationBaseResult();
            try
            {
                await runMethod(result);
                result.Code = 0;
            }
            catch (Exception e)
            {
                result.Message = "出错了,请稍后再试";
            }
            return result;
        }
    }
}
