using RepoPatternAndJwt.Core.Models;
using RepoPatternAndJwt.EF.Reopsitories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoPatternAndJwt.Core.RepositoriesInterFace
{
    public interface IUnitOfWork : IDisposable
    {
        public int Complete();
    }
}
