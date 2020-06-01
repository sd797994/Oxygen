using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.CsharpClientAgent
{
    /// <summary>
    /// actor服务标记物
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ActorServiceAttribute : Attribute
    {
        /// <summary>
        /// 自动保存
        /// </summary>
        public bool AutoSave { get; set; }
        public ActorServiceAttribute(bool AutoSave)
        {
            this.AutoSave = AutoSave;
        }
    }
}
