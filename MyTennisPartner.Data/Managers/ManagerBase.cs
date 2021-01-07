using Microsoft.Extensions.Logging;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Data.Managers
{
    /// <summary>
    /// base class for data managers
    /// </summary>
    public class ManagerBase: IDisposable
    {
        public List<NotificationEvent> NotificationEvents { get;  }

        protected ILogger Logger { get; }

        protected TennisContext Context { get; }

        public ManagerBase(TennisContext context, ILogger<ManagerBase> logger)
        {
            Context = context;
            Logger = logger;
            NotificationEvents = new List<NotificationEvent>();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ManagerBase()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
