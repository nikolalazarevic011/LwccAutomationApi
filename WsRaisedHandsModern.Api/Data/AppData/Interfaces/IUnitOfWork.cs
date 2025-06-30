using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WsRaisedHandsModern.Api.Data.AppData.Interfaces
{
    public interface IUnitOfWork
    {
        //uncomment when automapper is implemented
        // IUserRepository UserRepository {get;}

        Task<bool> Complete();
        bool HasChanges();
    }
}