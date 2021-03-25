using Serilog;
using System;
using System.IO;
using Tinkoff.Trading.OpenApi.Network;

namespace TinkoffData
{
    public  class Auth
    {
        public SandboxContext GetSanboxContext()
        {
            Log.Information("Start GetSanboxContext");
            string token = File.ReadAllLines("toksan.dll")[0].Trim();
            var connection = ConnectionFactory.GetSandboxConnection(token);
            var context = connection.Context;
            Log.Information("Stop GetSanboxContext");
            return context;
        }

        public Context GetContext()
        {
            Log.Information("Start GetContext");
            string token = File.ReadAllLines("tokst.dll")[0].Trim();
            var connection = ConnectionFactory.GetConnection(token);
            var context = connection.Context;
            Log.Information("Stop GetContext");
            return context;
        }
    }
}

