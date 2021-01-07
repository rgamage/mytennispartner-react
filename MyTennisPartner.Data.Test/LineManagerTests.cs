using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Data.Managers;
using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.Enums;
using Microsoft.Extensions.Logging;
using MyTennisPartner.Models.ViewModels;

namespace MyTennisPartner.Data.Test
{
    [TestClass]
    public class LineManagerTests: ManagerTestBase, IDisposable
    {
        private LineManager _lineManager;
        private readonly ILogger<LineManager> logger;
        private MemberManager _memberManager;
        private readonly ILogger<MemberManager> memberLogger;

        public LineManagerTests(): base() {
            logger = new Moq.Mock<ILogger<LineManager>>().Object;
            memberLogger = new Moq.Mock<ILogger<MemberManager>>().Object;
        }

        [TestMethod]
        public void CanGetMatchLines()
        {
            Seed(SeedOptions.WithPlayersInLineup);
            using (var context = new TennisContext(Options))
            {
                _memberManager = new MemberManager(context, memberLogger);
                _lineManager = new LineManager(context, logger, _memberManager);
                var result = _lineManager.GetLinesByMatchAsync(Match1.MatchId);                 
            }
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
                    if (_lineManager != null) _lineManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LineManagerTests()
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
