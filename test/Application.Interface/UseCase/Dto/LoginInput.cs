using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface
{
    public class LoginInput : ActorModel
    {
        [ActorKey]
        public string UserName { get; set; }
    }
}
