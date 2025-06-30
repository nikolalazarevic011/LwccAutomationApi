using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Data.AppData.Interfaces;

namespace WsRaisedHandsModern.Api.Data.AppData.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        // private readonly IMapper _mapper;
        public UnitOfWork(ApplicationDbContext context)
        {
            // _mapper = mapper;
            _context = context;
        }
        
        //uncomment when automapper is implemented
        //public IUserRepository UserRepository => new UserRepository(_context, _mapper);

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
        
    }
}