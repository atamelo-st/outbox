using Microsoft.AspNetCore.Mvc;
using OutboxSample.Application;

namespace OutboxSample.Controllers;

public abstract class ApplicationControllerBase : ControllerBase
{
    protected IUnitOfWorkFactory UnitOfWork { get; }

    protected ApplicationControllerBase(IUnitOfWorkFactory unitOfWorkFactory)
    {
        this.UnitOfWork = unitOfWorkFactory;
    }
}
