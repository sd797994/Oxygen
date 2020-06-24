using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public abstract class ActorModel
    {
        public ActorModel()
        {
            bool hashKey = false;
            foreach (var property in this.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<ActorKeyAttribute>() != null)
                {
                    hashKey = true;
                    break;
                }
            }
            if (!hashKey)
                throw new ArgumentException("actormodel must have one actorkey!");
        }
        public string GetKey() {
            return (string)this.GetType().GetRuntimeProperties().First(x => x.GetCustomAttribute(typeof(ActorKeyAttribute)) != null).GetValue(this);
        }
        public bool SaveChanges = false;
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class ActorKeyAttribute : Attribute
    {

    }
}
