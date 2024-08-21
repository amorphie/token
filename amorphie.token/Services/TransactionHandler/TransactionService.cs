
using amorphie.token.data;
using Microsoft.EntityFrameworkCore;

namespace amorphie.token.Services.TransactionHandler
{
    public class TransactionService : ServiceBase, ITransactionService
    {
        private string _ip;
        public string IpAddress { get => _ip; set => _ip = value; }
        private Logon _logon;
        public Logon Logon { get => _logon; set => _logon = value; }
        private int _roleKey{get;set;}
        public int RoleKey { set => _roleKey = value; get => _roleKey;}
        private ClientResponse? _client{get;set;}
        public ClientResponse? Client { set => _client = value; get => _client;}
        private DatabaseContext _databaseContext;

        public TransactionService(ILogger<TransactionService> logger, IConfiguration configuration, DatabaseContext databaseContext) : base(logger, configuration)
        {
            _databaseContext = databaseContext;
        }

        public async Task InitLogon(long instanceKey, long jobKey)
        {
            _logon = await _databaseContext.Logon.FirstOrDefaultAsync(l => l.WorkflowInstanceId == instanceKey);
            if (_logon == null)
            {
                _logon = new Logon
                {
                    WorkflowInstanceId = instanceKey,
                    LastJobKey = jobKey
                };
                await _databaseContext.Logon.AddAsync(_logon);
            }
            else
            {
                _logon.LastJobKey = jobKey;
            }
        }

        public async Task SaveLogon()
        {
            await _databaseContext.SaveChangesAsync();
        }
    }
}