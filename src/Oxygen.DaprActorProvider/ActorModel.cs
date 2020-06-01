using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.DaprActorProvider
{
    public class ActorModel : INotification
    {
        public string ActorId { get; set; }
    }
}
