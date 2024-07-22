using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YSCore.Base.Events
{
    public interface IBaseCommand { }

    public interface ICommand : IRequest, IBaseCommand { }


    public interface ICommand<out TResponse> : IRequest<TResponse>, IBaseCommand { }
}
