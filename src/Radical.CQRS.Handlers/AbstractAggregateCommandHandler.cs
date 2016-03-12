using Jason.Handlers.Commands;
using Radical.CQRS.Messages;

namespace Radical.CQRS.Handlers
{
    public abstract class AbstractAggregateCommandHandler<TAggregate, TCommand>
        : AbstractCommandHandler<TCommand>
        where TAggregate : IAggregate
        where TCommand : class, IAggregateCommand
    {
        public IRepositoryFactory RepositoryFactory { get; set; }

        protected override object OnExecute(TCommand command)
        {
            using(var session = this.RepositoryFactory.OpenSession())
            {
                var aggregate = session.GetById<TAggregate>(command.Id, command.Version);
                this.Manipulate(aggregate, command);
                session.CommitChanges();

                return aggregate.Id;
            }
        }

        public abstract void Manipulate(TAggregate aggregate, TCommand command);
    }
}
