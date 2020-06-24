using Oxygen.CsharpClientAgent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface.UseCase.Dto
{
    public class RegisterInput: ActorModel
    {
        [ActorKey]
        public string UserName { get; set; }
    }
}
