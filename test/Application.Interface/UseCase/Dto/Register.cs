using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface.UseCase.Dto
{
    public class RegisterInput: ActorModel
    {
        public string UserName { get; set; }
        public override string Key { get; set; }
    }
}
