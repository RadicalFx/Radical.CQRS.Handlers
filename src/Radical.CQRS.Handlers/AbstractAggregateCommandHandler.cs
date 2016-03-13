using Jason.Handlers.Commands;
using Radical.CQRS.Messages;
using System;

namespace Radical.CQRS.Handlers
{
    public abstract class AbstractAggregateCommandHandler<TAggregate, TCommand>
        : AbstractCommandHandler<TCommand>
        where TAggregate : class, IAggregate
        where TCommand : class
    {
        public IRepositoryFactory RepositoryFactory { get; set; }

        protected override object OnExecute(TCommand command)
        {
            TAggregate aggregate = null;

            using(var session = this.RepositoryFactory.OpenSession())
            {
                var aggregateId = this.GetAggregateId(command);
                var aggregateVersion = this.GetAggregateVersion(command);
                if(aggregateVersion.HasValue)
                {
                    aggregate = session.GetById<TAggregate>(AggregateQuery.GetLatest(aggregateId, aggregateVersion.Value));
                }
                else
                {
                    aggregate = session.GetById<TAggregate>(aggregateId);
                }

                this.Manipulate(aggregate, command);
                session.CommitChanges();

                return aggregate.Id;
            }
        }

        protected virtual int? GetAggregateVersion(TCommand command)
        {
            var iac = command as IAggregateCommand;
            if(iac != null)
            {
                return iac.Version;
            }

            var pi = command.GetType().GetProperty("Version");
            if(pi != null)
            {
                return (int)pi.GetValue(command);
            }

            return null;
        }

        protected virtual Guid GetAggregateId(TCommand command)
        {
            var iac = command as IAggregateCommand;
            if(iac != null)
            {
                return iac.Id;
            }

            var pi = command.GetType().GetProperty("Id");
            if(pi != null)
            {
                return (Guid)pi.GetValue(command);
            }

            return Guid.Empty;
        }

        public abstract void Manipulate(TAggregate aggregate, TCommand command);
    }
}
