using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HybridBlazor.Shared;
using HybridBlazor.Shared.Services;
using Grpc.Core;

namespace HybridBlazor.Server.Services
{
    public class CounterService : ICounterService
    {
        protected readonly CounterStateStorageService storage;

        public CounterService(CounterStateStorageService storage)
        {
            this.storage = storage;
        }

        public IAsyncEnumerable<CounterState> SubscribeAsync()
        {
            return storage.GetCounterState().ToAsyncEnumerable();
        }

        public Task Increment()
        {
            var currentState = storage.GetCounterState();

            currentState.Value.Count++;

            return storage.SetCounterState(currentState.Value);
        }

        public Task Decrement()
        {
            var currentState = storage.GetCounterState();

            currentState.Value.Count--;

            return storage.SetCounterState(currentState.Value);
        }

        public Task ThrowException()
        {
            var rng = new Random();
            object _ = rng.Next(0, 7) switch
            {
                0 => throw new Exception("Exception"),
                1 => throw new ArgumentException("Argument exception"),
                2 => throw new ArithmeticException("Arithmetic exception"),
                3 => throw new IndexOutOfRangeException("IndexOutOfRange exception"),
                4 => throw new RpcException(new Status(StatusCode.AlreadyExists, "Example AlreadyExists RpcException")),
                5 => throw new RpcException(new Status(StatusCode.InvalidArgument, "Example InvalidArgument RpcException")),
                _ => throw new RpcException(new Status(StatusCode.FailedPrecondition, "Example FailedPrecondition RpcException")),
            };

            return Task.CompletedTask;
        }
    }
}
