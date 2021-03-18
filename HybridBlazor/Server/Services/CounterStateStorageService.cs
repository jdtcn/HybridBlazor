using System;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HybridBlazor.Shared;
using HybridBlazor.Server.Data;
using HybridBlazor.Server.Data.Models;

namespace HybridBlazor.Server.Services
{
    public class CounterStateStorageService
    {
        protected readonly IServiceProvider serviceProvider;

        public ConcurrentDictionary<string, BehaviorSubject<CounterState>> States { get; set; } = new();

        public CounterStateStorageService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public BehaviorSubject<CounterState> GetCounterState()
        {
            var userId = GetCurrentUserId();

            var counter = GetSetPersistedCounterState();

            lock (this)
            {
                if (!States.ContainsKey(userId))
                {
                    States[userId] = new BehaviorSubject<CounterState>(
                        new CounterState
                        {
                            Count = counter.Count
                        });
                }
            }

            return States[userId];
        }

        public Task SetCounterState(CounterState state)
        {
            var userId = GetCurrentUserId();

            lock (this)
            {
                if (States.ContainsKey(userId))
                {
                    States[userId].OnNext(state);
                }
                else
                {
                    States[userId] = new BehaviorSubject<CounterState>(state);
                }
            }

            GetSetPersistedCounterState(state.Count);

            return Task.CompletedTask;
        }

        private Counter GetSetPersistedCounterState(int? newState = null)
        {
            var userId = GetCurrentUserId();

            lock (this)
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                var user = context.Users.FirstOrDefault(u => u.Id == userId);
                var counter = user?.Counter;
                if (counter == null) counter = context.Counters.FirstOrDefault(c => c.AnonymousId == userId);
                if (counter == null)
                {
                    counter = new Counter { AnonymousId = userId, User = user };
                    context.Add(counter);
                    context.SaveChanges();
                }

                if (newState.HasValue)
                {
                    counter.Count = newState.Value;
                    context.SaveChanges();
                }

                return counter;
            }
        }

        private string GetCurrentUserId()
        {
            var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            var context = httpContextAccessor?.HttpContext;

            var user = context.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            if (isAuthenticated)
            {
                return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            }

            var hasIdCookie = context.Request.Cookies.TryGetValue("hubrid-instance-id", out string anonymousUserIdCookie);
            return hasIdCookie
                ? anonymousUserIdCookie
                : Guid.NewGuid().ToString();
        }
    }
}
