﻿namespace NServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Pipeline;
    using Transports;

    class AttachCorrelationIdBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            var correlationId = context.Extensions.GetOrCreate<State>().CustomCorrelationId;

            //if we don't have a explicit correlation id set
            if (string.IsNullOrEmpty(correlationId))
            {
                IncomingMessage current;

                //try to get it from the incoming message
                if (context.TryGetIncomingPhysicalMessage(out current))
                {
                    string incomingCorrelationId;

                    if (current.Headers.TryGetValue(Headers.CorrelationId, out incomingCorrelationId))
                    {
                        correlationId = incomingCorrelationId;
                    }

                    if (string.IsNullOrEmpty(correlationId) && current.Headers.TryGetValue(Headers.MessageId, out incomingCorrelationId))
                    {
                        correlationId = incomingCorrelationId;
                    }
                }
            }

            //if we still doesn't have one we'll use the message id
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = context.MessageId;
            }

            context.Headers[Headers.CorrelationId] = correlationId;
            return next();
        }

        public class State
        {
            public string CustomCorrelationId { get; set; }
        }
    }
}